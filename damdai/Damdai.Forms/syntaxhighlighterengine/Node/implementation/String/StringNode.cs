using System;
using System.Collections.Specialized;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a string node.
	/// </summary>
	public class StringNode : NodeBase
	{
		private EntityCollection entities;

		/// <summary>
		/// Gets the collection of entities which contains information about the start, end and escape strings of strings.
		/// </summary>
		public EntityCollection Entities
		{
			get
			{
				return this.entities;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringNode"/> class.
		/// </summary>
		public StringNode()
		{
			this.entities = new EntityCollection();
		}
	}
}