using System;
using System.Reflection;
using System.Drawing;

namespace Ultimud
{
    public interface ILocatable
    {
        Location Location { get; set; }

        bool Intersects(Rectangle rect);
    }
}
