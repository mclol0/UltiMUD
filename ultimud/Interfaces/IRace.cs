using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IRace : IIdentifiable
    {
        List<Guid> Abilities { get; set; }
        List<Guid> StartItems { get; set; }
        List<Guid> Tags { get; set; }
        List<IStat> BaseStatModifiers { get; set; }
        Location StartLocation { get; set; }
        Location DeathLocation { get; set; }
        int StartMoney { get; set; }

        void Apply(ICharacter ch);
    }
}
