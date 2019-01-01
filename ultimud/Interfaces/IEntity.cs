using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface IEntity : IIdentifiable, ILocatable
    {
        IObject Parent
        {
            get;
            set;
        }

        bool CanHear();
        void Send(string str, IEntity actor);
        IStat GetStat(Guid id);
        IStat GetStat(string name);
    }
}
