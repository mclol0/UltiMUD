using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

namespace Ultimud
{
    public partial class Server
    {
        [Obfuscation(Exclude = false)]
        public class ObjectCollectionConverter<T> : TypeConverter where T : IIdentifiable
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                string str = null;

                if (value != null)
                {
                    ICollection<Guid> list = null;
                    if (value is ICollection<Guid>)
                        list = value as ICollection<Guid>;
                    else if (value is Dictionary<Guid, int>)
                        list = ((Dictionary<Guid, int>)value).Keys;

                    if (list != null)
                    {
                        foreach (Guid id in list)
                        {
                            Type type = typeof(T);
                            Dictionary<Guid, IIdentifiable> dataset = null;
                            Trace.Assert(Server.Instance.DataSets.TryGetValue(type, out dataset), type.Name + " dataset not found.");
                            IIdentifiable t;
                            if (dataset.TryGetValue(id, out t))
                                str += t.Name + ", ";
                        }

                        if (str != null)
                            str = str.Trim(' ', ',');
                    }
                }

                return str;
            }
        }
    }
}
