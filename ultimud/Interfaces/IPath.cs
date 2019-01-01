using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IPath : ILayer
    {
        float Thickness { get; set; }
    }
}
