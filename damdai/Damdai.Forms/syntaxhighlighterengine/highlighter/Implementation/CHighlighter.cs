using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a C syntax highlighter.
	/// </summary>
	public class CHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CHighlighter"/> class.
		/// </summary>
		public CHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public CHighlighter(IParser parser) : base(parser)
		{
			this.Name = "C";
			this.FullName = "C";
			this.TagValues.AddRange(new String[] { "c" });
			this.FileExtensions.Add("c");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new CHighlighter(this.Parser);
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
				"auto","double","int","struct",
				"break","else","long","switch",
				"case","enum","register","typedef",
				"char","extern","return","union",
				"const","float","short","unsigned",
				"continue","for","signed","void",
				"default","goto","sizeof","volatile",
				"do","if","static","while",
				"abort","clock","getenv","rand","srand",
				"abs","close","labs","read","strcmp",
				"atof","div","malloc","remove","strcpy",
				"atoi","exit","open","rename","system",
				"atol","free","printf","scanf","time",
				"calloc","getchar","putchar","signal","write",
				"acos","cos","floor","sin","tanh",
				"asin","cosh","log","sinh",
				"atan","exp","log10","sqrt",
				"ceil","fabs","pow","tan"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}