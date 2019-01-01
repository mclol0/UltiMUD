using System;
using System.Drawing;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a string block scanner.
	/// </summary>
	public class StringBlockScanner : ScannerBase
	{
		private StringNode stringNode;
		private string lastToken;

		/// <summary>
		/// Gets or sets the string node.
		/// </summary>
		public StringNode StringNode
		{
			get
			{
				return this.stringNode;
			}
			set
			{
				if (value != this.stringNode)
				{
					this.stringNode = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringBlockScanner"/> class.
		/// </summary>
		public StringBlockScanner() : this(null, null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringBlockScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public StringBlockScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : this(tokenizer, scannerResult, new StringNode())
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringBlockScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <param name="stringNode">A <see cref="Damdai.Forms.StringNode"/> object.</param>
		public StringBlockScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult, StringNode stringNode) : base(tokenizer, scannerResult)
		{
			this.stringNode = stringNode;
			this.lastToken = null;
			this.SetID("StringBlockScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new StringBlockScanner(tokenizer, scannerResult);
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
			this.stringNode.BackColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='BackColor']").InnerText);
			this.stringNode.ForeColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='ForeColor']").InnerText);
			this.stringNode.Font = (Font)converter.ConvertFromString(element.SelectSingleNode("settings/setting[@name='Font']").InnerText);
			this.stringNode.NavigateUrl = element.SelectSingleNode("settings/setting[@name='NavigateUrl']").InnerText;

			// Load entities.
			foreach (XmlElement entityElement in element["entities"].ChildNodes)
			{
				this.stringNode.Entities.Add(new StringEntity(entityElement.Attributes["start"].Value, entityElement.Attributes["end"].Value, entityElement.Attributes["escape"].Value));
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
			settingRootElement.AppendChild(this.CreateSetting(document, "BackColor", ColorTranslator.ToHtml(this.stringNode.BackColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "ForeColor", ColorTranslator.ToHtml(this.stringNode.ForeColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "Font", converter.ConvertToString(this.stringNode.Font)));
			settingRootElement.AppendChild(this.CreateSetting(document, "NavigateUrl", this.stringNode.NavigateUrl));

			// Save entities.
			XmlElement entityRootElement = document.CreateElement("entities");
			element.AppendChild(entityRootElement);
			XmlElement entityElement;
			foreach (StringEntity entity in this.stringNode.Entities)
			{
				entityElement = document.CreateElement("entity");
				entityElement.SetAttribute("start", entity.Start);
				entityElement.SetAttribute("end", entity.End);
				entityElement.SetAttribute("escape", entity.Escape);
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

				if (this.lastToken != null && this.lastToken == "@")
				{
					for (int i = 0; i < this.stringNode.Entities.Count; i++)
					{
						if ((this.Tokenizer.Position + this.stringNode.Entities[i].Start.Length) <= this.Tokenizer.Source.Length)
						{
							if (this.Tokenizer.GetNextTokens(this.stringNode.Entities[i].Start.Length) == this.stringNode.Entities[i].Start)
							{
								this.Tokenizer.MoveTo(this.Tokenizer.Position + this.StringNode.Entities[i].Start.Length);
								int startIndex = this.Tokenizer.Position;

								int endOfLineIndex = this.Tokenizer.Source.Length;

								string escape = ((StringEntity)this.StringNode.Entities[i]).Escape;

								int index;
								while (this.Tokenizer.Position <= endOfLineIndex)
								{
									index = this.Tokenizer.Source.IndexOf(this.stringNode.Entities[i].End, this.Tokenizer.Position, endOfLineIndex - this.Tokenizer.Position);
									if (index > -1)
									{
										if (stringNode.Entities[i].End == escape && index + escape.Length < this.Tokenizer.Source.Length && this.Tokenizer.Source.Substring(index + escape.Length, escape.Length) == escape)
										{
											this.Tokenizer.MoveTo(index + escape.Length * 2);

											// Error.
											if (this.Tokenizer.Position == this.Tokenizer.Source.Length)
											{
												this.ScannerResult.Add(new Occurrence(startIndex, endOfLineIndex - startIndex, this.stringNode));
											}
										}
										else if (stringNode.Entities[i].End != escape && this.Tokenizer.Source.Substring(index - escape.Length, escape.Length) == escape)
										{
											this.Tokenizer.MoveTo(index + this.stringNode.Entities[i].End.Length);

											// Error.
											if (this.Tokenizer.Position == this.Tokenizer.Source.Length)
											{
												this.ScannerResult.Add(new Occurrence(startIndex, endOfLineIndex - startIndex, this.stringNode));
											}
										}
										else
										{
											// Success.
											this.ScannerResult.Add(new Occurrence(startIndex, index - startIndex, this.stringNode));
											this.Tokenizer.MoveTo(index + this.stringNode.Entities[i].End.Length);
											break;
										}
									}
									else
									{
										// Error.
										this.ScannerResult.Add(new Occurrence(startIndex, endOfLineIndex - startIndex, this.stringNode));
										this.Tokenizer.MoveTo(endOfLineIndex - 1);
										break;
									}
								}

								isMatch = true;
								break;
							}
						}
					}
				}

				this.lastToken = token;

				if (!isMatch)
				{
					if (this.Child != null)
					{
						this.Child.Scan(token);
					}
				}
			}
		}

		/// <summary>
		/// Resets the scanner.
		/// </summary>
		public override void Reset()
		{
			this.lastToken = null;
		}
	}
}