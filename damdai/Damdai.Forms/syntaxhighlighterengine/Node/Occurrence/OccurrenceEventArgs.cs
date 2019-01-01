using System;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents the method that will handle the <see cref="Damdai.Forms.INode.OccurrenceAdded"/> and 
	/// <see cref="Damdai.Forms.INode.OccurrenceRemoved"/> events of the 
	/// <see cref="Damdai.Forms.INode"/> implementation classes.
	/// </summary>
	public delegate void OccurrenceEventHandler(object sender, OccurrenceEventArgs e);

	/// <summary>
	/// Provides data for the <see cref="Damdai.Forms.INode.OccurrenceAdded"/> and
	/// <see cref="Damdai.Forms.INode.OccurrenceRemoved"/> events.
	/// </summary>
	[Serializable]
	public class OccurrenceEventArgs : EventArgs
	{
		private Occurrence occurrence;

		/// <summary>
		/// Gets the occurrence used by this event.
		/// </summary>
		public Occurrence Occurrence
		{
			get
			{
				return this.occurrence;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.OccurrenceEventArgs"/> class.
		/// </summary>
		/// <param name="occ">The occurrence to store in this event.</param>
		public OccurrenceEventArgs(Occurrence occ)
		{
			this.occurrence = occ;
		}
	}
}