using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;

namespace Ultimud
{
    public class IStatFactorsToStringConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            string str = null;

            if (value != null)
            {
                if (value is List<IStatFactor>)
                {
                    List<IStatFactor> list = value as List<IStatFactor>;
                    foreach (IStatFactor statfactor in list)
                        if (statfactor.StatBlueprint != null)
                            str += statfactor.StatBlueprint.Name + " (" + statfactor.Factor + /*(Symbol == null ? "" : Symbol.ToString()) +*/ "), ";

                    if (str != null)
                        str = str.Trim(new char[] { ' ', ',' });
                }
            }

            return str;
        }
    }
}
