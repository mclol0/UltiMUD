using System;
using System.Collections;
using System.Collections.Generic;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a collection of occurrences.
	/// </summary>
	public class OccurrenceCollection : IList<Occurrence>
	{
        private List<Occurrence> items;

		/// <summary>
		/// Occurs when a new occurrence is added to the <see cref="Damdai.Forms.OccurrenceCollection"/>.
		/// </summary>
		internal event OccurrenceEventHandler OccurrenceAdded;

		/// <summary>
		/// Occurs when an occurrence is removed from the <see cref="Damdai.Forms.OccurrenceCollection"/>.
		/// </summary>
		internal event OccurrenceEventHandler OccurrenceRemoved;


        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count {
            get {
                return this.items.Count;
            }
        }

        /// <summary>
        /// Checks whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly {
            get {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the occurrence at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Occurrence this[int index] {
            get {
                return this.items[index];
            }
            set {
                throw new NotSupportedException();
            }
        }

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.OccurrenceCollection"/> class.
		/// </summary>
        public OccurrenceCollection()
        {
            this.items = new List<Occurrence>();
        }

        /// <summary>
        /// Adds an occurrence to the list.
        /// </summary>
        /// <param name="item"></param>
        public virtual void Add(Occurrence item)
        {
            this.items.Add(item);
            this.OnOccurrenceAdded(new OccurrenceEventArgs(item));
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            this.items.Clear();
        }

        /// <summary>
        /// Checks whether the collection contains the specified item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(Occurrence item)
        {
            return this.items.Contains(item);
        }

        /// <summary>
        /// Copies the contents of the collection to the array at the specified index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(Occurrence[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the index of the specified occurrence.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(Occurrence item)
        {
            return this.items.IndexOf(item);
        }

        /// <summary>
        /// Inserts an occurrence at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, Occurrence item)
        {
            this.items.Insert(index, item);
            this.OnOccurrenceAdded(new OccurrenceEventArgs(item));
        }

        /// <summary>
        /// Removes an occurrence from the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(Occurrence item)
        {
            bool result = this.items.Remove(item);
            this.OnOccurrenceRemoved(new OccurrenceEventArgs(item));
            return result;
        }

        /// <summary>
        /// Removes an occurrence from the list.
        /// </summary>
        /// <param name="index"></param>
        public virtual void RemoveAt(int index)
        {
            Occurrence item = this.items[index];
            this.Remove(item);
        }

        /// <summary>
        /// Sorts the collection.
        /// </summary>
        /// <param name="comparison"></param>
        public virtual void Sort(Comparison<Occurrence> comparison){
            this.items.Sort(comparison);
        }

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Occurrence> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

		/// <summary>
		/// Raises the <see cref="Damdai.Forms.OccurrenceCollection.OccurrenceAdded"/> event.
		/// </summary>
		/// <param name="e">An <see cref="Damdai.Forms.OccurrenceEventArgs"/> that contains the event data.</param>
		protected virtual void OnOccurrenceAdded(OccurrenceEventArgs e)
		{
			if (this.OccurrenceAdded != null)
				this.OccurrenceAdded(this, e);
		}

		/// <summary>
		/// Raises the <see cref="Damdai.Forms.OccurrenceCollection.OccurrenceRemoved"/> event.
		/// </summary>
		/// <param name="e">An <see cref="Damdai.Forms.OccurrenceEventArgs"/> that contains the event data.</param>
		protected virtual void OnOccurrenceRemoved(OccurrenceEventArgs e)
		{
			if (this.OccurrenceRemoved != null)
				this.OccurrenceRemoved(this, e);
		}
    }
}