using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an CSS syntax highlighter.
	/// </summary>
	public class CSSHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CSSHighlighter"/> class.
		/// </summary>
		public CSSHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CSSHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public CSSHighlighter(IParser parser) : base(parser)
		{
			this.Name = "CSS";
			this.FullName = "CSS";
            this.TagValues.Add("css");
            this.FileExtensions.Add("css");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new CSSHighlighter(this.Parser);
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

			CssScanner cssScanner = new CssScanner(this.Tokenizer, this.ScannerResult);
            cssScanner.Child = this.BuildCommentBlockScanner();
			return cssScanner;
		}
	}
}