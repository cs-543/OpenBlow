# Installing
    sudo ./install.sh

# Running
First, run the POX controller:

    ./pox/pox.py forwarding.hub

- forwarding.hub
  - This hub example just installs wildcarded flood rules on every switch, essentially turning them all into $10 ethernet hubs.
  - Fore more information [read here](https://openflow.stanford.edu/display/ONL/POX+Wiki#POXWiki-forwarding.hub)

Then run mininet:

    sudo mn --custom mesh.py --topo mesh,2,3 --mac --switch ovsk --controller remote

- --custom mesh.py
  - As of commit [94adabfaf7b067f7091a41aabaab97c651291418](https://github.com/cs-543/OpenBlow/commit/94adabfaf7b067f7091a41aabaab97c651291418) runs a script that creates one switch and one host connected to it [to be confirmed!]
- --topo mesh,2,3
  - tells mininet to start using the topology of a mesh network with 2 switches and 3 hosts??? [to be confirmed!]
- --mac
  - tells mininet to assign each host a sequencial mac address, matching its IP address
- --switch ovsk
  - tells mininet that the switches are to be of the type ovsk, this is the type for Openflow
- --controller remote
  - tells mininet that each Openflow switch is to talk to a controller, which is located at a remote location

# Further documentation
- [mininet introduction](https://github.com/mininet/mininet/wiki/Introduction-to-Mininet)