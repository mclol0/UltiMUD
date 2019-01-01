using System;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an occurrence.
	/// </summary>
	public struct Occurrence
	{
		private int start;
		private int length;
		private INode node;

		/// <summary>
		/// Gets or sets the start index of the occurrence.
		/// </summary>
		public int Start
		{
			get
			{
				return this.start;
			}
			set
			{
				if (value != this.start)
				{
					this.start = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the length of the occurrence.
		/// </summary>
		public int Length
		{
			get
			{
				return this.length;
			}
			set
			{
				if (value != this.length)
				{
					this.length = value;
				}
			}
		}

		/// <summary>
		/// Gets the node the occurrence represents.
		/// </summary>
		public INode Node
		{
			get
			{
				return this.node;
			}
		}
		
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.Occurrence"/> struct.
		/// </summary>
		/// <param name="start">The start index of the occurrence.</param>
		/// <param name="length">The length of the occurrence.</param>
		/// <param name="node">The node the occurrence represents.</param>
		public Occurrence(int start, int length, INode node)
		{
			this.start = start;
			this.length = length;
			this.node = node;
		}
	}
}