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
                if (value is Dictionary<IStatBlueprint, IStat>)
                {
                    Dictionary<IStatBlueprint, IStat> collection = value as Dictionary<IStatBlueprint, IStat>;
                    foreach (IStat stat in collection.Values)
                        if (stat.Max != 0)
                            str += stat.Blueprint.Name + " (" + stat.Current + "/" + stat.Max + "), ";

                    if (str != null)
                        str = str.Trim(' ', ',' );
                }
            }

            return str;
        }
    }
}
