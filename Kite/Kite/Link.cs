using System;

namespace Kite
{
    public class Link
    {
        private int delay; // in milliseconds

        public int Delay
        {
            get { return this.delay; }
        }

        public Link( int delay )
        {
            this.delay = delay;
        }
    }
}

