public class Packet
{ 
	private int flowId;
	private String from;
	private String to;
	private String payload;
	private int ttl;

	public Packet(int flowId, String from, String to, String payload, int ttl) {
		this.flowId = flowId;
		this.from = from;
		this.to = to;
		this.payload = payload;
		this.ttl = ttl;
	}

	public String toString() {
		return
		"{\"from\":\""+this.from+"\","+
		 "\"to\":\""+this.to+"\","+
		 "\"data\": {"+
		    "\"flow_id\":"+this.flowId+","+
		    "\"payload\":\""+this.payload+"\","+
		    "\"ttl\":"+this.ttl+"}"+
		"}\n";
	}
}