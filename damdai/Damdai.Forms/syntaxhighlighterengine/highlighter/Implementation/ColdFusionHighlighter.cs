using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Cold Fusion syntax highlighter.
	/// </summary>
	public class ColdFusionHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.ColdFusionHighlighter"/> class.
		/// </summary>
		public ColdFusionHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.ColdFusionHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public ColdFusionHighlighter(IParser parser) : base(parser)
		{
			this.Name = "ColdFusion";
			this.FullName = "Cold Fusion";
			this.TagValues.AddRange(new String[] { "cfm", "coldfusion" });
			this.FileExtensions.Add("cfm");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new ColdFusionHighlighter(this.Parser);
        }

		/// <summary>
		/// Builds a word scanner.
		/// </summary>
		/// <returns>A <see cref="Damdai.Forms.WordScanner"/> object.</returns>
		protected override IScanner BuildWordScanner()
		{
			WordScanner scanner = new WordScanner(this.Tokenizer, this.ScannerResult);
			scanner.WordNodes = new WordNode[1];
			scanner.WordNodes[0] = new WordNode();
			scanner.WordNodes[0].ForeColor = Color.Blue;
			scanner.WordNodes[0].Entities.AddRange(this.GetKeywords());
			return scanner;
		}

		/// <summary>
		/// Gets an array of registered keywords.
		/// </summary>
		/// <returns>An array of keywords.</returns>
		private string[] GetKeywords()
		{
			string[] keywordList = new string[]
			{
				"URL","Form","Session","Application",
				"Request","Variables","Server","Client",
				"Cookie","CGI","File","Caller","ThisTag"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}