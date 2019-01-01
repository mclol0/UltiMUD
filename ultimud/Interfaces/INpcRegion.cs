using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface INpcRegion : ILayer
    {
        Dictionary<Guid, int> Npcs { get; set; }
        IScript OnSpawn { get; set; }
        int SpawnLimit { get; set; }
        int CurrentCount { get; }
    }
}
