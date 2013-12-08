#!/bin/sh
ghc -Wall -O -threaded -rtsopts Switch.hs -o Switch -hide-package monads-tf

