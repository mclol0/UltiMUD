using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Fortran syntax highlighter.
	/// </summary>
	public class FortranHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.FortranHighlighter"/> class.
		/// </summary>
		public FortranHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.FortranHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public FortranHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Fortran";
			this.FullName = "Fortran";
			this.TagValues.AddRange(new String[] { "for" });
			this.FileExtensions.Add("for");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new FortranHighlighter(this.Parser);
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
				"ALLOCATE",
				"ASSIGN",
				"AUTOMATIC",
				"BACKSPACE",
				"BLOCK",
				"DATA",
				"BYTE",
				"CALL",
				"CASE",
				"CHARACTER",
				"CLOSE",
				"COMMON",
				"COMPLEX",
				"CONTINUE",
				"CYCLE",
				"DATA",
				"DEALLOCATE",
				"DIMENSION",
				"DO",
				"WHILE",
				"DOUBLE",
				"COMPLEX",
				"PRECISION",
				"END",
				"DO",
				"ENDFILE",
				"ENTRY",
				"EQUIVALENCE",
				"EXIT",
				"EXTERNAL",
				"FORMAT",
				"FUNCTION",
				"GOTO",
				"IF",
				"THEN",
				"ELSE",
				"IMPLICIT",
				"INCLUDE",
				"INQUIRE",
				"INTEGER",
				"INTERFACE TO",
				"INTRINSIC",
				"LOCKING",
				"LOGICAL",
				"MAP",
				"NAMELIST",
				"OPEN",
				"PARAMETER",
				"PAUSE",
				"PRINT",
				"PROGRAM",
				"READ",
				"REAL",
				"RECORD",
				"RETURN",
				"REWIND",
				"SAVE",
				"SELECT",
				"CASE",
				"END",
				"STOP",
				"STRUCTURE",
				"END",
				"SUBROUTINE",
				"UNION",
				"WRITE"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}