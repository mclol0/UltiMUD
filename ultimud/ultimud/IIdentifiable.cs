using System;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface IIdentifiable
    {
        Guid Id
        {
            get;
            set;
        }

        string Name
        {
            get;
            set;
        }
    }
}
