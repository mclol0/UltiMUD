using System;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    public interface ISocial : IIdentifiable, ICommandProcessor
    {
        String Output { get; set; }
        String TargetOutput { get; set; }
    }
}
