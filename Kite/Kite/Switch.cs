using System;
using System.Collections.Generic;

namespace Kite
{
    public class Switch : Node
    {
        private HashSet<Switch> neigboors = new HashSet<Switch>();

        public IEnumerable<Switch> Neigboors
        {
            get
            {
                return this.neigboors;
            }
        }
            

        public Switch(string name): base(name)
        {
        }

        public void AddNeighboor(Switch neigboor)
        {
            if (this.neigboors.Contains(neigboor))
            {
                return;
            }

            this.neigboors.Add(neigboor);
        }
    }
}

