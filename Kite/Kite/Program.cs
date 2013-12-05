using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Kite
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Building network...");
            var network = GraphReader.BuildNetwork(File.Open("../../../../mesh.dot", FileMode.Open));
            Console.WriteLine(string.Format("Network built with {0} switches.", network.Switches.Count()));

            Console.WriteLine("Establishing routes...");
            network.EstablishRoutes();
            Console.WriteLine(string.Format("Routes established."));

            Console.WriteLine("Sending one packet from s0 to s18...");
            Packet p = new Packet(1000);
            int delay = p.Travel(network.FindSwitch("s0"), network.FindSwitch("s18"));
            Console.WriteLine(string.Format("That took {0} virtual milliseconds.", delay));

            Console.WriteLine("Press any key to terminate...");
            Console.ReadKey(true);
        }
    }
}
