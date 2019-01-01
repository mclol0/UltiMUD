using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Drawing.Design;
using System.Runtime.Serialization;

namespace Ultimud
{
    public enum TextTriggerBusyAction
    {
        Ignore,
        Enqueue,
        Interrupt
    }

    public interface ITextTrigger
    {
        string Trigger { get; set; }
        IScript Script { get; set; }
        List<Guid> RaceRequirements { get; set; }
        List<Guid> JobRequirements { get; set; }
        List<Guid> AbilityRequirements { get; set; }
        TextTriggerBusyAction BusyAction { get; set; }

        bool IsMatch(string str, IObject actor);
    }
}
