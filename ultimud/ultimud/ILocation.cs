using System;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    interface ILocation
    {
        Location Location
        {
            get;
            set;
        }
    }
}
