using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IStatMod
    {
        IStatBlueprint StatBlueprint { get; set; }
        IScript Script { get; set; }
        Globals.StatModTargets ApplyTo { get; set; }
        bool Restore { get; set; }
        bool Silent { get; set; }
        int Chance { get; set; }

        long Apply(IObject applyto, IObject actor, IObject target, IAbility ability, bool silent);
        long Undo(IObject undofrom, IObject influence, bool silent);
        long Calculate(IObject actor, IObject target, IAbility ability, string script);
        long Calculate(IObject actor, IObject target, IAbility ability);
    }
}
