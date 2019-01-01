using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Cold Fusion syntax highlighter.
	/// </summary>
	public class CPPHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CPPHighlighter"/> class.
		/// </summary>
		public CPPHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CPPHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public CPPHighlighter(IParser parser) : base(parser)
		{
			this.Name = "CPP";
			this.FullName = "C++";
			this.TagValues.AddRange(new String[] { "cpp", "c++" });
			this.FileExtensions.Add("cpp");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new CPPHighlighter(this.Parser);
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
				"asm", "auto", "break", "case", "catch", "char", "class", "const", "continue", "default", "delete", "do", "double", "else", "enum", "except", "extern", "far", "finally", "float", "for", "friend", "goto", "huge", "if", "inline", "int", "bool", "string", "long", "fixed", "near", "new", "operator", "private", "protected", "typename", "false", "public", "register", "return", "short", "signed", "sizeof", "static", "struct", "switch", "template", "this", "throw", "try", "typedef", "union", "unsigned", "virtual", "void", "volatile", "while", "#define", "#elif", "#else", "#endif", "#error", "#ifdef", "#ifndef", "#if", "#include", "#line", "#pragma", "#undef", "#warning", "using", "namespace", "true", "s8", "u8", "s16", "u16", "s32", "u32", "s64", "u64", "s128", "u128", "f32", "and", "or", "xor", "not", "explicit", "mutable", "const_cast", "dynamic_cast", "reinterpret_cast", "static_cast"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}