using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IJob : IIdentifiable
    {
        List<IStat> BaseStatModifiers { get; set; }
        List<Guid> Abilities { get; set; }
        List<Guid> StartItems { get; set; }
        List<Guid> Tags { get; set; }
        List<Guid> RaceRequirements { get; set; }
        Location StartLocation { get; set; }
        int StartMoney { get; set; }

        void Apply(ICharacter ch);
    }
}
