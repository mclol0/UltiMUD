using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an Xml scanner.
	/// </summary>
	public class XmlScanner : ScannerBase
	{
		private XmlSpecialCharNode xmlSpecialCharNode;
		private XmlNamespaceNode xmlNamespaceNode;
		private XmlTagNode xmlTagNode;
		private XmlCommentNode xmlCommentNode;
		private XmlAttributeNameNode xmlAttributeNameNode;
		private XmlAttributeValueNode xmlAttributeValueNode;
		private bool identifierEnabled;

		/// <summary>
		/// Occurs when XML was matched.
		/// </summary>
		public event MatchEventHandler Match;

		/// <summary>
		/// Gets or sets the Xml special char node.
		/// </summary>
		public XmlSpecialCharNode XmlSpecialCharNode
		{
			get
			{
				return this.xmlSpecialCharNode;
			}
			set
			{
				if (value != this.xmlSpecialCharNode)
				{
					this.xmlSpecialCharNode = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the Xml namespace node.
		/// </summary>
		public XmlNamespaceNode XmlNamespaceNode
		{
			get
			{
				return this.xmlNamespaceNode;
			}
			set
			{
				if (value != this.xmlNamespaceNode)
				{
					this.xmlNamespaceNode = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the Xml tag node.
		/// </summary>
		public XmlTagNode XmlTagNode
		{
			get
			{
				return this.xmlTagNode;
			}
			set
			{
				if (value != this.xmlTagNode)
				{
					this.xmlTagNode = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the Xml comment node.
		/// </summary>
		public XmlCommentNode XmlCommentNode
		{
			get
			{
				return this.xmlCommentNode;
			}
			set
			{
				if (value != this.xmlCommentNode)
				{
					this.xmlCommentNode = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the Xml attribute name node.
		/// </summary>
		public XmlAttributeNameNode XmlAttributeNameNode
		{
			get
			{
				return this.xmlAttributeNameNode;
			}
			set
			{
				if (value != this.xmlAttributeNameNode)
				{
					this.xmlAttributeNameNode = value;
				}
			}
		}


		/// <summary>
		/// Gets or sets the Xml attribute value node.
		/// </summary>
		public XmlAttributeValueNode XmlAttributeValueNode
		{
			get
			{
				return this.xmlAttributeValueNode;
			}
			set
			{
				if (value != this.xmlAttributeValueNode)
				{
					this.xmlAttributeValueNode = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of an <see cref="Damdai.Forms.XmlScanner"/> class.
		/// </summary>
		public XmlScanner() : base(null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of an <see cref="Damdai.Forms.XmlScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public XmlScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : base(tokenizer, scannerResult)
		{
			this.xmlSpecialCharNode = new XmlSpecialCharNode();
			this.xmlSpecialCharNode.ForeColor = System.Drawing.Color.Blue;
			this.xmlTagNode = new XmlTagNode();
			this.xmlTagNode.ForeColor = System.Drawing.Color.Maroon;
			this.xmlNamespaceNode = new XmlNamespaceNode();
			this.xmlNamespaceNode.ForeColor = System.Drawing.Color.MediumVioletRed;
			this.xmlCommentNode = new XmlCommentNode();
			this.xmlCommentNode.ForeColor = System.Drawing.Color.Green;
			this.xmlAttributeNameNode = new XmlAttributeNameNode();
			this.xmlAttributeNameNode.ForeColor = System.Drawing.Color.Red;
			this.xmlAttributeValueNode = new XmlAttributeValueNode();
			this.xmlAttributeValueNode.ForeColor = System.Drawing.Color.Blue;
			this.SetID("XmlScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new XmlScanner(tokenizer, scannerResult);
		}

		/// <summary>
		/// Loads the state of the scanner.
		/// </summary>
		/// <param name="state">An <see cref="System.Object"/> that contains the state of the scanner.</param>
		public override void LoadState(object state)
		{
			XmlElement element = (XmlElement)state;
			
			this.LoadNode(element, this.xmlSpecialCharNode, "SpecialCharNode");
			this.LoadNode(element, this.xmlTagNode, "TagNode");
			this.LoadNode(element, this.xmlNamespaceNode, "NamespaceNode");
			this.LoadNode(element, this.xmlCommentNode, "CommentNode");
			this.LoadNode(element, this.xmlAttributeNameNode, "AttributeNameNode");
			this.LoadNode(element, this.xmlAttributeValueNode, "AttributeValueNode");
		}

		/// <summary>
		/// Loads the information for a node.
		/// </summary>
		/// <param name="element">The <see cref="System.Xml.XmlElement"/> which contains the information about the node.</param>
		/// <param name="node">The <see cref="Damdai.Forms.INode"/> implementation class for which the information will be set.</param>
		/// <param name="name">The name of the node.</param>
		private void LoadNode(XmlElement element, INode node, string name)
		{
			FontConverter converter = new FontConverter();
			XmlElement nodeElement = (XmlElement)element.SelectSingleNode(String.Format("nodes/node[@name='{0}']", name));
			node.BackColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='BackColor']").InnerText);
			node.ForeColor = ColorTranslator.FromHtml(element.SelectSingleNode("settings/setting[@name='ForeColor']").InnerText);
			node.Font = (Font)converter.ConvertFromString(element.SelectSingleNode("settings/setting[@name='Font']").InnerText);
			node.NavigateUrl = element.SelectSingleNode("settings/setting[@name='NavigateUrl']").InnerText;
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
			XmlElement nodeRootElement = document.CreateElement("nodes");
			element.AppendChild(nodeRootElement);

			this.StoreNode(document, nodeRootElement, this.xmlSpecialCharNode, "SpecialCharNode");
			this.StoreNode(document, nodeRootElement, this.xmlTagNode, "TagNode");
			this.StoreNode(document, nodeRootElement, this.xmlNamespaceNode, "NamespaceNode");
			this.StoreNode(document, nodeRootElement, this.xmlCommentNode, "CommentNode");
			this.StoreNode(document, nodeRootElement, this.xmlAttributeNameNode, "AttributeNameNode");
			this.StoreNode(document, nodeRootElement, this.xmlAttributeValueNode, "AttributeValueNode");

			return element;
		}

		/// <summary>
		/// Stores a node.
		/// </summary>
		/// <param name="document">The document which will contain the settings for this node.</param>
		/// <param name="nodeRootElement">The element which will hold the node's settings.</param>
		/// <param name="node">The node which should be represented.</param>
		/// <param name="name">The name of the node.</param>
		private void StoreNode(XmlDocument document, XmlElement nodeRootElement, INode node, string name)
		{
			XmlElement nodeElement = document.CreateElement("node");
			nodeElement.SetAttribute("name", name);
			nodeRootElement.AppendChild(nodeElement);

			XmlElement settingRootElement = document.CreateElement("settings");
			nodeElement.AppendChild(settingRootElement);

			FontConverter converter = new FontConverter();
			settingRootElement.AppendChild(this.CreateSetting(document, "BackColor", ColorTranslator.ToHtml(node.BackColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "ForeColor", ColorTranslator.ToHtml(node.ForeColor)));
			settingRootElement.AppendChild(this.CreateSetting(document, "ForeColor", converter.ConvertToString(node.Font)));
			settingRootElement.AppendChild(this.CreateSetting(document, "NavigateUrl", node.NavigateUrl));
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

				// Match the closing tag, such as "</asp:Label>";
				if ((this.Tokenizer.Position + 2 <= this.Tokenizer.Source.Length) && this.Tokenizer.GetNextTokens(2) == "</")
				{
					this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, 2, this.xmlSpecialCharNode));
					
					// Match [special chars/namespace/]tag.
					int offset = 2;
					string ns = this.MatchNamespace(offset);
					if (ns.Length > 0)
						offset += ns.Length + 1;

					string tag = this.MatchTag(offset);
					if (tag.Length > 0)
						offset += tag.Length;

					// Match end character of the closing tag (">").
					int endCharIndex = this.Tokenizer.Source.IndexOfAny("<>".ToCharArray(), this.Tokenizer.Position + offset);
					if (endCharIndex > -1)
					{
						if (this.Tokenizer.Source.Substring(endCharIndex, 1).IndexOfAny("<".ToCharArray()) == -1)
						{
							this.ScannerResult.Add(new Occurrence(endCharIndex, 1, this.xmlSpecialCharNode));
							this.OnMatch(new MatchEventArgs(this.Tokenizer.Position, endCharIndex + 1, ns, tag, null, MatchType.EndTag));
						}
						this.Tokenizer.MoveTo(endCharIndex);
					}
					else
						this.Tokenizer.MoveTo(this.Tokenizer.Position + offset);
				}
				// Match comments.
				else if ((this.Tokenizer.Position + 4 <= this.Tokenizer.Source.Length) && this.Tokenizer.GetNextTokens(4) == "<!--")
				{
					int endCharIndex = this.Tokenizer.Source.IndexOf("->", this.Tokenizer.Position + 4);
					if (endCharIndex == -1)
						endCharIndex = this.Tokenizer.Source.Length;
					else
						endCharIndex += 2;
					this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, endCharIndex - this.Tokenizer.Position, this.xmlCommentNode));
					this.Tokenizer.MoveTo(endCharIndex - 1);
				}
				// Match start identifier.
				else if ((this.Tokenizer.Position + 2 <= this.Tokenizer.Source.Length) && this.Tokenizer.GetNextTokens(2) == "<?")
				{
					this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, 2, this.xmlSpecialCharNode));
					this.OnMatch(new MatchEventArgs(this.Tokenizer.Position, 2, null, null, null, MatchType.StartIdentifier));
					this.Tokenizer.MoveTo(this.Tokenizer.Position + 1);
					this.identifierEnabled = true;
				}
				// Match end identifier.
				else if ((this.Tokenizer.Position + 2 <= this.Tokenizer.Source.Length) && this.Tokenizer.GetNextTokens(2) == "?>")
				{
					this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, 2, this.xmlSpecialCharNode));
					this.OnMatch(new MatchEventArgs(this.Tokenizer.Position, 2, null, null, null, MatchType.EndIdentifier));
					this.Tokenizer.MoveTo(this.Tokenizer.Position + 1);
					this.identifierEnabled = false;
				}
				// Match attributes.
				else if (this.identifierEnabled)
				{
					// TODO: Fix this.
					/*int length = this.MatchAttributes(1, new NameValueCollection());
					if (length > -1)
						this.Tokenizer.MoveTo(this.Tokenizer.Position + length);*/
				}
				// Match either a starting tag, such as "<asp:Label>", or a single tag, such as "<asp:Label />".
				else if ((this.Tokenizer.Position + 1 <= this.Tokenizer.Source.Length) && this.Tokenizer.GetNextTokens(1) == "<")
				{
					this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, 1, this.xmlSpecialCharNode));

					// Match [special chars/namespace/]tag.
					int offset = 1;
					string ns = this.MatchNamespace(offset);
					if (ns.Length > 0)
						offset += ns.Length + 1;

					string tag = this.MatchTag(offset);
					if (tag.Length > 0)
						offset += tag.Length;

					// Match attributes.
					NameValueCollection attributes = new NameValueCollection();
					if (tag.Length > 0)
					{
						int attributesLength = this.MatchAttributes(offset, attributes);
						if (attributesLength > -1)
							offset += attributesLength;
					}

					// Match end character(s) of the start/single tag, such as ">" or "/>".
					int endCharIndex = this.Tokenizer.Source.IndexOfAny("\0<>".ToCharArray(), this.Tokenizer.Position + offset);
					if (endCharIndex > -1)
					{
						if (this.Tokenizer.Source.Substring(endCharIndex, 1).IndexOfAny("\0<".ToCharArray()) == -1)
						{
							if (this.Tokenizer.Source[endCharIndex - 1] == '/')
							{
								this.ScannerResult.Add(new Occurrence(endCharIndex - 1, 2, this.xmlSpecialCharNode));
								this.OnMatch(new MatchEventArgs(this.Tokenizer.Position, endCharIndex + 1, ns, tag, attributes, MatchType.StandaloneTag));
							}
							else
							{
								this.ScannerResult.Add(new Occurrence(endCharIndex, 1, this.xmlSpecialCharNode));
								this.OnMatch(new MatchEventArgs(this.Tokenizer.Position, endCharIndex + 1, ns, tag, attributes, MatchType.StartTag));
							}
						}
						this.Tokenizer.MoveTo(endCharIndex - 1);
					}
					else
						this.Tokenizer.MoveTo(this.Tokenizer.Position + offset);
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

		/// <summary>
		/// Matches a namespace.
		/// </summary>
		/// <param name="offset">The offset from which the search can begin.</param>
		/// <returns>The matched namespace.</returns>
		private string MatchNamespace(int offset)
		{
			if ((this.Tokenizer.Position + offset) < this.Tokenizer.Source.Length && !char.IsNumber(this.Tokenizer.Source, this.Tokenizer.Position + offset))
			{
				for (int i = this.Tokenizer.Position + offset; i < this.Tokenizer.Source.Length; i++)
				{
					if (!char.IsLetter(this.Tokenizer.Source, i))
					{
						if (this.Tokenizer.Source[i] == ':')
						{
							int length = i - (this.Tokenizer.Position + offset);
							if (length > 0)
								this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position + offset, length, this.xmlNamespaceNode));
							return this.Tokenizer.Source.Substring(this.Tokenizer.Position + offset, length);
						}
						else
							return String.Empty;
					}
				}
			}
			return String.Empty;
		}

		/// <summary>
		/// Matches a tag.
		/// </summary>
		/// <param name="offset">The offset from which the search can begin.</param>
		/// <returns>The matched tag.</returns>
		private string MatchTag(int offset)
		{
			if ((this.Tokenizer.Position + offset) < this.Tokenizer.Source.Length && !char.IsNumber(this.Tokenizer.Source, this.Tokenizer.Position + offset))
			{
				for (int i = this.Tokenizer.Position + offset; i < this.Tokenizer.Source.Length; i++)
				{
                    char ch = this.Tokenizer.Source[i];
					if (ch != '.' && !char.IsLetterOrDigit(ch))
					{
						int length = i - (this.Tokenizer.Position + offset);
						if (length > 0)
							this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position + offset, length, this.xmlTagNode));
						return this.Tokenizer.Source.Substring(this.Tokenizer.Position + offset, length);
					}
				}
				this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position + offset, this.Tokenizer.Source.Length - (this.Tokenizer.Position + offset), this.xmlTagNode));
				return this.Tokenizer.Source.Substring(this.Tokenizer.Position + offset, this.Tokenizer.Source.Length - (this.Tokenizer.Position + offset));
			}
			return String.Empty;
		}

		/// <summary>
		/// Matches an attributes.
		/// </summary>
		/// <param name="offset">The offset from which the search can begin.</param>
		/// <param name="attributes">The matched attributes.</param>
		/// <returns>The length of the matched attribute value.</returns>
		private int MatchAttributes(int offset, NameValueCollection attributes)
		{
			// NOTE: This method should be reimplemented. The regexp won't work properly when an attribute without a value is entered, e.g.: <?xml a="b"?>
			int lastCharIndex = this.Tokenizer.Source.Length;

			// Get end char of the tag (">").
			char stringType = '\0';
			for (int i = this.Tokenizer.Position + offset; i < this.Tokenizer.Source.Length; i++)
			{
				if (this.Tokenizer.Source[i] == '\"' || this.Tokenizer.Source[i] == '\'')
                {
                    if (stringType == '\0')
                        stringType = this.Tokenizer.Source[i];
                    else if (stringType == this.Tokenizer.Source[i])
                        stringType = '\0';
                }
				else if (stringType == '\0' && this.Tokenizer.Source[i] == '>')
				{
					lastCharIndex = i;
					if (this.Tokenizer.Source[i - 1] == '/')
						lastCharIndex--;
					break;
				}
			}

			MatchCollection matches = Regex.Matches(this.Tokenizer.Source.Substring(this.Tokenizer.Position + offset, lastCharIndex - (this.Tokenizer.Position + offset)),
				@"(
					(?'name'\w+) \s* (?'attGroup' = \s*
					(?'value' [^\s""'/\?>]+ | ""[^""]*"" | '[^']*')){0,1}
					\s*
				)*",
				RegexOptions.ExplicitCapture|RegexOptions.IgnorePatternWhitespace|RegexOptions.Compiled);

			int length = -1;
			for (int i = 0; i < matches.Count; i++)
			{
				for (int j = 0; j < matches[i].Groups["name"].Captures.Count; j++)
				{
					if (matches[i].Groups["name"].Captures[j].Length > 0)
					{
						this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position + offset + matches[i].Groups["name"].Captures[j].Index, matches[i].Groups["name"].Captures[j].Length, this.xmlAttributeNameNode));

						if (j < matches[i].Groups["value"].Captures.Count && matches[i].Groups["value"].Captures[j].Length > 0)
						{
							this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position + offset + matches[i].Groups["value"].Captures[j].Index, matches[i].Groups["value"].Captures[j].Length, this.xmlAttributeValueNode));
							length = matches[i].Groups["value"].Captures[j].Index + matches[i].Groups["value"].Captures[j].Length;
						}
						else
							length = matches[i].Groups["name"].Captures[j].Index + matches[i].Groups["name"].Captures[j].Length;

						if (matches[i].Groups["name"].Captures[j].Length > 0 && j < matches[i].Groups["value"].Captures.Count && matches[i].Groups["value"].Captures[j].Length > 0)
							attributes.Add(matches[i].Groups["name"].Captures[j].Value, matches[i].Groups["value"].Captures[j].Value);
					}
				}
			}

			return length;
		}

		/// <summary>
		/// Raises the <see cref="Damdai.Forms.XmlScanner.Match"/> event.
		/// </summary>
		/// <param name="e">An <see cref="System.EventArgs"/> object that contains the event data.</param>
		protected void OnMatch(MatchEventArgs e)
		{
			if (this.Match != null)
				this.Match(this, e);
		}

		/// <summary>
		/// Resets the scanner.
		/// </summary>
		public override void Reset()
		{
			this.identifierEnabled = false;
		}
	}
}