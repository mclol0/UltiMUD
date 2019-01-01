using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace Ultimud
{
    public partial class Server
    {
        public class SerializationBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                //Debug.WriteLine(typeName);
                //typeName = typeName.Replace("Ultimud.StatFactor`1[[System.Single, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]", "Ultimud.StatFactor");
                //typeName = typeName.Replace("f+Column", "Ultimud.StringTable+Column");
                return Type.GetType(typeName);
            }
        }
    }
}
