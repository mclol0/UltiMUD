using System;
using System.Collections.Specialized;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a comment block node.
	/// </summary>
	public class CommentBlockNode : NodeBase
	{
		private EntityCollection entities;

		/// <summary>
		/// Gets the collection of entities which contains information about the start and end strings of comment blocks.
		/// </summary>
		public EntityCollection Entities
		{
			get
			{
				return this.entities;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CommentBlockNode"/> class.
		/// </summary>
		public CommentBlockNode()
		{
			this.entities = new EntityCollection();
		}
	}
}