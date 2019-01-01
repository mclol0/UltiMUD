using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ultimud
{
    public interface IChannel : IIdentifiable, ICommandProcessor
    {
        List<Guid> RaceRequirements { get; set; }
        List<Guid> JobRequirements { get; set; }
        List<Guid> AbilityRequirements { get; set; }
        List<Guid> ItemRequirements { get; set; }
        List<IStat> StatRequirements { get; set; }
        List<Guid> Tags { get; set; }
        string SenderOutput { get; set; }
        string ListenerOutput { get; set; }
    }
}
