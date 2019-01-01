using System;
using System.Collections;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a collection of scanners.
	/// </summary>
	public class ScannerCollection : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.ScannerCollection"/> class.
		/// </summary>
		public ScannerCollection()
		{
			//
		}

		/// <summary>
		/// Gets an <see cref="Damdai.Forms.IScanner"/> object at the specified index.
		/// </summary>
		public IScanner this[int index]
		{
			get
			{
				return (IScanner)this.List[index];
			}
		}

		/// <summary>
		/// Gets an <see cref="Damdai.Forms.IScanner"/> object at the specified index.
		/// </summary>
		public IScanner this[string id]
		{
			get
			{
				for (int i = 0; i < this.Count; i++)
					if (this[i].ID == id)
						return this[i];
				return null;
			}
		}

		/// <summary>
		/// Adds a scanner.
		/// </summary>
		/// <param name="scanner">An <see cref="Damdai.Forms.IScanner"/> object.</param>
		/// <returns>Index of the added object.</returns>
		public int Add(IScanner scanner)
		{
			return this.List.Add(scanner);
		}

		/// <summary>
		/// Removes a scanner.
		/// </summary>
		/// <param name="scanner">An <see cref="Damdai.Forms.IScanner"/> object.</param>
		public void Remove(IScanner scanner)
		{
			this.List.Remove(scanner);
		}
	}
}