using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface INpcReset : IReset
    {
        ILayer Boundary { get; set; }
    }
}
