import socket
from optparse import OptionParser
import threading

import json
import errno
from socket import error as socket_error

numofpackets=0

class PacketParser:
	source=None
	destination=None
	
	def __init__(self,jsonstring):
		j = json.loads(jsonstring)
		self.source=j['from']
		self.destination=j['to']


class ForwardingTable:
	ftable={}
	def __init__(self,ft=None):
		if ft is not None:
			self.parse(ft)
	
	def parse(self,ftable):
		with open(ftable) as f:
			for line in f:
				(key, val) = line.split('->')
				self.ftable[key] = val.split(':')
	
	def getIP(self,entry):
		return entry[0]
	
	def getPort(self,entry):
		return int(entry[1])


def main():
	#parse the command line arguments
	parser = OptionParser()
	parser.add_option("-p", "--port", dest="portnum",help="port number", metavar="<some number>", default=None)
	parser.add_option("-t", "--forwarding-table", dest="ftable",help="Forwarding table in a txt file", default=None)
	parser.add_option("-i", "--ip", dest="IP", help="IP adress of the interface to connect", default=None)
	(options, args) = parser.parse_args()
	ftable = options.ftable
	portnum = options.portnum
	IP = options.IP
	if ftable is not None and portnum is not None and IP is not None:
		run(ftable,int(portnum),IP)
	else:
		print 'Input forwarding table and port number. -h for help'


def handle_connection(sock,conn,message):
	fsock = sock.makefile()
	global numofpackets
	try:
		while 1:
			
			data = fsock.readline()
			if not data: 
				print "%s:connection refused by peer",message
				conn.close
				break
			else:
				numofpackets+=1
				print "%s:receieved data" % message
				conn.send(data)
				print 'total number of packets: %d' % numofpackets
	except socket_error:
		sock.close()
		conn.close()
		

def run(ftable,PORT,IP):
	forwarding_table = ForwardingTable(ft=ftable)
	t = forwarding_table.ftable
	HOST = IP
	print HOST,str(PORT)
	s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	s.bind((HOST, PORT))
	s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
	s.listen(5)
	while 1:
		conn, addr = s.accept()
		print 'Connected by', addr
		s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
		f = conn.makefile()
		data = f.readline()
		if not data:
			continue
		print str(data)
		packet = PacketParser(str(data))
		sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
		s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
		dest_IP = forwarding_table.getIP(t[packet.destination])
		dest_PORT = forwarding_table.getPort(t[packet.destination])
		print dest_IP,dest_PORT
		sock.connect((dest_IP,dest_PORT))
		
		t1 = threading.Thread(target=handle_connection, args=(sock,conn,'sock->conn'))
		t1.daemon = True
		t1.start()
		
		t2 = threading.Thread(target=handle_connection, args=(conn,sock,'conn->sock'))
		t2.daemon = True
		t2.start()
		sock.send(data+'\n')
main()

