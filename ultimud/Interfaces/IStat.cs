using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IStat
    {
        Guid BlueprintId { get; }
        IStatBlueprint Blueprint { get; }
        long Current { get; set; }
        long Max { get; set; }
        double Percent { get; }

        void CheckTriggers(IObject o);
        string Meter(char c);
        string Meter(string str);
    }
}
