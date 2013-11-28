using System;
using System.Collections.Generic;
using System.Linq;

namespace Kite
{
    public class Network
    {
        private HashSet<Switch> switches = new HashSet<Switch>();

        public IEnumerable<Switch> Switches
        {
            get
            {
                return this.switches;
            }
        }

        public void AddSwitch(Switch sw)
        {
            this.switches.Add(sw);
        }

        public Switch FindSwitch(string name)
        {
            return this.switches.FirstOrDefault(sw => sw.Name == name);
        }

        public int EstablishRoutes()
        {
            int iteration = 0;

            return iteration;
        }
    }
}

