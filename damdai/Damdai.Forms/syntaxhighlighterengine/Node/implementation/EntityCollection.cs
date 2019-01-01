using System;
using System.Collections;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a collection of entities.
	/// </summary>
	public class EntityCollection : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.EntityCollection"/> class.
		/// </summary>
		public EntityCollection()
		{
			//
		}

		/// <summary>
		/// Gets an <see cref="Damdai.Forms.Entity"/> object at the specified index.
		/// </summary>
		public Entity this[int index]
		{
			get
			{
				return (Entity)this.List[index];
			}
		}

		/// <summary>
		/// Adds an entity.
		/// </summary>
		/// <param name="entity">An <see cref="Damdai.Forms.Entity"/> object.</param>
		/// <returns>Index of the added object.</returns>
		public int Add(Entity entity)
		{
			return this.List.Add(entity);
		}

		/// <summary>
		/// Removes a entity.
		/// </summary>
		/// <param name="entity">An <see cref="Damdai.Forms.Entity"/> object.</param>
		public void Remove(Entity entity)
		{
			this.List.Remove(entity);
		}
	}
}