using System;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface IIdentifiable
    {
        Guid Id { get; }
        string Name { get; set; }
    }
}
