using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Mercury syntax highlighter.
	/// </summary>
	public class MercuryHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.MercuryHighlighter"/> class.
		/// </summary>
		public MercuryHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.MercuryHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public MercuryHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Mercury";
			this.FullName = "Mercury";
			this.TagValues.AddRange(new String[] { "m" });
			this.FileExtensions.Add("m");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new MercuryHighlighter(this.Parser);
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
				"div",
				"mod",
				"rem",
				"aditi_bottom_up",
				"aditi_top_down",
				"is",
				"and",
				"or",
				"func",
				"impure",
				"pred",
				"semipure",
				"not",
				"when",
				"all",
				"lambda",
				"some",
				"then",
				"if",
				"else",
				"where",
				"type",
				"end_module",
				"import_module",
				"include_module",
				"instance",
				"inst",
				"mode",
				"module",
				"pragma",
				"promise",
				"rule",
				"typeclass",
				"use_module",
				"type",
				"pred",
				"func",
				"inst",
				"mode",
				"typeclass",
				"instance",
				"pragma",
				"promise",
				"module",
				"interface",
				"implementation",
				"import_module",
				"use_module",
				"include_module",
				"end_module"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}