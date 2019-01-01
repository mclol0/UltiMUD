using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface IPortal : IContainer, ICloseable
    {
        Location Destination
        {
            get;
            set;
        }

        [Description("The message that is shown when someone enters this portal.")]
        String EnterMessage
        {
            get;
            set;
        }
        
        [Description("The message that is shown when someone exits this portal.")]
        String ExitMessage
        {
            get;
            set;
        }
        
        bool AutoEnter
        {
            get;
            set;
        }
    }
}
