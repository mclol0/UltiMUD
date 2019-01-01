using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Ruby syntax highlighter.
	/// </summary>
	public class RubyHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.RubyHighlighter"/> class.
		/// </summary>
		public RubyHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.RubyHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public RubyHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Ruby";
			this.FullName = "Ruby";
			this.TagValues.AddRange(new String[] { "ruby" });
			this.FileExtensions.Add("ruby");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new RubyHighlighter(this.Parser);
        }

		/// <summary>
		/// Builds a comment line scanner.
		/// </summary>
		/// <returns>A <see cref="Damdai.Forms.CommentLineScanner"/> object.</returns>
		protected override IScanner BuildCommentLineScanner()
		{
			CommentLineScanner scanner = new CommentLineScanner(this.Tokenizer, this.ScannerResult);
			scanner.CommentLineNode.ForeColor = Color.Green;
			scanner.CommentLineNode.Entities.Add("#");
			return scanner;
		}

		/// <summary>
		/// Builds a word scanner.
		/// </summary>
		/// <returns>A <see cref="Damdai.Forms.WordScanner"/> object.</returns>
		protected override IScanner BuildWordScanner()
		{
			WordScanner scanner = new WordScanner(this.Tokenizer, this.ScannerResult);
			scanner.WordNodes = new WordNode[3];
			scanner.WordNodes[0] = new WordNode();
			scanner.WordNodes[0].ForeColor = Color.Blue;
			scanner.WordNodes[0].Entities.AddRange(this.GetKeywords());
			
			scanner.WordNodes[1] = new WordNode();
			scanner.WordNodes[1].ForeColor = Color.DarkBlue;
			scanner.WordNodes[1].Entities.AddRange(this.GetKeywords2());

			scanner.WordNodes[2] = new WordNode();
			scanner.WordNodes[2].ForeColor = Color.Red;
			scanner.WordNodes[2].Entities.AddRange(this.GetKeywords3());
			scanner.WordNodes[2].NavigateUrl = "http://www.ruby-doc.org/docs/rdoc/1.9/classes/{0}.html";
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
				"__FILE__","and","def","end","in","or","self","unless",
				"__LINE__","begin","defined?","ensure","module","redo","super","until",
				"BEGIN","break","do","false","next","rescue","then","when",
				"END","case","else","for","nil","retry","true","while",
				"alias","class","elsif","if","not","return","undef","yield"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}

		/// <summary>
		/// Gets an array of registered keywords.
		/// </summary>
		/// <returns>An array of keywords.</returns>
		private string[] GetKeywords2()
		{
			string[] keywordList = new string[]
				{
					"Array", "Float", "Integer", "String", "at_exit", "autoload", "binding", "caller",
					"catch", "chomp", "chomp", "chop", "chop", "eval", "exec", "exists", "exit",
					"exit", "exp", "fail", "fork", "format", "gets", "glob", "global_variables",
					"gmtime", "goto", "gsub", "gsub", "gt", "hex", "import", "iterator", "lamda", "load", "local_variables",
					"loop", "open", "p", "print", "printf", "proc", "putc", "puts", "raise", "rand", "readline",
					"readlines", "require", "select", "sleep", "split", "sprintf", "srand", "sub", "sub", "syscall",
					"system", "test", "throw", "trace_var", "trap", "untrace_var"
				};
			
			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}

		/// <summary>
		/// Gets an array of registered keywords.
		/// </summary>
		/// <returns>An array of keywords.</returns>
		private string[] GetKeywords3()
		{
			string[] keywordList = new string[]
				{
					"ArgumentError", "Array", "Bignum", "Class", "Data", "Dir", "EOFError", "Exception", "File", "Fixnum",
					"Float", "Hash", "IO", "IOError", "IndexError", "Integer", "Interrupt", "LoadError", "LocalJumpError", "MatchingData",
					"Module", "NameError", "NilClass", "NotImplementError", "Numeric", "Object", "Proc", "Range", "Regexp",
					"RuntimeError", "SecurityError", "SignalException", "StandardError", "String", "Struct", "SyntaxError", "SystemCallError",
					"SystemExit", "SystemStackError", "ThreadError", "Time", "TypeError", "ZeroDivisionError", "fatal"
				};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}