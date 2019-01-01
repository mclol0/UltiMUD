using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an interface for nodes.
	/// </summary>
	public interface INode
	{
		/// <summary>
		/// Occurs when a new occurrence is added to the <see cref="Damdai.Forms.INode.Occurrences"/>.
		/// </summary>
		event OccurrenceEventHandler OccurrenceAdded;

		/// <summary>
		/// Occurs when an occurrence is removed from the <see cref="Damdai.Forms.INode.Occurrences"/>.
		/// </summary>
		event OccurrenceEventHandler OccurrenceRemoved;

		/// <summary>
		/// Gets or sets the foreground color of the node.
		/// </summary>
		Color BackColor
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the font of the text represented by the occurrences of the node.
		/// </summary>
		Font Font
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the foreground color of the node.
		/// </summary>
		Color ForeColor
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the navigate Url.
		/// </summary>
		string NavigateUrl
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a collection of occurrences of the <see cref="Damdai.Forms.INode"/> implementation.
		/// </summary>
		OccurrenceCollection Occurrences
		{
			get;
		}
	}
}