using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.ComponentModel;

namespace Damdai.Utils
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
                    {
                        if (bits.Get(bit))
                        {
                            str +=  Enum.GetName(typeof(T), bit) + ", ";
                        }
                    }

                    if (str != null)
                    {
                        str = str.Trim(new char[] { ' ', ',' });
                    }
                }
            }

            return str;
        }
    }
}
