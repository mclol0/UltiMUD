using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    public class BlueprintsToNameConverter<T> : TypeConverter where T : IIdentifiable
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            string str = null;

            if (value != null)
            {
                ICollection<T> list = null;
                if (value is ICollection<T>)
                    list = value as ICollection<T>;
                else if (value is Dictionary<Guid, int>)
                    list = ((Dictionary<T, int>)value).Keys;

                if (list != null)
                {
                    foreach (T t in list)
                        str += t.Name + ", ";

                    if (str != null)
                        str = str.Trim(' ', ',');
                }
            }

            return str;
        }
    }
}
