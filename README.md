# Installing
    sudo ./install.sh

# Running
First, run the POX controller:

    ./pox/pox.py forwarding.hub

Then run mininet:

    sudo mn --custom mesh.py --topo mesh,2,3 --mac --switch ovsk --controller remote