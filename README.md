# Installing
    sudo ./install.sh

# Running
First, run the POX controller:

    ./pox/pox.py forwarding.hub

- forwarding.hub
  - This hub example just installs wildcarded flood rules on every switch, essentially turning them all into $10 ethernet hubs.
  - Fore more information [read here](https://openflow.stanford.edu/display/ONL/POX+Wiki#POXWiki-forwarding.hub)

Currently you need to enable STP in the Open vSwitch controller or otherwise
the hubbing controller will make the switches replicate packets to all other
switches which in turn will make more and more packets. And before you know it,
all the switches are $0 bricks and it's your fault. STP is a spanning tree
protocol that makes the switches be a little more smart about looping
topologies.

At the moment we don't have a clean way to enable STP so go to mininet source
and modify mininet/node.py file:

    node.py:
    ...
    self.cmd( 'ovs-vsctl -- set Bridge', self,
              'other_config:datapath-id=' + self.dpid )
    self.cmd( 'ovs-vsctl set-fail-mode', self, self.failMode )
    self.cmd( 'ovs-vsctl set Bridge', self, 'stp_enable=yes' )
    for intf in self.intfList():                  ^
        if not intf.IP():                         |
    ...                                           +-- Add this line

Add the line that says stp_enable=yes.

Then run mininet:


    sudo mn --custom mesh.py --topo mesh --mac --switch ovsk --controller remote
                      ^              ^      ^            ^                  ^
                      |              |      |            |                  |
                      |              |      |   Use Open vSwitch switches   |
                      |              |      |                               |
                      |              |      |                Use remote OpenFlow
                      |              |      |                controller (in our case
                      |              |      |                pox).
                      |              |      |
                      |              |      +-- Use mac addresses that match
                      |              |          IP addresses
    Load our custom topology file    |
                                     |
                               Actually use that topology


It takes about 30 seconds before there actually is connectivity between
hosts to don't assume it is broken if at first you get unreachable host
messages from trying to ping stuff.

# Further documentation
- [mininet introduction](https://github.com/mininet/mininet/wiki/Introduction-to-Mininet)
