#!/usr/bin/python

# Creates mesh network topologies

from mininet.topo import Topo
from mininet.net import Mininet
from mininet.node import RemoteController
from mininet.cli import CLI

import random

WIDTH = 16
HEIGHT = 9

class MeshTopo(Topo):

    def __init__(self, **opts):
        Topo.__init__(self, **opts)

        # Now, let us try to make "a realistic" mesh network.
        # In a wireless mesh network, I assume people just spread those access
        # points everywhere and then make them contact each other to see who
        # can reach who and build a wonderful topology. What we are going to do
        # is emulate this behaviour


        # Now, first let's find out nice locations for the switches.
        # For the moment, I'll use a rectangle with aspect ratio 16:9 because
        # then it'll fit nicely in a presentation without having to stretch in
        # any direction.

        #  0             16
        #  v             v
        #  +-------------+<--0
        #  |             |
        #  |             |
        #  +-------------+<--9
        #

        # Get me my switch locations
        switch_locations = []
        tries = 100
        while tries > 0:
            tries = place_switch( tries, 100, switch_locations
                                , minDistance = 8.0 )

        # Okay so then let's decide where the links go. Switches should link to
        # those switches that are within their area of influence. So what we do
        # is that we assign a distance on how far the influence reaches and
        # then link all those switches that are in range. We start with 1.2
        # distance for all switches and then we check if everything is
        # connected. If not, then we find a pair of switches where the other
        # one is not connected and then increase the strength of 
        # non-connected switch until it connects. We continue until everything
        # is connected.

        switch_strengths = map(lambda _: 9.0, switch_locations)
        def new_links():
            return make_switch_links( switch_locations, switch_strengths )
        switch_links = new_links()

        while True:
            unconnected_switches = get_unconnected_switches( switch_links )
            if not unconnected_switches:
                break

            print(unconnected_switches)

            # Find the best switch to muscle up
            best_unconnected_switch = None
            best_unconnected_distance = 100000000
            for i1 in range(len(switch_locations)):
                for i2 in range(len(switch_locations)):
                    if (i1 != i2 and

                        distance2( switch_locations[i1]
                                 , switch_locations[i2] ) <
                        best_unconnected_distance and

                        ( (i1 in unconnected_switches and
                           i2 not in unconnected_switches ) or
                          (i1 not in unconnected_switches and
                           i2 in unconnected_switches ) )):

                        best_unconnected_switch = i1
                        if i2 in unconnected_switches:
                            best_unconnected_switch = i2
                        best_unconnected_distance = distance2(
                                switch_locations[i1]
                              , switch_locations[i2])

            switch_strengths[best_unconnected_switch] += 0.1
            switch_links = new_links()

        # Now, the mininet stuff. Take the results of our wonderful
        # calculations and spit out something mininet can understand.
        counter = 0
        switches = []
        for switch in switch_locations:
            switches.append(self.addSwitch('s%s' % counter))
            counter += 1

        print(switches)

        added_links = { }
        for i in range(len(switches)):
            # Create a host for each switch.
            host = self.addHost('h%s' % i)
            self.addLink(host, switches[i])

            for ( from_link, to_link ) in switch_links[i]:
                min_link = min(from_link, to_link)
                max_link = max(from_link, to_link)
                if (not added_links.has_key(min_link) or
                    not (max_link in added_links[min_link])):
                    self.addLink( switches[from_link], switches[to_link] )

                    if added_links.has_key(min_link):
                        added_links[min_link].add(max_link)
                    else:
                        added_links[min_link] = set([max_link])

def make_switch_links( switch_locations, switch_strengths ):
    """
    Returns a list of links given switch locations and their strengths.
    """
    results = map( lambda _: [], switch_locations )
    for i in range(len(switch_locations)):
        switch = switch_locations[i]
        my_strength = switch_strengths[i]
        close_switches = filter( lambda other:
                             other != i and
                             distance2( switch, switch_locations[other] ) <
                              max( switch_strengths[other], my_strength )**2
                          , range(len(switch_locations)) )

        for c in close_switches:
            results[i].append( (i, c) )
            results[c].append( (c, i) ) # I think we don't actually need pairs
                                        # inside but whatever
    return results

def place_switch( tries, triesReset, switchLocations, minDistance = 1 ):
    """
    Places a switch. Tries to place it somewhere where it is not too close
    to other switches. Returns 'tries-1' if it fails and 'triesReset' if
    it succeeds. Also updates 'switchLocations' on success.

    'minDistance' is the minimum euclidean distance between two switches.

    'switchLocations' is an array of pairs of numbers (and those pairs are
    locations).
    """
    x = random.random() * WIDTH
    y = random.random() * HEIGHT

    minDistance = minDistance * 2

    for existing_location in switchLocations:
        if distance2( (x,y), existing_location ) < minDistance:
            return tries-1

    switchLocations.append( (x,y) )
    return triesReset

def get_unconnected_switches( switch_links ):
    """
    Returns the set of unconnected switches.

    'switch_links' is an array (where each index corresponds to one switch) of
    arrays where values are index pairs between two switches.

    E.g. [[(0,1)],[(1,0)]] is two switches that are connected to each other.
    """

    # Don't throw error if switch_links is empty
    if not switch_links:     # They say this is pythonic. I'm not so sure.
        return set()

    unconnected_switches = set(range(len(switch_links)))

    def traverse(index):
        if index not in unconnected_switches:
            return

        unconnected_switches.remove(index)
        for links in switch_links[index]:
            ( from_switch, to_switch ) = links
            # One of these equals index but I don't care.
            traverse( from_switch )
            traverse( to_switch )

    traverse(0)
    return unconnected_switches

def distance2( coords1, coords2 ):
    "Returns squared distance between two two-dimensional coordinates."
    ( x1, y1 ) = coords1
    ( x2, y2 ) = coords2
    return (x1-x2)**2 + (y1-y2)**2

topos = { 'mesh': MeshTopo }

