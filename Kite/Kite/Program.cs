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
			var network = GraphReader.BuildNetwork(File.Open("../../../../mesh.dot", FileMode.Open));

			Console.WriteLine(string.Format("Network built. There are {0} nodes.", network.Nodes.Count()));
            Console.WriteLine("Press any key to terminate...");
            Console.ReadKey(true);
        }
    }
}
