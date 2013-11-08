#!/bin/bash

if [[ $(id -u) -ne 0 ]]; then
    echo "You must run this script as root."
    exit
fi

# Install mininet
apt-get install mininet

# Remove openvswitch controller
service openvswitch-controller stop
update-rc.d openvswitch-controller disable

# Install POX
user=`who am i | cut -d ' ' -f 1`
su -'c git clone https://github.com/noxrepo/pox.git' $user
