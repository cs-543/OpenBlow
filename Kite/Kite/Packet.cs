using System;
using System.Diagnostics;

namespace Kite
{
    public class Packet
    {
        private int payloadSize;
        private FlowType flow;

        // Flow types for a packet
        public enum FlowType { OpenBlow            // This packet is a control packet for OpenBlow
                             , Guided              // Travels the same path as OpenBlow but needs
                                                   // "guidance" from OpenBlow controller
        }

        public Packet( int packetSize )
        {
            payloadSize = packetSize;
            flow = FlowType.OpenBlow;
        }

        public Packet( int packetSize, FlowType flow ) : this(packetSize)
        {
            this.flow = flow;
        }

        // Travels from one switch to other.
        // returns the number of milliseconds the traversal took.
        //
        // Returns -1 if the packet was dropped.
        public int Travel( Switch from, Switch to )
        {
            switch (flow)
            {
                case FlowType.OpenBlow:
                    return TravelOpenBlow(from, to);
                case FlowType.Guided:
                    // IMPLEMENT ME
                    // return TravelGuided(from, to);
                    throw new System.NotImplementedException();
                default:
                    // wot
                    Debug.Assert(false, "I did not expect to find myself in this part of code.");
                    return 0;
            }
        }

        private int TravelOpenBlow( Switch from, Switch to )
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

