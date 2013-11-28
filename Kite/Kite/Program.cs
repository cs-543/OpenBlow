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

            Console.WriteLine("Press any key to terminate...");
            Console.ReadKey(true);
        }
    }
}
