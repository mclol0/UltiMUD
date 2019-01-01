using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an CSS scanner.
	/// </summary>
	public class CssScanner : ScannerBase
	{
        private CssAttributeNameNode cssSelectorNameNode;
        private CssAttributeNameNode cssAttributeNameNode;
        private CssAttributeValueNode cssAttributeValueNode;

        private int state;

		/// <summary>
		/// Initializes a new instance of an <see cref="Damdai.Forms.CssScanner"/> class.
		/// </summary>
		public CssScanner() : base(null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of an <see cref="Damdai.Forms.CssScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public CssScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : base(tokenizer, scannerResult)
		{
            this.cssSelectorNameNode = new CssAttributeNameNode();
            this.cssSelectorNameNode.ForeColor = System.Drawing.Color.Maroon;
            this.cssAttributeNameNode = new CssAttributeNameNode();
            this.cssAttributeNameNode.ForeColor = System.Drawing.Color.Red;
            this.cssAttributeValueNode = new CssAttributeValueNode();
            this.cssAttributeValueNode.ForeColor = System.Drawing.Color.Blue;
			this.SetID("CssScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new CssScanner(tokenizer, scannerResult);
		}

		/// <summary>
		/// Loads the state of the scanner.
		/// </summary>
		/// <param name="state">An <see cref="System.Object"/> that contains the state of the scanner.</param>
		public override void LoadState(object state)
		{
			XmlElement element = (XmlElement)state;
            this.LoadNode(element, this.cssSelectorNameNode, "SelectorNameNode");
            this.LoadNode(element, this.cssAttributeNameNode, "AttributeNameNode");
            this.LoadNode(element, this.cssAttributeValueNode, "AttributeValueNode");
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

            this.StoreNode(document, nodeRootElement, this.cssSelectorNameNode, "SelectorNameNode");
            this.StoreNode(document, nodeRootElement, this.cssAttributeNameNode, "AttributeNameNode");
            this.StoreNode(document, nodeRootElement, this.cssAttributeValueNode, "AttributeValueNode");

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

                char ch = token[0];

                if (!char.IsWhiteSpace(ch))
                {
                    // Match identifier(s).
                    if (this.state == 0)
                    {
                        if (ch == '{')
                        {
                            this.state = 1;
                            isMatch = true;
                        }
                        else
                        {
                            isMatch = this.MatchSelector();
                        }
                    }
                    else
                    {
                        if (ch == '}')
                        {
                            this.state = 0;
                            isMatch = true;
                        }
                        else
                        {
                            if (this.state == 1)
                            {
                                isMatch = this.MatchStyleName();
                            }
                            else if (this.state == 2)
                            {
                                isMatch = this.MatchStyleValue();
                            }
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

        private bool MatchSelector()
        {
            int start = this.Tokenizer.Position;

            char ch = this.Tokenizer.Source[this.Tokenizer.Position];
            while (char.IsLetterOrDigit(ch) || ch == '-' || ch == '#')
            {
                if (this.Tokenizer.MoveNext())
                {
                    ch = this.Tokenizer.Source[this.Tokenizer.Position];
                }
                else
                {
                    this.Tokenizer.MoveTo(this.Tokenizer.Source.Length);
                    break;
                }
            }

            if (start < this.Tokenizer.Position)
            {
                this.ScannerResult.Add(new Occurrence(start, this.Tokenizer.Position - start, this.cssSelectorNameNode));

                return true;
            }

            return false;
        }

        private bool MatchStyleName()
        {
            int start = this.Tokenizer.Position;

            char ch = this.Tokenizer.Source[this.Tokenizer.Position];
            while (char.IsLetterOrDigit(ch) || ch == '-')
            {
                this.Tokenizer.MoveNext();
                ch = this.Tokenizer.Source[this.Tokenizer.Position];
            }

            if (start < this.Tokenizer.Position)
            {
                this.ScannerResult.Add(new Occurrence(start, this.Tokenizer.Position - start, this.cssAttributeNameNode));

                this.state = 2;
                return true;
            }

            return false;
        }

        private bool MatchStyleValue()
        {
            int end = this.Tokenizer.Source.IndexOf(";", this.Tokenizer.Position);
            if (end > -1)
            {
                this.ScannerResult.Add(new Occurrence(this.Tokenizer.Position, end - this.Tokenizer.Position, this.cssAttributeValueNode));
                this.Tokenizer.MoveTo(end);
                this.state = 1;
                return true;
            }
            return false;
        }

		/// <summary>
		/// Resets the scanner.
		/// </summary>
		public override void Reset()
		{
            this.state = 0;
		}
	}
}