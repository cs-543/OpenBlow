public class TrafficGenerator
{

    /*
    20% high ttl
    40% 
    */

	public static void main(String [] args)
	{
		if (args.length <1)
			System.out.println("Usage: TrafficGenerator ip:port");
		else
			(new TimedOutput(args[0])).start();
	}

	
}