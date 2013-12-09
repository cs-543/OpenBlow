{-# LANGUAGE DeriveDataTypeable, TemplateHaskell, OverloadedStrings #-}
{-# LANGUAGE BangPatterns, ViewPatterns #-}

module Main ( main ) where

import qualified Data.Map as M
import qualified Data.Text as T
import qualified Data.Text.Encoding as T
import Control.Lens hiding ( (.=) )
import Control.Applicative
import Control.Monad ( forever )
import Control.Monad.IO.Class ( liftIO )
import Control.Concurrent
import Control.Concurrent.STM
import System.IO.Error
import Control.Exception
import Control.Monad.State.Strict
import Data.Typeable
import Data.Char ( ord )
import Data.Word
import Data.IORef
import Network
import Data.Maybe
import Data.Time.Clock
import Data.Aeson
import Data.Monoid
import Data.ByteString ( hGet, hPut )
import qualified Data.ByteString as B
import qualified Data.ByteString.Lazy as B ( fromStrict, toStrict )
import System.IO ( Handle, hClose, hFlush )
import System.IO.Unsafe ( unsafePerformIO )
import System.Environment

data Rule = Rule { _nextHop :: !T.Text
                 , _lifeTime :: Integer
                 , _expires :: UTCTime }
            deriving ( Eq, Ord, Show, Read, Typeable )

data AnyPacket = AnyPacket { _from :: !T.Text
                           , _to :: !T.Text
                           , _data :: Value }
                  deriving ( Eq, Show, Typeable )

data Packet = Packet { _flowID :: Integer
                     , _payload :: !T.Text
                     , _ttl :: Integer }
              deriving ( Eq, Ord, Show, Read, Typeable )

instance FromJSON AnyPacket where
    parseJSON (Object v) = AnyPacket <$>
                           v .: "from" <*>
                           v .: "to" <*>
                           v .: "data"

instance ToJSON AnyPacket where
    toJSON (AnyPacket from to dataload) = object ["from" .= from
                                                 ,"to" .= to
                                                 ,"data" .= dataload]

instance FromJSON Rule where
    parseJSON (Object v) = Rule <$>
                           v .: "next_hop" <*>
                           v .: "ttl" <*>
                           pure (unsafePerformIO getCurrentTime)

instance FromJSON Packet where
    parseJSON (Object v) = Packet <$>
                           v .: "flow_id" <*>
                           v .: "payload" <*>
                           v .: "ttl"

instance ToJSON Packet where
    toJSON (Packet flow_id payl ttl) =
        object ["flow_id" .= flow_id
               ,"payload" .= payl
               ,"ttl" .= ttl]

type SwitchConnections = MVar (M.Map T.Text Handle)

data SwitchTable = SwitchTable { _rules :: M.Map Integer Rule }
                   deriving ( Eq, Ord, Show, Read, Typeable )
makeLenses ''Rule
makeLenses ''SwitchTable
makeLenses ''Packet
makeLenses ''AnyPacket

stdoutLock :: MVar ()
stdoutLock = unsafePerformIO $ newMVar ()
{-# NOINLINE stdoutLock #-}

syncPutStrLn :: String -> IO ()
syncPutStrLn str = withMVar stdoutLock $ \_ -> putStrLn str

measureTime :: MonadIO m => String -> m a -> m a
measureTime measurement action = do
    now <- liftIO $ getCurrentTime
    result <- action
    end <- liftIO $ getCurrentTime
    liftIO $ syncPutStrLn $ "measurement " ++ measurement ++ ": " ++ (show $ end `diffUTCTime` now)
    return result

splitHost :: T.Text -> (T.Text, PortID)
splitHost host = over _2 (PortNumber . fromIntegral . read . T.unpack) $ toTuple $ T.split ((==) ':') host
  where
    toTuple [x, y] = (x, y)

main :: IO ()
main = do
    (me:controllers) <- (fmap T.pack) <$> getArgs
    listener <- listenOn (PortNumber 8888)
    sw <- newMVar M.empty
    controller_chan <- newTChanIO
    ref <- newIORef M.empty
    forkIO $ controllerHandler me controllers controller_chan ref
    forever $ fst3 <$> accept listener >>= forkIO . clientHandler me controller_chan sw
  where
    fst3 (x, _, _) = x

data Bailout = Bailout
               deriving ( Eq, Show, Typeable )

instance Exception Bailout

controllerHandler :: T.Text -> [T.Text] -> TChan (Integer, (MVar Rule)) -> IORef (M.Map Integer Rule) -> IO ()
controllerHandler me controller_hosts chan table_ref = forever $ do
    (fid, mvar) <- atomically $ readTChan chan

    table <- readIORef table_ref
    case M.lookup fid table of
        Nothing -> actuallyGetTheRule fid mvar
        Just rule -> do
            has_expired <- expired rule
            if has_expired
              then actuallyGetTheRule fid mvar
              else putMVar mvar rule
  where
    actuallyGetTheRule fid mvar = do
        measureTime "rule_activation_withtcp" $ do
            flip catch (\Bailout -> pure ()) $ do
                forM_ controller_hosts $ \host -> do
                    result <- try $ do
                        syncPutStrLn $ "Making connection to controller... (" ++ T.unpack host ++ ")"
                        let (actual_host, _) = splitHost host
                        handle <- connectTo (T.unpack actual_host) (PortNumber 8888)
                        syncPutStrLn "Done"

                        rule <- measureTime "rule_activation" $ do
                            hPut handle $ B.toStrict $
                                encode (AnyPacket { _from = me
                                                  , _to = host
                                                  , _data = toJSON (object ["flow_id" .= fid]) })
                            hFlush handle
                            anypacket <- readJSON handle
                            let Success rule = fromJSON $ _data anypacket :: Result Rule
                            syncPutStrLn $ "Controller sent me this rule: " ++ show rule
                            return rule

                        now <- getCurrentTime
                        let rule' = rule { _expires = fromIntegral (_lifeTime rule) `addUTCTime` now }
                        atomicModifyIORef' table_ref $ \old ->
                            ( M.insert fid rule' old, () )
                        putMVar mvar rule'
                        hClose handle
                    case result of
                        Left e -> (e :: SomeException) `seq` pure ()
                        Right _ -> throwIO Bailout

hGetByte :: Handle -> IO Word8
hGetByte h = B.head <$> hGet h 1

readJSON :: MonadIO m => Handle -> m AnyPacket
readJSON handle = liftIO $ do
    Just packet <- do
        untilFirstBrace handle
        content <- (B.singleton (fromIntegral $ ord '{') <>) <$> untilSecondBrace 1 B.empty handle
        syncPutStrLn $ T.unpack $ T.decodeUtf8 content
        return $ decode (B.fromStrict content)
    return packet

getByte :: MonadIO m => Handle -> m Word8
getByte handle = liftIO $ hGetByte handle
{-# INLINE getByte #-}

untilFirstBrace :: Handle -> IO ()
untilFirstBrace handle = do
    first_brace <- getByte handle
    unless (first_brace == fromIntegral (ord '{')) $ untilFirstBrace handle

untilSecondBrace :: Int -> B.ByteString -> Handle -> IO B.ByteString
untilSecondBrace 0 !accum handle = pure (B.reverse accum)
untilSecondBrace !depth !accum handle = do
    b <- getByte handle
    let consed = b `B.cons` accum
        next x = untilSecondBrace x consed handle
        b' = fromIntegral b

    next $ if b' == ord '{'
             then depth+1
             else if b' == ord '}'
               then depth-1
               else depth

clientHandler :: T.Text -> TChan (Integer, MVar Rule) -> SwitchConnections -> Handle -> IO ()
clientHandler me controller_chan switch_connections_mvar handle = do
    syncPutStrLn "I have been connected from somewhere."
    forever $ do
        anypacket <- readJSON handle
        let Success packet = fromJSON $ _data anypacket :: Result Packet

        rule <- askForRule packet
        let destination = _nextHop rule

        unless (destination == me) $
            modifyMVar_ switch_connections_mvar $ \switch_connections -> do
                new_connections <- getSwitchConnection destination switch_connections
                forward destination (over ttl ((+) (-1)) packet) new_connections
                return new_connections
  where
    forward destination packet switch_connections
        | _ttl packet == 0 = pure ()
        | otherwise = hPut conn $ B.toStrict $ encode $ AnyPacket
            { _from = me
            , _to = destination
            , _data = toJSON packet }
      where
        Just conn = M.lookup destination switch_connections

    getSwitchConnection destination switch_connections =
        case M.lookup destination switch_connections of
            Nothing -> do
                syncPutStrLn $ "I'm connecting to switch " ++ T.unpack destination
                let (host, _) = splitHost destination
                conn <- connectTo (T.unpack host) (PortNumber 8888)
                syncPutStrLn "Connected"
                return $ M.insert destination conn switch_connections
            Just _ -> return switch_connections

    askForRule (_flowID -> fid) = do
        rule_mvar <- newEmptyMVar
        atomically $ writeTChan controller_chan (fid, rule_mvar)
        takeMVar rule_mvar

expired :: Rule -> IO Bool
expired rule = do
    now <- getCurrentTime
    return $ now > _expires rule

