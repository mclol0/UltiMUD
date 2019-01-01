using System;
using System.Collections;
using System.Collections.Generic;

namespace Damdai.Collections
{
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
			if(this.Count == 0)
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
			if(useObjectsComparison_ && !(t is IComparable)) 
				throw new ArgumentException("The Heap is set to use the IComparable interface of objects, and the object to add does not implement the IComparable interface.");
			if(!addDuplicates_ && Contains(t))
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
}