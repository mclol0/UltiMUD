using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class StatsFormat : Formatter
    {
        public StatsFormat()
        {
        }

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
