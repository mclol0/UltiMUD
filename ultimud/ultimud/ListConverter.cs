using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Ultimud
{
    public class ListConverter<T> : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            string str = null;

            if (value != null)
            {
                if (value is List<T>)
                {
                    List<T> list = value as List<T>;
                    foreach (T t in list)
                    {
                        str += t.ToString() + ", ";
                    }

                    if (str != null)
                    {
                        str = str.Trim(' ', ',');
                    }
                }
            }

            return str;
        }
    }
}
