using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a JScript syntax highlighter.
	/// </summary>
	public class JScriptHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.JScriptHighlighter"/> class.
		/// </summary>
		public JScriptHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.JScriptHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public JScriptHighlighter(IParser parser) : base(parser)
		{
			this.Name = "JScript";
			this.FullName = "JScript";
			this.TagValues.AddRange(new String[] { "jscript" });
			this.FileExtensions.Add("jscript");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new JScriptHighlighter(this.Parser);
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
				"break",
				"delete",
				"function",
				"return",
				"typeof",
				"case",
				"do",
				"if",
				"switch",
				"var",
				"catch",
				"else",
				"in",
				"this",
				"void",
				"continue",
				"false",
				"instanceof",
				"throw",
				"while",
				"debugger",
				"finally",
				"new",
				"true",
				"with",
				"default",
				"for",
				"null",
				"try",
				"abstract",
				"double",
				"goto",
				"native",
				"static",
				"boolean",
				"enum",
				"implements",
				"package",
				"super",
				"byte",
				"export",
				"import",
				"private",
				"synchronized",
				"char",
				"extends",
				"int",
				"protected",
				"throws",
				"class",
				"final",
				"interface",
				"public",
				"transient",
				"const",
				"float",
				"long",
				"short",
				"volatile"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}