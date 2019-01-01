using System;

namespace Damdai.Forms
{
	/// <summary>
	/// Provides a highlighter which can represent different languages at run-time.
	/// </summary>
	public class ConfiguredHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.ConfiguredHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public ConfiguredHighlighter(IParser parser) : base(parser)
		{
			//
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new ConfiguredHighlighter(this.Parser);
        }

		/// <summary>
		/// Gets the associated tokenizer.
		/// </summary>
		/// <returns>An <see cref="Damdai.Forms.TokenizerBase"/> implementation class.</returns>
		public TokenizerBase GetTokenizer()
		{
			return this.Tokenizer;
		}

		/// <summary>
		/// Gets the scanner result.
		/// </summary>
		/// <returns>An <see cref="Damdai.Forms.OccurrenceCollection"/> class.</returns>
		public OccurrenceCollection GetScannerResult()
		{
			return this.ScannerResult;
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

			// We don't know yet what the chain of scanners will be.
			return null;
		}
	}
}