#!/usr/bin/python

from mininet.topo import Topo
from mininet.net import Mininet
from mininet.node import RemoteController
from mininet.cli import CLI

class MeshTopo(Topo):
    def __init__(self, switchCount = 1, hostsPerSwitch = 1, **opts):
        Topo.__init__(self, **opts)

        switches = []

        # Create switches
        for i in range(switchCount):
            switch = self.addSwitch('s%s' % (i + 1))
            switches.append(switch)

            # Add link to other switches
            for other in switches:
                self.addLink(switch, other)

        # Create hosts
        for i in range(switchCount * hostsPerSwitch):
            host = self.addHost('h%s' % i)
            self.addLink(switches[i / hostsPerSwitch], host)

topos = { 'mesh': MeshTopo }
