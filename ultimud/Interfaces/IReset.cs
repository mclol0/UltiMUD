using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IReset : ILocatable
    {
        Damdai.Timer Timer { get; set; }
        IScript OnReset { get; set; }

        void Update();
    }
}
