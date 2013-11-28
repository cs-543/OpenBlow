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
        	var switchNames = line.Trim().Split(new string[] { " -- " }, StringSplitOptions.RemoveEmptyEntries);

        	if (switchNames.Length == 2)
        	{
            // Get the switches or build new ones.
            var sw0 = network.FindSwitch(switchNames[0]) ?? new Switch(switchNames[0]);
            var sw1 = network.FindSwitch(switchNames[1]) ?? new Switch(switchNames[1]);

            // Link them together
                        sw0.AddNeighboor(sw1);
                        sw1.AddNeighboor(sw0);

            // Store them back in the network.
            network.AddSwitch(sw0);
            network.AddSwitch(sw1);
        	}
        }
    	}

    	return network;
    }
	}
}
