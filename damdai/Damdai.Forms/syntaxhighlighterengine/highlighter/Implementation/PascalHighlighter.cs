using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Pascal syntax highlighter.
	/// </summary>
	public class PascalHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.PascalHighlighter"/> class.
		/// </summary>
		public PascalHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.PascalHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public PascalHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Pascal";
			this.FullName = "Pascal";
			this.TagValues.AddRange(new String[] { "pas" });
			this.FileExtensions.Add("pas");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new PascalHighlighter(this.Parser);
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
				"and",
				"goto",
				"program",
				"array",
				"if",
				"record",
				"asm",
				"implementation",
				"repeat",
				"begin",
				"in",
				"set",
				"case",
				"inherited",
				"shl",
				"const",
				"inline",
				"shr",
				"constructor",
				"interface",
				"string",
				"declare",
				"label",
				"then",
				"destructor",
				"library",
				"to",
				"div",
				"mod",
				"type",
				"do",
				"nil",
				"unit",
				"downto",
				"not",
				"until",
				"else",
				"object",
				"uses",
				"end",
				"of",
				"var",
				"exports",
				"or",
				"virtual",
				"file",
				"overload",
				"while",
				"for",
				"packed",
				"with",
				"function",
				"procedure",
				"xor",
				"Absolute",
				"Declare",
				"os2call",
				"Assembler",
				"Export",
				"name",
				"Cdecl",
				"external",
				"virtual",
				"Code",
				"Forward",
				"stdcall",
				"Conv",
				"Index",
				"div",
				"mod",
				"and",
				"in",
				"not",
				"or",
				"shl",
				"shr",
				"xor"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}