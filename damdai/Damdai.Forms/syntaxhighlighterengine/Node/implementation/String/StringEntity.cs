using System;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a string entity.
	/// </summary>
	public class StringEntity : Entity
	{
		private string escape;

		/// <summary>
		/// Gets the string which is used to escape the end.
		/// </summary>
		public string Escape
		{
			get
			{
				return this.escape;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringEntity"/> struct.
		/// </summary>
		/// <param name="start">The string which indicates the start of a string.</param>
		/// <param name="end">The string which indicates the end of a string.</param>
		/// <param name="escape">The string which is used to escape the end.</param>
		public StringEntity(string start, string end, string escape) : base(start, end)
		{
			this.escape = escape;
		}
	}
}