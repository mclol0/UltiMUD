using System;
using System.Drawing;

namespace Damdai.Forms
{
    /// <summary>
    /// Represents a Lua syntax highlighter.
    /// </summary>
    public class LuaHighlighter : HighlighterBase
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="Damdai.Forms.CSharpHighlighter"/> class.
        /// </summary>
        public LuaHighlighter()
            : this(null)
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="Damdai.Forms.CSharpHighlighter"/> class.
        /// </summary>
        /// <param name="parser">The parser which will be used to parse the source code.</param>
        public LuaHighlighter(IParser parser)
            : base(parser)
        {
            this.Name = "Lua";
            this.FullName = "Lua";
            this.TagValues.AddRange(new String[] { "Lua" });
            this.FileExtensions.Add("lua");
        }

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new LuaHighlighter(this.Parser);
        }

        /// <summary>
        /// Builds a new chain of scanners.
        /// </summary>
        /// <param name="tokenizer">The tokenizer used by the scanners.</param>
        /// <param name="scannerResult">The scanner result.</param>
        /// <returns>
        /// The scanner at the start of the chain.
        /// </returns>
        public override IScanner BuildEntryPointScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
        {
            this.Tokenizer = tokenizer;
            this.ScannerResult = scannerResult;

            IScanner wordScanner = this.BuildWordScanner();

            StringLineScanner stringLineScanner = this.BuildStringScanner() as StringLineScanner;
            stringLineScanner.StringNode.Entities.Add(new StringEntity("'", "'", "\\"));
            stringLineScanner.Child = wordScanner;

            IScanner stringBlockScanner = this.BuildStringBlockScanner();
            stringBlockScanner.Child = stringLineScanner;

            IScanner commentLineScanner = this.BuildCommentLineScanner();
            commentLineScanner.Child = stringBlockScanner;

            CommentLineScanner xmlCommentLineScanner = new CommentLineScanner(this.Tokenizer, this.ScannerResult);
            xmlCommentLineScanner.CommentLineNode.Entities.Add("--");
            xmlCommentLineScanner.CommentLineNode.ForeColor = Color.Gray;
            xmlCommentLineScanner.Child = commentLineScanner;

            IScanner commentBlockScanner = this.BuildCommentBlockScanner();
            commentBlockScanner.Child = xmlCommentLineScanner;

            return commentBlockScanner;
        }

        /// <summary>
        /// Builds a word scanner.
        /// </summary>
        /// <returns>A <see cref="Damdai.Forms.WordScanner"/> object.</returns>
        protected override IScanner BuildWordScanner()
        {
            WordScanner scanner = new WordScanner(this.Tokenizer, this.ScannerResult);
            scanner.WordNodes = new WordNode[4];
            scanner.WordNodes[0] = new WordNode();
            scanner.WordNodes[0].ForeColor = Color.Blue;
            scanner.WordNodes[0].Entities.AddRange(this.GetKeywords());
            scanner.WordNodes[1] = new WordNode();
            scanner.WordNodes[1].ForeColor = Color.Red;
            scanner.WordNodes[1].Entities.AddRange(new string[] { "luanet", "load_assembly", "import_type" });
            scanner.WordNodes[2] = new WordNode();
            scanner.WordNodes[2].ForeColor = Color.Green;
            scanner.WordNodes[2].Entities.AddRange(new string[] { "local" });
            scanner.WordNodes[3] = new WordNode();
            scanner.WordNodes[3].ForeColor = Color.Orange;
            scanner.WordNodes[3].Entities.AddRange(new string[] { "true", "false" });
            return scanner;
        }

        /// <summary>
        /// Builds a string block scanner.
        /// </summary>
        /// <returns>A <see cref="Damdai.Forms.StringBlockScanner"/> object.</returns>
        protected virtual IScanner BuildStringBlockScanner()
        {
            StringBlockScanner blockScanner = new StringBlockScanner(this.Tokenizer, this.ScannerResult);
            blockScanner.StringNode.Entities.Add(new StringEntity("\"", "\"", "\""));
            blockScanner.StringNode.ForeColor = Color.DarkRed;
            return blockScanner;
        }

        /// <summary>
        /// Gets an array of registered keywords.
        /// </summary>
        /// <returns>An array of keywords.</returns>
        private string[] GetKeywords()
        {
            string[] keywordList = new string[]
			{
				"and", "break", "continue", "do", "else", "elseif", "end",
				"for", "function", "if", "nil", "or", "return", "switch", "then", "typeof", "while"
			};

            Array.Sort(keywordList);
            Array.Reverse(keywordList);

            return keywordList;
        }
    }
}