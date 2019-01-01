using System;
using System.Collections.Generic;
using System.Reflection;
using Damdai;

namespace Ultimud
{
    public interface IStatBlueprint : ICloneable, IIdentifiable
    {
        long Min { get; set; }
        long Max { get; set; }
        long StartCurrent { get; set; }
        long StartMax { get; set; }
        int UpdateFrequency { get; set; }
        bool IsSystemStat { get; set; }
        IScript OnMin { get; set; }
        IScript OnMax { get; set; }
        IScript OnUpdate { get; set; }
        List<Guid> Tags { get; set; }

        void MinReached(IStat stat, IObject obj);
        void MaxReached(IStat stat, IObject obj);
        void Update(IStat stat, IObject obj);
    }
}
