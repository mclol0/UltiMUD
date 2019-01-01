using System;
using System.Drawing;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a comment line scanner.
	/// </summary>
	public class CommentLineScanner : ScannerBase
	{
		private CommentLineNode commentLineNode;

		/// <summary>
		/// Gets or sets the comment line node.
		/// </summary>
		public CommentLineNode CommentLineNode
		{
			get
			{
				return this.commentLineNode;
			}
			set
			{
				if (value != this.commentLineNode)
				{
					this.commentLineNode = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CommentLineScanner"/> class.
		/// </summary>
		public CommentLineScanner() : this(null, null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CommentLineScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public CommentLineScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : this(tokenizer, scannerResult, new CommentLineNode())
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CommentLineScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <param name="commentLineNode">A <see cref="Damdai.Forms.CommentLineNode"/> object.</param>
		public CommentLineScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult, CommentLineNode commentLineNode) : base(tokenizer, scannerResult)
		{
			this.commentLineNode = commentLineNode;
			this.SetID("CommentLineScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new CommentLineScanner(tokenizer, scannerResult);
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
			this.commentLineNode.BackColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='BackColor']").InnerText);
			this.commentLineNode.ForeColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='ForeColor']").InnerText);
			this.commentLineNode.Font = (Font)converter.ConvertFromString(element.SelectSingleNode("settings/setting[@name='Font']").InnerText);
			this.commentLineNode.NavigateUrl = element.SelectSingleNode("settings/setting[@name='NavigateUrl']").InnerText;

			// Load entities.
			foreach (XmlElement entityElement in element["entities"].ChildNodes)
			{
				this.commentLineNode.Entities.Add(entityElement.InnerText);
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
			settingRootElement.AppendChild(this.CreateSetting(document, "BackColor", ColorTranslator.ToHtml(this.commentLineNode.BackColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "ForeColor", ColorTranslator.ToHtml(this.commentLineNode.ForeColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "Font", converter.ConvertToString(this.commentLineNode.Font)));
			settingRootElement.AppendChild(this.CreateSetting(document, "NavigateUrl", this.commentLineNode.NavigateUrl));

			// Save entities.
			XmlElement entityRootElement = document.CreateElement("entities");
			element.AppendChild(entityRootElement);
			XmlElement entityElement;
			foreach (string entity in this.commentLineNode.Entities)
			{
				entityElement = document.CreateElement("entity");
				entityElement.InnerText = entity;
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

				for (int i = 0; i < this.commentLineNode.Entities.Count; i++)
				{
					if ((this.Tokenizer.Position + this.commentLineNode.Entities[i].Length) <= this.Tokenizer.Source.Length)
					{
						if (this.Tokenizer.GetNextTokens(this.commentLineNode.Entities[i].Length) == this.commentLineNode.Entities[i])
						{
							isMatch = true;
							int endOfLineIndex = this.Tokenizer.Source.IndexOf("\n", this.Tokenizer.Position);
							if (endOfLineIndex == -1)
								endOfLineIndex = this.Tokenizer.Source.Length;
							else if (this.Tokenizer.Source[endOfLineIndex - 1] == '\r')
								endOfLineIndex--;

							this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, endOfLineIndex - this.Tokenizer.Position, this.commentLineNode));
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