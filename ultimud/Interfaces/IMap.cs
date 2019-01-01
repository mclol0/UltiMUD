using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Ultimud
{
    public interface IMap
    {
        string[,] Grid { get; set; }
        int Size { get; }
        Telnet.Color[,] Color { get; set; }
        Point Offset { get; }
        IObject Pov { get; }
    }
}
