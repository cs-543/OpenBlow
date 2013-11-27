using System;
using System.Collections.Generic;
using System.IO;

namespace Kite
{
	public class GraphReader
	{
		// Probably the most awful and lazy parsing method I've ever written. Oh well.
		public static Network BuildNetwork(Stream stream)
		{
			var network = new Network();

			using (var reader = new StreamReader(stream))
			{
				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					// We only care about links, really.
					var nodeNames = line.Trim().Split(new string[] { " -- " }, StringSplitOptions.RemoveEmptyEntries);

					if (nodeNames.Length == 2)
					{
						// Get the nodes or build new ones.
						var node0 = network.FindNode(nodeNames[0]) ?? new Node(nodeNames[0]);
						var node1 = network.FindNode(nodeNames[1]) ?? new Node(nodeNames[1]);

						// Link them together
						// TODO

						// Store them back in the network.
						network.AddNode(node0);
						network.AddNode(node1);
					}
				}
			}

			return network;
		}
	}
}
