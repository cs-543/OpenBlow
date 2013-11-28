using System;

namespace Kite
{
    public class Node : IEquatable<Node>
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

        public bool Equals(Node other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name == this.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Node);
        }

        public static bool operator==(Node left, Node right)
        {
            if (Object.ReferenceEquals(left, null) || Object.ReferenceEquals(right, null))
            {
                return Object.Equals(left, right);
            }

            return left.Equals(right);
        }

        public static bool operator!=(Node left, Node right)
        {
            return !(left == right);
        }
    }
}

