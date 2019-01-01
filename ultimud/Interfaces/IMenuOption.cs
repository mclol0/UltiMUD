using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IMenuOption
    {
        string Key { get; }
        string Text { get; }
        string[] Columns { get; }
        object Tag { get; }
    }
}
