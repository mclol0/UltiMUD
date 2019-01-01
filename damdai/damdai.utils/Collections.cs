using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Damdai.Collections
{
    public class Util
    {
        public static bool IsEmpty(BitArray array)
        {
            for (int x = 0; x < array.Count; ++x)
                if (array.Get(x) == true)
                    return false;
            return true;
        }
        public static List<T> CopyList<T>(List<T> list)
        {
            lock (list)
                return new List<T>(list);
        }
        public static Dictionary<K, V> CopyDictionary<K, V>(Dictionary<K, V> dictionary)
        {
            Dictionary<K, V> newdictionary = new Dictionary<K, V>();
            lock (dictionary)
            {
                foreach (KeyValuePair<K, V> pair in dictionary)
                    newdictionary.Add(pair.Key, pair.Value);
            }
            return newdictionary;
        }
        public static Dictionary<K, V2> Convert<K, V1, V2>(Dictionary<K, V1> dictionary) where V2 : class
        {
            Dictionary<K, V2> newDictionary = new Dictionary<K, V2>();
            if (dictionary == null)
                return newDictionary;

            foreach (KeyValuePair<K, V1> pair in dictionary)
                newDictionary.Add(pair.Key, pair.Value as V2);
            return newDictionary;
        }
        public static string ListToString(ICollection list, string separator)
        {
            string str = string.Empty;
            foreach (object o in list)
                str += o.ToString() + separator;

            return str.TrimEnd(separator.ToCharArray());
        }
        public static bool Compare<K, V>(Dictionary<K, V> d1, Dictionary<K, V> d2)
        {
            if (d1.Count != d2.Count)
                return false;

            foreach (KeyValuePair<K, V> p in d1)
                if (!d2.ContainsKey(p.Key) || !d2[p.Key].Equals(p.Value))
                    return false;

            return true;
        }
        public static bool Compare(BitArray b1, BitArray b2)
        {
            if(b1.Count != b2.Count)
                return false;

            for (int x = 0; x < b1.Length; ++x)
                if (b1[x] != b2[x])
                    return false;

            return true;
        }
        public static bool Compare<T>(List<T> l1, List<T> l2)
        {
            if (l1.Count != l2.Count)
                return false;

            return l1.TrueForAll(delegate(T t) { return l2.Contains(t); });
        }
    }

    /// <summary>
    /// The Heap allows to maintain a list sorted as long as needed.
    /// If no IComparer interface has been provided at construction, then the list expects the Objects to implement IComparer.
    /// If the list is not sorted it behaves like an ordinary list.
    /// When sorted, the list's "Add" method will put new objects at the right place.
    /// As well the "Contains" and "IndexOf" methods will perform a binary search.
    /// </summary>
    public class Heap<T> : List<T>
    {
        #region Private Members

        private bool useObjectsComparison_ = true;
        private bool addDuplicates_ = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// Since no IComparer is provided here, added objects must implement the IComparer interface.
        /// </summary>
        public Heap()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// If set to true, it will not be possible to add an object to the list if its value is already in the list.
        /// </summary>
        public bool AddDuplicates
        {
            set
            {
                addDuplicates_ = value;
            }
            get
            {
                return addDuplicates_;
            }
        }

        #endregion

        #region IList Members

        /// <summary>
        /// IList implementation.
        /// Adds the object at the right place.
        /// </summary>
        /// <param name="O">The object to add.</param>
        /// <returns>The index where the object has been added.</returns>
        /// <exception cref="ArgumentException">The Heap is set to use object's IComparable interface, and the specifed object does not implement this interface.</exception>
        new public void Add(T t)
        {
            if (ObjectIsCompliant(t))
            {
                int index = IndexOf(t);
                int newIndex = index >= 0 ? index : -index - 1;
                if (newIndex >= this.Count)
                    base.Add(t);
                else
                    base.Insert(newIndex, t);
            }
        }

        /// <summary>
        /// IList implementation.
        /// Cannot be used on a Heap.
        /// </summary>
        /// <param name="Index">The index before which the object must be added.</param>
        /// <param name="O">The object to add.</param>
        /// <exception cref="InvalidOperationException">Insert method cannot be called on a Heap.</exception>
        new public void Insert(int index, T t)
        {
            throw new InvalidOperationException("Insert method cannot be called on a Heap.");
        }

        #endregion

        #region Delegate Members

        /// <summary>
        /// Defines an equality for two objects
        /// </summary>
        public delegate bool Equality(object Object1, object Object2);

        #endregion

        #region Overridden Members

        /// <summary>
        /// Object.ToString() override.
        /// Build a string to represent the list.
        /// </summary>
        /// <returns>The string refecting the list.</returns>
        public override string ToString()
        {
            string OutString = "{";
            for (int i = 0; i < this.Count; i++)
                OutString += this[i].ToString() + (i != this.Count - 1 ? "; " : "}");
            return OutString;
        }

        /// <summary>
        /// Object.Equals() override.
        /// </summary>
        /// <returns>true if object is equal to this, otherwise false.</returns>
        public override bool Equals(object o)
        {
            Heap<T> heap = (Heap<T>)o;
            if (heap.Count != this.Count)
                return false;
            for (int i = 0; i < this.Count; i++)
                if (!heap[i].Equals(this[i]))
                    return false;
            return true;
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Cannot be called on a Heap.
        /// </summary>
        /// <param name="Index">The index before which the objects must be added.</param>
        /// <param name="C">The object to add.</param>
        /// <exception cref="InvalidOperationException">Insert cannot be called on a Heap.</exception>
        public void InsertRange(int Index, ICollection C)
        {
            throw new InvalidOperationException("Insert cannot be called on a Heap.");
        }

        /// <summary>
        /// Returns the topmost object on the list and removes it from the list
        /// </summary>
        /// <returns>Returns the topmost object on the list</returns>
        public T Pop()
        {
            if (this.Count == 0)
                throw new InvalidOperationException("The heap is empty.");
            T t = this[this.Count - 1];
            this.RemoveAt(this.Count - 1);
            return t;
        }

        /// <summary>
        /// Pushes an object on list. It will be inserted at the right spot.
        /// </summary>
        /// <param name="Object">Object to add to the list</param>
        /// <returns></returns>
        public void Push(T t)
        {
            this.Add(t);
        }

        #endregion

        #region Private Members

        private bool ObjectIsCompliant(T t)
        {
            if (useObjectsComparison_ && !(t is IComparable))
                throw new ArgumentException("The Heap is set to use the IComparable interface of objects, and the object to add does not implement the IComparable interface.");
            if (!addDuplicates_ && Contains(t))
                return false;
            return true;
        }

        private class Comparison : IComparer
        {
            public int Compare(object Object1, object Object2)
            {
                IComparable C = Object1 as IComparable;
                return C.CompareTo(Object2);
            }
        }

        #endregion
    }

    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}