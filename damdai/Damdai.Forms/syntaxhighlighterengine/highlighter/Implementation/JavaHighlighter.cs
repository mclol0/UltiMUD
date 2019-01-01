using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Java syntax highlighter.
	/// </summary>
	public class JavaHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.JavaHighlighter"/> class.
		/// </summary>
		public JavaHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.JavaHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public JavaHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Java";
			this.FullName = "Java";
			this.TagValues.AddRange(new String[] { "java" });
			this.FileExtensions.Add("java");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new JavaHighlighter(this.Parser);
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
				"abstract", "boolean", "break", "byte", "case", "catch", "char", "class", "const", "continue", "default", "delegate", "do", "double", "else", "extends", "false", "final",
				"finally", "float", "for", "goto", "if", "implements", "import", "instanceof", "int", "interface", "long", "multicast", "native", "new", "null", "object", "package", "private",
				"protected", "public", "return", "short", "static", "strictfp", "super", "switch", "synchronized", "this", "throw", "throws", "transient", "true", "try", "void", "volatile", "while"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}