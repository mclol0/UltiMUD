using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Provides an abstract node class.
	/// </summary>
	public abstract class NodeBase : INode
	{
		private Color backColor;
		private Color foreColor;
        private Font font;
		private string navigateUrl;
		private OccurrenceCollection occurrences;

		/// <summary>
		/// Occurs when a new occurrence is added to the <see cref="Damdai.Forms.INode.Occurrences"/>.
		/// </summary>
		public event OccurrenceEventHandler OccurrenceAdded;

		/// <summary>
		/// Occurs when an occurrence is removed from the <see cref="Damdai.Forms.INode.Occurrences"/>.
		/// </summary>
		public event OccurrenceEventHandler OccurrenceRemoved;

		/// <summary>
		/// Gets or sets the foreground color of the node.
		/// </summary>
		public Color BackColor
		{
			get
			{
				return this.backColor;
			}
			set
			{
				if (value != this.backColor)
				{
					this.backColor = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the font of the text represented by the occurrences of the node.
		/// </summary>
		public Font Font
		{
			get
			{
				return this.font;
			}
			set
			{
				if (value != this.font)
				{
					this.font = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the foreground color of the node.
		/// </summary>
		public Color ForeColor
		{
			get
			{
				return this.foreColor;
			}
			set
			{
				if (value != this.foreColor)
				{
					this.foreColor = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the navigate Url.
		/// </summary>
		public string NavigateUrl
		{
			get
			{
				return this.navigateUrl;
			}
			set
			{
				if (value != this.navigateUrl)
				{
					this.navigateUrl = value;
				}
			}
		}

		/// <summary>
		/// Gets a collection of occurrences of the <see cref="Damdai.Forms.INode"/> implementation.
		/// </summary>
		public OccurrenceCollection Occurrences
		{
			get
			{
				return this.occurrences;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.NodeBase"/> class.
		/// </summary>
		public NodeBase()
		{
			this.backColor = Color.Transparent;
			this.foreColor = Color.Black;
			this.font = new Font(new FontFamily("Courier New"), 10, GraphicsUnit.Point);
			this.occurrences = new OccurrenceCollection();
			this.occurrences.OccurrenceAdded += new OccurrenceEventHandler(this.occurrences_OccurrenceAdded);
			this.occurrences.OccurrenceRemoved += new OccurrenceEventHandler(this.occurrences_OccurrenceRemoved);
		}

		/// <summary>
		/// Handles the occurrence added event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void occurrences_OccurrenceAdded(object sender, OccurrenceEventArgs e)
		{
			this.OnOccurrenceAdded(e);
		}

		/// <summary>
		/// Handles the occurrence removed event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void occurrences_OccurrenceRemoved(object sender, OccurrenceEventArgs e)
		{
			this.OnOccurrenceRemoved(e);
		}

		/// <summary>
		/// Raises the <see cref="Damdai.Forms.NodeBase.OccurrenceAdded"/> event.
		/// </summary>
		/// <param name="e">An <see cref="Damdai.Forms.OccurrenceEventArgs"/> that contains the event data.</param>
		protected virtual void OnOccurrenceAdded(OccurrenceEventArgs e)
		{
			if (this.OccurrenceAdded != null)
				this.OccurrenceAdded(this, e);
		}

		/// <summary>
		/// Raises the <see cref="Damdai.Forms.NodeBase.OccurrenceRemoved"/> event.
		/// </summary>
		/// <param name="e">An <see cref="Damdai.Forms.OccurrenceEventArgs"/> that contains the event data.</param>
		protected virtual void OnOccurrenceRemoved(OccurrenceEventArgs e)
		{
			if (this.OccurrenceRemoved != null)
				this.OccurrenceRemoved(this, e);
		}
	}
}