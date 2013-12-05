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

        // This method magically establishes routing for all switches
        public void EstablishRoutes()
        {
            // FIXME: what happens if you call EstablishRoutes() more than once?
            // (the old rules should be probably thrown away)
            foreach (Switch s in this.switches)
            {
                EstablishRoutesFor(s);
            }
        }

        private void EstablishRoutesFor(Switch sw)
        {
            // this is breadth-first generation. Every switch knows which direction
            // to shove a packet to get the shortest path. It also means that every switch
            // needs to know where every other switch is.

            // Some observations of the current algorithm:
            //
            // While this will always choose the shortest path, it will favor one switch over the others
            // when path lengths would be equal. I don't know if this is a problem.
            //
            // ...Will it blend?
            var visited = new Dictionary<Switch, Switch>();
            var visit_next = new Dictionary<Switch, Switch>();

            foreach (Switch n in sw.Neigboors)
            {
                visited[n] = n;
                foreach (Switch ns in n.Neigboors)
                {
                    visit_next[ns] = n;
                }
            }

            while (visit_next.Count > 0)
            {
                var old_visit = visit_next;
                visit_next = new Dictionary<Switch, Switch>();
                foreach (KeyValuePair<Switch, Switch> pair in old_visit)
                {
                    Switch came_from = pair.Value;
                    Switch next = pair.Key;

                    if (!visited.ContainsKey(next))
                    {
                        visited[next] = came_from;
                        foreach (Switch ns in next.Neigboors)
                        {
                            if (!visited.ContainsKey(ns))
                            {
                                visit_next [ns] = came_from;
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<Switch, Switch> pair in visited)
            {
                Switch target_switch = pair.Key;
                Switch came_from = pair.Value;

                // Don't add rule to switch itself
                if (target_switch == sw)
                    continue;

                // Console.WriteLine(String.Format("Switch {0}: Go through {1} to get to {2}", sw.Name, came_from.Name, target_switch.Name));

                sw.AddRoutingRule(target_switch.Name, came_from);
            }
        }
    }
}

