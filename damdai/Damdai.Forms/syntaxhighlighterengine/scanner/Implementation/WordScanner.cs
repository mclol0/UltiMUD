using System;
using System.Drawing;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a word scanner.
	/// </summary>
	public class WordScanner : ScannerBase
	{
		private WordNode[] wordNodes;
		private string lastToken;

		/// <summary>
		/// Gets or sets the word nodes.
		/// </summary>
		public WordNode[] WordNodes
		{
			get
			{
				return this.wordNodes;
			}
			set
			{
				if (value != this.wordNodes)
				{
					this.wordNodes = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.WordScanner"/> class.
		/// </summary>
		public WordScanner() : this(null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.WordScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public WordScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : this(tokenizer, scannerResult, new WordNode[0])
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.WordScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <param name="wordNodes">An array of <see cref="Damdai.Forms.WordNode"/> objects.</param>
		public WordScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult, WordNode[] wordNodes) : base(tokenizer, scannerResult)
		{
			this.wordNodes = wordNodes;
			this.SetID("WordScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new WordScanner(tokenizer, scannerResult);
		}

		/// <summary>
		/// Loads the state of the scanner.
		/// </summary>
		/// <param name="state">An <see cref="System.Object"/> that contains the state of the scanner.</param>
		public override void LoadState(object state)
		{
			XmlElement element = (XmlElement)state;
			
			XmlNodeList wordGroupRootList = element["wordGroups"].ChildNodes;
			this.wordNodes = new WordNode[wordGroupRootList.Count];
			for (int i = 0; i < wordGroupRootList.Count; i++)
			{
				// Load settings.
				this.wordNodes[i] = new WordNode();
				FontConverter converter = new FontConverter();
				this.wordNodes[i].BackColor = ColorTranslator.FromHtml(wordGroupRootList[i].SelectSingleNode("settings/setting[@name='BackColor']").InnerText);
				this.wordNodes[i].ForeColor = ColorTranslator.FromHtml(wordGroupRootList[i].SelectSingleNode("settings/setting[@name='ForeColor']").InnerText);
				this.wordNodes[i].Font = (Font)converter.ConvertFromString(wordGroupRootList[i].SelectSingleNode("settings/setting[@name='Font']").InnerText);
				this.wordNodes[i].NavigateUrl = wordGroupRootList[i].SelectSingleNode("settings/setting[@name='NavigateUrl']").InnerText;
				this.wordNodes[i].IgnoreCase = Convert.ToBoolean(wordGroupRootList[i].SelectSingleNode("settings/setting[@name='IgnoreCase']").InnerText);

				// Load entities.
				foreach (XmlElement entityElement in wordGroupRootList[i]["entities"].ChildNodes)
				{
					this.wordNodes[i].Entities.Add(entityElement.InnerText);
				}
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
			
			FontConverter converter = new FontConverter();
			XmlElement wordRootElement = document.CreateElement("wordGroups");
			element.AppendChild(wordRootElement);
			XmlElement wordElement;
			XmlElement settingRootElement;
			XmlElement entityRootElement;
			XmlElement entityElement;
			foreach (WordNode node in this.wordNodes)
			{
				wordElement = document.CreateElement("wordGroup");
				wordRootElement.AppendChild(wordElement);

				// Save settings.
				settingRootElement = document.CreateElement("settings");
				wordElement.AppendChild(settingRootElement);

				settingRootElement.AppendChild(this.CreateSetting(document, "BackColor", ColorTranslator.ToHtml(node.BackColor)));
				settingRootElement.AppendChild(this.CreateSetting(document, "ForeColor", ColorTranslator.ToHtml(node.ForeColor)));
				settingRootElement.AppendChild(this.CreateSetting(document, "Font", converter.ConvertToString(node.Font)));
				settingRootElement.AppendChild(this.CreateSetting(document, "NavigateUrl", node.NavigateUrl));
				settingRootElement.AppendChild(this.CreateSetting(document, "IgnoreCase", node.IgnoreCase.ToString()));

				// Save entities.
				entityRootElement = document.CreateElement("entities");
				wordElement.AppendChild(entityRootElement);
				foreach (string entity in node.Entities)
				{
					entityElement = document.CreateElement("entity");
					entityElement.InnerText = entity;
					entityRootElement.AppendChild(entityElement);
				}
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

				if (this.lastToken == null || (this.lastToken != token && this.IsValidChar(this.lastToken[0])))
				{
					string currentWord = String.Empty;
					for (int i = this.Tokenizer.Position; i < this.Tokenizer.Source.Length; i++)
					{
						if (!this.IsValidChar(this.Tokenizer.Source[i]))
							currentWord += this.Tokenizer.Source[i];
						else
							break;
					}

					if (currentWord.Length > 0)
					{
						for (int i = 0; i < this.wordNodes.Length; i++)
						{
							for (int j = 0; j < this.wordNodes[i].Entities.Count; j++)
							{
								if (this.wordNodes[i].Entities[j].Length == currentWord.Length)
								{
									if (String.Compare(currentWord, this.wordNodes[i].Entities[j], this.wordNodes[i].IgnoreCase) == 0)
									{
										isMatch = true;
										this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, this.wordNodes[i].Entities[j].Length, this.wordNodes[i]));
										this.Tokenizer.MoveTo(this.Tokenizer.Position + this.wordNodes[i].Entities[j].Length - 1);
										break;
									}
								}
							}
							if (isMatch)
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

			this.lastToken = token;
		}

		/// <summary>
		/// Checks whether the valid is a valid char in the sense that it can be placed in front or after a word.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		private bool IsValidChar(char c)
		{
			return (!char.IsLetter(c)) && (c != '_');
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