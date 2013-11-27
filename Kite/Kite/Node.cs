using System;

namespace Kite
{
    public class Node
    {
		public string Name
		{
			get;
			private set;
		}

        public Node(string name)
        {
			this.Name = name;
        }

		public int GetHashCode()
		{
			return this.Name.GetHashCode();
		}
    }
}

