using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Damdai.Forms;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Rtf parser.
	/// </summary>
	public class RtfParser : ParserBase
	{
		private RichTextBox targetControl;
		private string rtfHeader;

		/// <summary>
		/// Gets or sets the target control.
		/// </summary>
		public RichTextBox Target
		{
			get
			{
				return this.targetControl;
			}
			set
			{
				if (value != this.targetControl)
				{
					this.targetControl = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.RtfParser"/> class.
		/// </summary>
		/// <param name="targetControl">The target control which contains the to be highlighted text.</param>
		public RtfParser(RichTextBox targetControl)
		{
			this.targetControl = targetControl;
			this.rtfHeader = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fnil\\fprq1\\fcharset0 Courier New;}{\\f1\\fnil\\fcharset0 Microsoft Sans Serif;}}" +
			"{\\colortbl ;\\red255\\green0\\blue0; \\red0\\green128\\blue0; \\red0\\green0\\blue255; \\red128\\green0\\blue0; \\red199\\green21\\blue133; " + 
				"\\red155\\green0\\blue0; \\red255\\green255\\blue0; \\red125\\green125\\blue125; \\red255\\green165\\blue0; }" +
			"\\viewkind4\\uc1\\pard\\f0\\fs20 ";
		}

		/// <summary>
		/// Parses source code.
		/// </summary>
		/// <param name="source">The source code which will be parsed.</param>
		/// <param name="scannerResult">The result returned by the scanners after scanning the source code.</param>
		/// <returns>The highlighted source code.</returns>
		public override string Parse(string source, IList<Occurrence> scannerResult)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(this.rtfHeader);
			int lastIndex = 0;

			for (int i = 0; i < scannerResult.Count; i++)
			{
				if (lastIndex < (scannerResult[i].Start + scannerResult[i].Length))
				{
					builder.Append(this.ParseRtf(source.Substring(lastIndex, scannerResult[i].Start - lastIndex)));
				}

				builder.Append(this.ParseToken(this.ParseRtf(source.Substring(scannerResult[i].Start, scannerResult[i].Length)), scannerResult[i].Node));
				lastIndex = scannerResult[i].Start + scannerResult[i].Length;
			}

			if (lastIndex < source.Length)
			{
				builder.Append(this.ParseRtf(source.Substring(lastIndex, source.Length - lastIndex)));
			}

			return builder.ToString().Replace("\n", "\\par\n");
		}

		/// <summary>
		/// Parses a token.
		/// </summary>
		/// <param name="token">The token which the node matched.</param>
		/// <param name="node">The node which matched the token.</param>
		/// <returns>The highlighted token.</returns>
		protected override string ParseToken(string token, INode node)
		{
			if (node.ForeColor == Color.Red)
				return String.Format("\\cf1 {0}\\cf0 ", token);
			else if (node.ForeColor == Color.Green)
				return String.Format("\\cf2 {0}\\cf0 ", token);
			else if (node.ForeColor == Color.Blue)
				return String.Format("\\cf3 {0}\\cf0 ", token);
			else if (node.ForeColor == Color.Maroon)
				return String.Format("\\cf4 {0}\\cf0 ", token);
			else if (node.ForeColor == Color.MediumVioletRed)
				return String.Format("\\cf5 {0}\\cf0 ", token);
			else if (node.ForeColor == Color.DarkRed)
				return String.Format("\\cf6 {0}\\cf0 ", token);
            else if (node.ForeColor == Color.Gray)
                return String.Format("\\cf8 {0}\\cf0 ", token);
            else if (node.ForeColor == Color.Orange)
                return String.Format("\\cf9 {0}\\cf0 ", token);
			else if (node.BackColor == Color.Yellow)
				return String.Format("\\highlight7 {0}\\highlight0 ", token);
			return token;
		}

		/// <summary>
		/// Parses a string as a string which can be used within Rtf documents.
		/// </summary>
		/// <param name="token">The <see cref="System.String"/> which should be displayed within Rtf documents.</param>
		/// <returns>An Rtf formatted string.</returns>
		private string ParseRtf(string token)
		{
			return token.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Replace("?", "\\?");
		}
	}
}