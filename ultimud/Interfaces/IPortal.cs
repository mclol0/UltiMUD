using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    public interface IPortal : IObject, ICloseable
    {
        bool AutoEnter { get; set; }
        Location Destination { get; set; }
        String EnterMessage { get; set; }
        String ExitMessage { get; set; }
    }
}
