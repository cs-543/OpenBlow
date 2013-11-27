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
			var links = new List<string>();
			var network = new Network();

			using (var reader = new StreamReader(stream))
			{
				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					// We only care about links, really.
					if (line.Contains("--"))
					{
						links.Add(line.Trim());
					}
				}
			}

			links.ForEach(link => {
				var nodeNames = link.Split(new string[] { " -- " }, StringSplitOptions.None);

				var node0 = network.FindNode(nodeNames[0]) ?? new Node(nodeNames[0]);
				var node1 = network.FindNode(nodeNames[1]) ?? new Node(nodeNames[1]);

				network.AddNode(node0);
				network.AddNode(node1);
			});

			return network;
		}
	}
}

