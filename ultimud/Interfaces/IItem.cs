using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IItem : IObject, ICloseable
    {
        IItem Blueprint { get; }
        List<Guid> RaceRequirements { get; set; }
        List<Guid> JobRequirements { get; set; }
        List<Guid> AbilityRequirements { get; set; }
        List<IStat> StatRequirements { get; set; }
        BodyLocation EquipLocation { get; set; }
        int PrepareTime { get; set; }
        int RecoverTime { get; set; }
        int RangeMax { get; set; }
        uint RangeMin { get; set; }
        IScript OnUse { get; set; }
        IScript OnRemove { get; set; }
        IScript OnDrop { get; set; }
        IScript OnPickup { get; set; }

        bool CanUse(ICharacter ch);
    }
}
