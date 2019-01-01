using System;
using System.Collections;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a collection of highlighters.
	/// </summary>
	public class HighlighterCollection : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.HighlighterCollection"/> class.
		/// </summary>
		public HighlighterCollection()
		{
			//
		}

		/// <summary>
		/// Gets the highlighter at the specified index.
		/// </summary>
		public HighlighterBase this[int index]
		{
			get
			{
				return (HighlighterBase)this.List[index];
			}
		}

		/// <summary>
		/// Gets the highlighter with the specified name.
		/// </summary>
		public HighlighterBase this[string name]
		{
			get
			{
				HighlighterBase highlighter = null;
				for (int i = 0; i < this.List.Count; i++)
				{
					highlighter = (HighlighterBase)this.List[i];
					if (highlighter.Name.ToLower() == name.ToLower())
						return highlighter;
				}
				return null;
			}
		}

		/// <summary>
		/// Adds an highlighter.
		/// </summary>
		/// <param name="highlighter">The highlighter to add.</param>
		/// <returns>Index of the added highlighter.</returns>
		public int Add(HighlighterBase highlighter)
		{
			return this.List.Add(highlighter);
		}

		/// <summary>
		/// Removes an highlighter.
		/// </summary>
		/// <param name="highlighter">The highlighter to remove.</param>
		public void Remove(HighlighterBase highlighter)
		{
			this.List.Remove(highlighter);
		}
	}
}