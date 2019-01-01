using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class WhoFormat : Formatter
    {
        string footer_;

        [Obfuscation(Exclude = false)]
        public override string[] FormatFields(object o)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        [Obfuscation(Exclude = false)]
        public override string ToString(string sortColumn, object o)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
