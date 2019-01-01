using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface IContainer : ICloseable
    {
        int RemainingSpace { get; }

        [Obfuscation(Exclude = false)]
        T FindChild<T>(T t, bool searchchildren) where T : class, IObject;

        [Obfuscation(Exclude = false)]
        T FindChild<T>(string name, bool sightlimited, bool searchchildren) where T : class, IObject;
    }
}
