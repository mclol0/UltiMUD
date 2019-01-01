using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IStatFactor
    {
        IStatBlueprint StatBlueprint { get; set; }
        float Factor { get; set; }

        long Calculate(IStat stat);
    }
}
