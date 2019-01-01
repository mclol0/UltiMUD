using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    public class StatCollectionConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            string str = null;

            if (value != null)
            {
                if (value is Dictionary<Guid, Stat>)
                {
                    Dictionary<Guid, Stat> collection = value as Dictionary<Guid, Stat>;
                    foreach (Stat stat in collection.Values)
                        if (stat.Max != 0)
                            str += stat.Name + " (" + stat.Current + "/" + stat.Max + "), ";

                    if (str != null)
                        str = str.Trim(' ', ',' );
                }
            }

            return str;
        }
    }
}
