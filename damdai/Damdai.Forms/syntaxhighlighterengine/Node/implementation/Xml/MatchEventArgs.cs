using System;
using System.Collections.Specialized;

namespace Damdai.Forms
{
	/// <summary>
	/// Enumeration of match types.
	/// </summary>
	public enum MatchType
	{
		/// <summary>
		/// The start tag.
		/// </summary>
		/// <example>
		/// &lt;label&gt;
		/// </example>
		StartTag = 1,

		/// <summary>
		/// The end tag.
		/// </summary>
		/// <example>
		/// &lt;/label&gt;
		/// </example>
		EndTag = 2,

		/// <summary>
		/// A single tag.
		/// </summary>
		/// <example>
		/// &lt;label /&gt;
		/// </example>
		StandaloneTag = 4,

		/// <summary>
		/// Comments.
		/// </summary>
		/// <example>
		/// &lt;!-- Comments here --&gt;
		/// </example>
		Comments = 8,

		/// <summary>
		/// The start identifier.
		/// </summary>
		/// <example>
		/// &lt;?
		/// </example>
		StartIdentifier = 16,

		/// <summary>
		/// The end identifier.
		/// </summary>
		/// <example>
		/// ?&gt;
		/// </example>
		EndIdentifier = 32
	}

	/// <summary>
	/// Represents the method that will handle the <see cref="Damdai.Forms.XmlScanner.Match"/> event of the 
	/// <see cref="Damdai.Forms.XmlScanner"/> class.
	/// </summary>
	public delegate void MatchEventHandler(object sender, MatchEventArgs e);

	/// <summary>
	/// Provides data for the <see cref="Damdai.Forms.XmlScanner.Match"/> event.
	/// </summary>
	[Serializable]
	public class MatchEventArgs : EventArgs
	{
		private int start;
		private int length;
		private string ns;
		private string tag;
		private NameValueCollection attributes;
		private MatchType type;

		/// <summary>
		/// Gets the start of the match.
		/// </summary>
		public int Start
		{
			get
			{
				return this.start;
			}
		}

		/// <summary>
		/// Gets the length of the match.
		/// </summary>
		public int Length
		{
			get
			{
				return this.length;
			}
		}
		
		/// <summary>
		/// Gets the namespace.
		/// </summary>
		public string Namespace
		{
			get
			{
				return this.ns;
			}
		}

		/// <summary>
		/// Gets the tag.
		/// </summary>
		public string Tag
		{
			get
			{
				return this.tag;
			}
		}

		/// <summary>
		/// Gets the attributes.
		/// </summary>
		public NameValueCollection Attributes
		{
			get
			{
				return this.attributes;
			}
		}

		/// <summary>
		/// Gets the type of match.
		/// </summary>
		public MatchType Type
		{
			get
			{
				return this.type;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.MatchEventArgs"/> class.
		/// </summary>
		/// <param name="start">The start of the tag to store in this event.</param>
		/// <param name="length">The length of the tag to store in the event.</param>
		/// <param name="ns">The namespace to store in the event.</param>
		/// <param name="tag">The tag to store in the event.</param>
		/// <param name="attributes">The attributes to store in the event.</param>
		/// <param name="type">The type of token to store in this event.</param>
		public MatchEventArgs(int start, int length, string ns, string tag, NameValueCollection attributes, MatchType type)
		{
			this.start = start;
			this.length = length;
			this.ns = ns;
			this.tag = tag;
			this.attributes = attributes;
			this.type = type;
		}
	}
}