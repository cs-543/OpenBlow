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

        // Separate dictionary for neigboors and link qualities.
        private Dictionary<Switch, Link> ltable = new Dictionary<Switch, Link>();

        // (What the crap is a neigboor? Are you messing with me?)

        public IEnumerable<Switch> Neigboors
        {
            get
            {
                return this.rtable.Keys;
            }
        }

        public void AddRoutingRule( string target_name
                                  , Switch target )
        {
            // Refuse to add a routing rule if the neigboor has not been declared with 'AddNeighboor'
            Debug.Assert( rtable.ContainsKey(target)
                        , "neigboors does not contain target in AddRoutingRule().");

            rtable [target].Add(target_name);
        }

        public Switch(string name): base(name)
        {
        }

        // This one returns the switch where to go if you want to reach 'target'.
        // Or it might return 'null' if this switch has no idea.
        public Switch WhereDoIGoNext(string target)
        {
            // FIXME: I didn't test this method

            // Are we sure there is not some smart one-liner to do this?
            foreach (KeyValuePair<Switch, HashSet<string> > pair in this.rtable)
            {
                if (pair.Value.Contains(target))
                {
                    return pair.Key;
                }
            }
            return null;
        }

        // Returns the link quality to given switch. If the given switch is not a neigboor of this one,
        // returns null.
        public Link LinkQualityTo(Switch target)
        {
            return ltable.ContainsKey(target) ? ltable [target] : null;
        }

        public void AddNeighboor(Switch neigboor, Link link)
        {
            if (this.rtable.ContainsKey(neigboor))
            {
                return;
            }

            this.rtable.Add(neigboor, new HashSet<string>());
            this.ltable.Add(neigboor, link);
        }
    }
}

