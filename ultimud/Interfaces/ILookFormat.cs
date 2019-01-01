using System;
using System.Collections.Generic;

namespace Ultimud
{
    public interface ILookFormat
    {
        string FormatObject(ICharacter ch, IObject o, int count);
    }
}
