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
        public class IdToNameConverter<T> : TypeConverter where T : IIdentifiable
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                Guid id = (Guid)value;

                if (id != Guid.Empty)
                {
                    Type type = typeof(T);
                    Dictionary<Guid, IIdentifiable> dataset = null;
                    Trace.Assert(Server.Instance.DataSets.TryGetValue(type, out dataset), type.Name + " dataset not found.");
                    IIdentifiable o = null;
                    if (dataset.TryGetValue(id, out o))
                        return o.Name;
                }

                return null;
            }
        }
    }
}
