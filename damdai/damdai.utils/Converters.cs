using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Damdai.TypeConverters
{
    public class BitArrayConverter<T> : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            string str = null;

            if (value != null)
            {
                if (value is BitArray)
                {
                    BitArray bits = value as BitArray;
                    foreach (int bit in Enum.GetValues(typeof(T)))
                        if (bits.Get(bit))
                            str += Enum.GetName(typeof(T), bit) + ", ";

                    if (str != null)
                        str = str.Trim(new char[] { ' ', ',' });
                }
            }

            return str;
        }
    }

    public class ListToStringConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            string str = string.Empty;

            if (value != null)
            {
                foreach (object o in (ICollection)value)
                    str += o.ToString() + ", ";
            }

            return str.TrimEnd(',', ' ');
        }
    }

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
                        str += t.ToString() + ", ";
                    if (str != null)
                        str = str.Trim(' ', ',');
                }
            }

            return str;
        }
    }
}
