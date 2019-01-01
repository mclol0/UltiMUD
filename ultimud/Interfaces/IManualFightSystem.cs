using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IManualFightSystem : IFightSystem
    {
        IAbility ComboAbility { get; set; }
        List<IAbility> ComboTriggers { get; set; }
        string ComboChance { get; set; }
        string ComboLength { get; set; }
        string OnNpcUpdate { get; set; }
        Globals.FightSystemRestoreTypes RestoreType { get; set; }
    }
}
