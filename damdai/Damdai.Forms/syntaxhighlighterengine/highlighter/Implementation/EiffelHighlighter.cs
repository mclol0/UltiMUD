using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an Eiffel syntax highlighter.
	/// </summary>
	public class EiffelHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.EiffelHighlighter"/> class.
		/// </summary>
		public EiffelHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.EiffelHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public EiffelHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Eiffel";
			this.FullName = "Eiffel";
			this.TagValues.AddRange(new String[] { "e" });
			this.FileExtensions.Add("e");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new EiffelHighlighter(this.Parser);
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
				"alias",
				"all",
				"and",
				"as",
				"check",
				"class",
				"creation",
				"debug",
				"deferred",
				"do",
				"else",
				"elseif",
				"end",
				"ensure",
				"expanded",
				"export",
				"external",
				"feature",
				"from",
				"frozen",
				"if",
				"implies",
				"indexing",
				"infix",
				"inherit",
				"inspect",
				"invariant",
				"is",
				"like",
				"local",
				"loop",
				"not",
				"obsolete",
				"old",
				"once",
				"or",
				"prefix",
				"redefine",
				"rename",
				"require",
				"rescue",
				"retry",
				"select",
				"separate",
				"then",
				"undefine",
				"until",
				"variant",
				"when",
				"xor",
				"False",
				"Strip",
				"True",
				"Unique"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}