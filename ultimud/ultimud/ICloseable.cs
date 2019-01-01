using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public interface ICloseable : IObject
    {
        bool Closeable
        {
            get;
            set;
        }

        bool Closed
        {
            get;
            set;
        }

        bool Locked
        {
            get;
            set;
        }

        [Editor(typeof(Server.ObjectBlueprintSelector<Item>), typeof(UITypeEditor))]
        Item Key
        {
            get;
            set;
        }
    }
}
