using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface IContainer : IObject, ICloseable
    {
        int RemainingSpace
        {
            get;
        }

        [Obfuscation(Exclude = false)]
        T FindChild<T>(Guid id, bool searchChildren) where T : class, IIdentifiable;

        [Obfuscation(Exclude = false)]
        T FindChild<T>(string name, bool sightlimited, bool searchChildren) where T : class, IIdentifiable;
    }
}
