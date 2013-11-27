using System;
using System.Collections.Generic;
using System.Linq;

namespace Kite
{
    public class Network
    {
        private HashSet<Node> nodes = new HashSet<Node>();

        public IEnumerable<Node> Nodes
        {
            get
            {
                return this.nodes;
            }
        }

        public Network()
        {
        }

        public void AddNode(Node node)
        {
			this.nodes.Add(node);
        }

		public Node FindNode(string name)
		{
			return this.nodes.FirstOrDefault(node => node.Name == name);
		}
    }
}

