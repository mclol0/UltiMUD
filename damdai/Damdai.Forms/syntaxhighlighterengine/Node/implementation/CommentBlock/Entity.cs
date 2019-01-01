using System;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an entity.
	/// </summary>
	public class Entity
	{
		private string start;
		private string end;

		/// <summary>
		/// Gets the string which indicates the start of a comment block.
		/// </summary>
		public string Start
		{
			get
			{
				return this.start;
			}
		}

		/// <summary>
		/// Gets the string which indicates the end of a comment block.
		/// </summary>
		public string End
		{
			get
			{
				return this.end;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.Entity"/> struct.
		/// </summary>
		/// <param name="start">The string which indicates the start of a comment block.</param>
		/// <param name="end">The string which indicates the end of a comment block.</param>
		public Entity(string start, string end)
		{
			this.start = start;
			this.end = end;
		}
	}
}