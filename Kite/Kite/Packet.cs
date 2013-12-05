using System;

namespace Kite
{
    public class Packet
    {
        private int payloadSize;

        public Packet( int packetSize )
        {
            payloadSize = packetSize;
        }

        // Travels from one switch to other, using the magical routing and
        // returns the number of milliseconds the traversal took.
        //
        // Returns -1 if the packet was dropped.
        public int Travel( Switch from, Switch to )
        {
            int timing = 0;
            while ( from != to )
            {
                Switch next = from.WhereDoIGoNext(to.Name);
                if (next == null)
                    return -1;

                Link link = from.LinkQualityTo(next);
                timing += link.Delay;

                from = next;
            }

            return timing;
        }
    }
}

