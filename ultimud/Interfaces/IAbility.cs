using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IAbility : IIdentifiable
    {
        List<Guid> Defense { get; set; }
        List<Guid> AbilityRequirements { get; set; }
        List<Guid> RaceRequirements { get; set; }
        List<Guid> JobRequirements { get; set; }
        List<Guid> RequiredItemTags { get; set; }
        List<IStat> StatRequirements { get; set; }
        BitArray TargetTypes { get; set; }
        BodyLocation BodyPart { get; set; }
        AbilityStyle Style { get; set; }
        AbilityTarget Target { get; set; }
        IScript Script { get; set; }
        IScript OnLevelUp { get; set; }

        bool IsInterruptable { get; set; }
        bool LimitRangeBySight { get; set; }
        bool DisableMenu { get; set; }
        int MaxLevel { get; set; }
        int AreaEffect { get; set; }
        int PrepareTime { get; set; }
        int RecoverTime { get; set; }
        int RangeMax { get; set; }
        uint RangeMin { get; set; }
        string Shortcut { get; set; }

        int GetPrepareTime(ICharacter ch);
        int GetRecoverTime(ICharacter ch);
        int GetPrepareTime(IItem item);
        int GetRecoverTime(IItem item);
        bool IsTargetInRange(IObject actor, IObject target);
        AbilityStatus Run(ICharacter actor, IObject target, IItem item, AbilityState state);
    }
}
