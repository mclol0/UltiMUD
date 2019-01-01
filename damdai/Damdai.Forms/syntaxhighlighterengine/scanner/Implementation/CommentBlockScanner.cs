using System;
using System.Drawing;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a comment block scanner.
	/// </summary>
	public class CommentBlockScanner : ScannerBase
	{
		private CommentBlockNode commentBlockNode;

		/// <summary>
		/// Gets or sets the comment block node.
		/// </summary>
		public CommentBlockNode CommentBlockNode
		{
			get
			{
				return this.commentBlockNode;
			}
			set
			{
				if (value != this.commentBlockNode)
				{
					this.commentBlockNode = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CommentBlockScanner"/> class.
		/// </summary>
		public CommentBlockScanner() : this(null, null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CommentBlockScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public CommentBlockScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : this(tokenizer, scannerResult, new CommentBlockNode())
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CommentBlockScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <param name="commentBlockNode">A <see cref="Damdai.Forms.CommentBlockNode"/> object.</param>
		public CommentBlockScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult, CommentBlockNode commentBlockNode) : base(tokenizer, scannerResult)
		{
			this.commentBlockNode = commentBlockNode;
			this.SetID("CommentBlockScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new CommentBlockScanner(tokenizer, scannerResult);
		}

		/// <summary>
		/// Loads the state of the scanner.
		/// </summary>
		/// <param name="state">An <see cref="System.Object"/> that contains the state of the scanner.</param>
		public override void LoadState(object state)
		{
			XmlElement element = (XmlElement)state;
			
			// Load settings.
			FontConverter converter = new FontConverter();
			this.commentBlockNode.BackColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='BackColor']").InnerText);
			this.commentBlockNode.ForeColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='ForeColor']").InnerText);
			this.commentBlockNode.Font = (Font)converter.ConvertFromString(element.SelectSingleNode("settings/setting[@name='Font']").InnerText);
			this.commentBlockNode.NavigateUrl = element.SelectSingleNode("settings/setting[@name='NavigateUrl']").InnerText;

			// Load entities.
			foreach (XmlElement entityElement in element["entities"].ChildNodes)
			{
				this.commentBlockNode.Entities.Add(new Entity(entityElement.Attributes["start"].Value, entityElement.Attributes["end"].Value));
			}
		}

		/// <summary>
		/// Saves the current state of the scanner.
		/// </summary>
		/// <param name="container">The container which will contain the state.</param>
		/// <returns>An <see cref="System.Object"/> that contains the state of the scanner.</returns>
		public override object SaveState(object container)
		{
			XmlDocument document = (XmlDocument)container;
			XmlElement element = (XmlElement)base.SaveState(container);
			
			// Save settings.
			XmlElement settingRootElement = document.CreateElement("settings");
			element.AppendChild(settingRootElement);

			FontConverter converter = new FontConverter();
			settingRootElement.AppendChild(this.CreateSetting(document, "BackColor", ColorTranslator.ToHtml(this.commentBlockNode.BackColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "ForeColor", ColorTranslator.ToHtml(this.commentBlockNode.ForeColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "Font", converter.ConvertToString(this.commentBlockNode.Font)));
			settingRootElement.AppendChild(this.CreateSetting(document, "NavigateUrl", this.commentBlockNode.NavigateUrl));

			// Save entities.
			XmlElement entityRootElement = document.CreateElement("entities");
			element.AppendChild(entityRootElement);
			XmlElement entityElement;
			foreach (Entity entity in this.commentBlockNode.Entities)
			{
				entityElement = document.CreateElement("entity");
				entityElement.SetAttribute("start", entity.Start);
				entityElement.SetAttribute("end", entity.End);
				entityRootElement.AppendChild(entityElement);
			}

			return element;
		}

		/// <summary>
		/// Creates a steting.
		/// </summary>
		/// <param name="document">The document which will contain the setting.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		/// <returns>The <see cref="System.Xml.XmlElement"/> which represents the setting.</returns>
		private XmlElement CreateSetting(XmlDocument document, string name, string value)
		{
			XmlElement element = document.CreateElement("setting");
			element.SetAttribute("name", name);
			element.InnerText = value;
			return element;
		}

		/// <summary>
		/// Scans a token.
		/// </summary>
		/// <remarks>
		/// An <see cref="Damdai.Forms.IScanner"/> implementation will generally have a reference to a 
		/// <see cref="Damdai.Forms.NodeCollection"/> which will be used to store results of a scan.
		/// </remarks>
		/// <param name="token">A token from the source code.</param>
		public override void Scan(string token)
		{
			if (!this.Enabled)
			{
				if (this.Child != null)
				{
					this.Child.Scan(token);
				}
			}
			else
			{
				bool isMatch = false;

				for (int i = 0; i < this.commentBlockNode.Entities.Count; i++)
				{
					if ((this.Tokenizer.Position + this.commentBlockNode.Entities[i].Start.Length) <= this.Tokenizer.Source.Length)
					{
						if (this.Tokenizer.GetNextTokens(this.commentBlockNode.Entities[i].Start.Length) == this.commentBlockNode.Entities[i].Start)
						{
							isMatch = true;

							int endOfLineIndex = this.Tokenizer.Source.IndexOf(this.commentBlockNode.Entities[i].End, this.Tokenizer.Position);
							if (endOfLineIndex == -1)
								endOfLineIndex = this.Tokenizer.Source.Length;
							else
								endOfLineIndex += this.commentBlockNode.Entities[i].End.Length;

							this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, endOfLineIndex - this.Tokenizer.Position, this.commentBlockNode));
							this.Tokenizer.MoveTo(endOfLineIndex - 1);
							break;
						}
					}
				}

				if (!isMatch)
				{
					if (this.Child != null)
					{
						this.Child.Scan(token);
					}
				}
			}
		}
	}
}