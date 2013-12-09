import java.net.*;

public class TimedOutput extends Thread {
		
	public static int NUMBER_OF_PACKETS = 1;
    public static int MAXIMUM_DELAY = 102;
    public static int MINIMUM_DELAY = 13;

    public static int HIGH_TTL_PERCENTAGE = 20;
    public String address;
    //public DatagramSocket socket;

	public TimedOutput(String address) {
	    super(address);
		this.address = address;
		//try { socket = new DatagramSocket(8888); }
		//catch (Exception e) { System.out.println(e); }
		
	}

	private int counter = 2;
	private int delay = 123; // initial delay

    public void run() {
    	for(int i=0; i<NUMBER_OF_PACKETS; i++) {
    		try { this.sleep(delay); }
	        catch (Exception e) { System.out.println(e); }
	        delay = ((delay * counter) % MAXIMUM_DELAY)+MINIMUM_DELAY;
	        counter++;
	        System.out.println(
	        	new Packet((delay % 5)+1,
	        	           "143.248.195.95:12345",
	        	           //"143.248.229.61:8888",
	        	           address,
	        	           "payload"+delay,
	        	           counter % (100/HIGH_TTL_PERCENTAGE) == 0 ? 128 : 4));
    	}
    	//try {
    	//byte[] buf = "foobar".getBytes();
		//InetAddress address = InetAddress.getByName("192.168.0.113");
		//DatagramPacket packet = new DatagramPacket(buf, buf.length, address, 4445);
		//socket.send(packet);
		//System.out.println(buf.length);
		//socket.close();
	    //} catch (Exception e) { System.out.println(e); }
    	
    }
}