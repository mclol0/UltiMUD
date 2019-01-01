using System;
using System.Collections;
using System.Collections.Generic;

namespace Ultimud
{
    public interface IInfoChannel
    {
        BitArray Events { get; set; }
        string Output { get; set; }

        void Broadcast(BroadcastEvent ev, string str);
    }
}
