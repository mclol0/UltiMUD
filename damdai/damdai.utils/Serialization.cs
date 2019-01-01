using System;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Messaging;

namespace Damdai.Serialization
{
    public class Xml
    {
        public static void Serialize(object o, string filename)
        {
            using (StreamWriter w = File.CreateText(filename))
            {
                XmlSerializer serializer = new XmlSerializer(o.GetType());
                serializer.Serialize(w, o);
            }
        }

        public static T Deserialize<T>(string filename) where T : class, new()
        {
            using (StreamReader r = File.OpenText(filename))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return serializer.Deserialize(r) as T;
            }
        }
    }

    public class Binary
    {
        public static byte[] Serialize(object o)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, o);
                return stream.ToArray();
            }
        }

        public static object Deserialize(byte[] data, System.Runtime.Serialization.SerializationBinder binder)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Binder = binder;
                return formatter.Deserialize(stream);
            }
        }

        public static void Serialize(object o, string filename)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filename)))
                writer.Write(Serialize(o));
        }

        public static T Deserialize<T>(string filename, System.Runtime.Serialization.SerializationBinder binder) where T : class
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
                return Deserialize(reader.ReadBytes((int)reader.BaseStream.Length), binder) as T;
        }
    }
}
