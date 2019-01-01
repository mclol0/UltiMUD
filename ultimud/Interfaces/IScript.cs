using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.ComponentModel;

namespace Ultimud
{
    public interface IScript : IIdentifiable
    {
        string Code { get; set; }

        bool Test(out string error);
        bool Test(string code, out string error);
    }
}
