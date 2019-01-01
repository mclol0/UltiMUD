using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Damdai;

namespace Ultimud
{
    public interface IStatusEffectBlueprint : IIdentifiable
    {
        List<Guid> Tags { get; set; }
        Timer UpdateTimer { get; set; }
        BitArray RemoveOnPosition { get; set; }
        IScript OnStart { get; set; }
        IScript OnUpdate { get; set; }
        IScript OnEnd { get; set; }
    }
}
