using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Damdai;

namespace Ultimud
{
    public interface IStatusEffect : ICloneable
    {
        IStatusEffectBlueprint Blueprint { get; set; }
        Timer Duration { get; set; }
        bool IsPermanent { get; }

        void Undo(IObject o);
        void Update(IObject o);
    }
}
