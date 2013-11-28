using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kite
{
    public class Switch : Node
    {
        // This is the routing table and neigboors at the same time.
        // (lookup by target string is not efficient with this but who cares...)
        private Dictionary<Switch, HashSet<string> > rtable = new Dictionary<Switch, HashSet<string> >();

        // (What the crap is a neigboor? Are you messing with me?)

        public IEnumerable<Switch> Neigboors
        {
            get
            {
                return this.rtable.Keys;
            }
        }

        public void AddRoutingRule(string target_name
                                  ,Switch target)
        {
            // Refuse to add a routing rule if the neigboor has not been declared with 'AddNeighboor'
            Debug.Assert( rtable.ContainsKey(target)
                        , "neigboors does not contain target in AddRoutingRule().");

            rtable [target].Add(target_name);
        }

        public Switch(string name): base(name)
        {
        }

        public void AddNeighboor(Switch neigboor)
        {
            if (this.rtable.ContainsKey(neigboor))
            {
                return;
            }

            this.rtable.Add(neigboor, new HashSet<string>());
        }
    }
}

