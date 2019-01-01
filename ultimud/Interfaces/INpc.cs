using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Damdai;

namespace Ultimud
{
    public interface INpc : ICharacter
    {
        IScript OnText { get; set; }
    }
}
