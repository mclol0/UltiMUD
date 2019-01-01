using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an XML syntax highlighter.
	/// </summary>
	public class XMLHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.XMLHighlighter"/> class.
		/// </summary>
		public XMLHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.XMLHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public XMLHighlighter(IParser parser) : base(parser)
		{
			this.Name = "XML";
			this.FullName = "XML";
			this.TagValues.AddRange(new String[] { "xml" });
			this.FileExtensions.AddRange(new String[] { "xml", "html", "htm" });
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new XMLHighlighter(this.Parser);
        }

		/// <summary>
		/// Builds a new chain of scanners.
		/// </summary>
		/// <param name="tokenizer">The tokenizer used by the scanners.</param>
		/// <param name="scannerResult">The scanner result.</param>
		/// <returns>
		/// The scanner at the start of the chain.
		/// </returns>
		public override IScanner BuildEntryPointScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			this.Tokenizer = tokenizer;
			this.ScannerResult = scannerResult;

			XmlScanner xmlScanner = new XmlScanner(this.Tokenizer, this.ScannerResult);
			return xmlScanner;
		}
	}
}