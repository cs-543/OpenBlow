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












================== NS-3 OpenFlow =================================

Installing NS-3 :
1) run install_ns3_prereq.sh to install long list of prerequisities.(You better use eth connection since those prereq may take up to 2 GB to download)
2) Follow this steps:

         Downloading ns-3 Using a Tarball

        The process for downloading ns-3 via tarball is simpler than the Mercurial process since all of the pieces are pre-packaged for you. You just have to pick a release, download it and decompress it.

        As mentioned above, one practice is to create a directory called repos in one's home directory under which one can keep local Mercurial repositories. One could also keep a tarballs directory. If you adopt the tarballs directory approach, you can get a copy of a release by typing the following into your Linux shell (substitute the appropriate version numbers, of course):

         cd
         mkdir tarballs
         cd tarballs
         wget http://www.nsnam.org/release/ns-allinone-3.13.tar.bz2
         tar xjf ns-allinone-3.13.tar.bz2

        If you change into the directory ns-allinone-3.13 you should see a number of files:

         build.py*      ns-3.13/    pybindgen-0.15.0.795/  util.py
         constants.py   nsc-0.5.2/  README

        You are now ready to build the ns-3 distribution.
        Building ns-3 with build.py

        The first time you build the ns-3 project you should build using the allinone environment. This will get the project configured for you in the most commonly useful way.

        Change into the directory you created in the download section above. If you downloaded using Mercurial you should have a directory called ns-3-allinone under your ~/repos directory. If you downloaded using a tarball you should have a directory called something like ns-allinone-3.13 under your ~/tarballs directory. Type the following:

         ./build.py

        You will see lots of typical compiler output messages displayed as the build script builds the various pieces you downloaded. Eventually you should see the following magic words:

         Build finished successfully (00:02:37)
         Leaving directory `./ns-3-dev'

        Once the project has built you typically will not use ns-3-allinone scripts. You will now interact directly with Waf and we do it in the ns-3-dev directory and not in the ns-3-allinone directory. 

3) Install openflow for NS-3 like below:
        In order to use the OpenFlowSwitch module, you must create and link the OFSID (OpenFlow Software Implementation Distribution) to ns-3. To do this:

        #1 Obtain the OFSID code. An ns-3 specific OFSID branch is provided to ensure operation with ns-3. Use mercurial to download this branch and waf to build the library::

        $ hg clone http://code.nsnam.org/jpelkey3/openflow
        $ cd openflow

        From the “openflow” directory, run::

        $ ./waf configure
        $ ./waf build

        #2 Your OFSID is now built into a libopenflow.a library! To link to an ns-3 build with this OpenFlow switch module, run from the ns-3-dev (or whatever you have named your distribution)::

        $ ./waf configure --enable-examples --enable-tests --with-openflow=path/to/openflow

        #3 Under ---- Summary of optional NS-3 features: you should see::

        "NS-3 OpenFlow Integration     : enabled"

        indicating the library has been linked to ns-3. Run::

        $ ./waf build

        to build ns-3 and activate the OpenFlowSwitch module in ns-3.

4) Run openflow example :

    Go to ns-allinone-3.13/ns-3.13 folder
    copy openflow-switch.cc to ns-allinone-3.13/ns-3.13/scratch folder
    from ns-allinone/ns-3.13 type $./waf --run "scratch/openflow-switch -v"
