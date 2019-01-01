using System;
using System.Drawing;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a string line scanner.
	/// </summary>
	public class StringLineScanner : ScannerBase
	{
		private StringNode stringNode;

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
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringLineScanner"/> class.
		/// </summary>
		public StringLineScanner() : this(null, null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringLineScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public StringLineScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : this(tokenizer, scannerResult, new StringNode())
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.StringLineScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <param name="stringNode">A <see cref="Damdai.Forms.StringNode"/> object.</param>
		public StringLineScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult, StringNode stringNode) : base(tokenizer, scannerResult)
		{
			this.ScannerResult = scannerResult;
			this.stringNode = stringNode;
			this.SetID("StringLineScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new StringLineScanner(tokenizer, scannerResult);
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

				for (int i = 0; i < this.stringNode.Entities.Count; i++)
				{
					if ((this.Tokenizer.Position + this.stringNode.Entities[i].Start.Length) <= this.Tokenizer.Source.Length)
					{
						if (this.Tokenizer.GetNextTokens(this.stringNode.Entities[i].Start.Length) == this.stringNode.Entities[i].Start)
						{
							this.Tokenizer.MoveTo(this.Tokenizer.Position + this.StringNode.Entities[i].Start.Length);
							int startIndex = this.Tokenizer.Position;

							int endOfLineIndex = this.Tokenizer.Source.IndexOf("\n", this.Tokenizer.Position);
							if (endOfLineIndex == -1)
								endOfLineIndex= this.Tokenizer.Source.Length;

							string escape = ((StringEntity)this.StringNode.Entities[i]).Escape;

							while (this.Tokenizer.Position <= endOfLineIndex)
							{
								if ((this.Tokenizer.Position + escape.Length * 2 <= this.Tokenizer.Source.Length) && this.Tokenizer.GetNextTokens(escape.Length * 2) == (escape + escape))
								{
									this.Tokenizer.MoveTo(this.Tokenizer.Position + escape.Length * 2);
								}
								else if ((this.Tokenizer.Position + escape.Length + this.stringNode.Entities[i].End.Length <= this.Tokenizer.Source.Length) &&
									this.Tokenizer.GetNextTokens(escape.Length + this.stringNode.Entities[i].End.Length) == (escape + this.stringNode.Entities[i].End))
								{
									this.Tokenizer.MoveTo(this.Tokenizer.Position + escape.Length + this.stringNode.Entities[i].End.Length);
								}
								else if ((this.Tokenizer.Position + this.stringNode.Entities[i].End.Length <= this.Tokenizer.Source.Length) &&
									this.Tokenizer.GetNextTokens(this.stringNode.Entities[i].End.Length) == this.stringNode.Entities[i].End)
								{
									this.ScannerResult.Add(new Occurrence(startIndex, this.Tokenizer.Position - startIndex, this.stringNode));
									
									this.Tokenizer.MoveTo(this.Tokenizer.Position + this.stringNode.Entities[i].End.Length);
									break;
								}
								else if (this.Tokenizer.Position == endOfLineIndex)
								{
									if (endOfLineIndex - startIndex > 0)
										this.ScannerResult.Add(new Occurrence(startIndex, endOfLineIndex - startIndex, this.stringNode));
									break;
								}
								else
								{
									this.Tokenizer.MoveTo(this.Tokenizer.Position + 1);
								}
							}
							
							/*while (this.Tokenizer.Position <= endOfLineIndex)
							{
								// e.g.: string x = "hello \\world\\\\";
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
							}*/

							isMatch = true;
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