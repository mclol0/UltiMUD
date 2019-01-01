using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface IObject : IIdentifiable
    {
        Object Parent
        {
            get;
            set;
        }

        Location Location
        {
            get;
            set;
        }

        int Distance(Point p);
        int Distance(IObject o);
        bool CanHear();
        void Send(string str, IObject actor);
        Stat GetStat(Guid id);
        Stat GetStat(string name);
    }
}
