using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an abstract base class for a syntax highlighter.
	/// </summary>
	public abstract class HighlighterBase : Component
	{
		private IParser parser;
		private string name;
		private string fullName;
		private StringCollection tagValues;
		private StringCollection fileExtensions;
		private TokenizerBase tokenizer;
		private ScannerCollection scanners;
		private OccurrenceCollection scannerResult;

		/// <summary>
		/// Gets or sets the unique name for this highlighter.
		/// </summary>
		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				if (value != this.name)
				{
					this.name = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the full name for this highlighter.
		/// </summary>
		public string FullName
		{
			get
			{
				return this.fullName;
			}
			set
			{
				if (value != this.fullName)
				{
					this.fullName = value;
				}
			}
		}

		/// <summary>
		/// Gets the values which can be used to specify this language.
		/// </summary>
		/// <remarks>
		/// This property can be used in different contexts such as ASP.NET context. In that case this property could 
		/// contain the possible values in order to use this highlighter.
		/// </remarks>
		public StringCollection TagValues
		{
			get
			{
				return this.tagValues;
			}
		}

		/// <summary>
		/// Gets the file extensions registered for the language this highlighter represents.
		/// </summary>
		public StringCollection FileExtensions
		{
			get
			{
				return this.fileExtensions;
			}
		}

		/// <summary>
		/// Gets or sets the parser.
		/// </summary>
		public IParser Parser
		{
			get
			{
				return this.parser;
			}
			set
			{
				if (value != this.parser)
				{
					this.parser = value;
				}
			}
		}

		/// <summary>
		/// Gets the scanners which will be used to scan the source code.
		/// </summary>
		public ScannerCollection Scanners
		{
			get
			{
				return this.scanners;
			}
		}

		/// <summary>
		/// Gets or sets the scanner result.
		/// </summary>
		protected OccurrenceCollection ScannerResult
		{
			get
			{
				return this.scannerResult;
			}
			set
			{
				if (value != this.scannerResult)
				{
					this.scannerResult = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the tokenizer.
		/// </summary>
		protected TokenizerBase Tokenizer
		{
			get
			{
				return this.tokenizer;
			}
			set
			{
				if (value != this.tokenizer)
				{
					this.tokenizer = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.HighlighterBase"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public HighlighterBase(IParser parser)
		{
			this.parser = parser;
			this.tagValues = new StringCollection();
			this.fileExtensions = new StringCollection();
			this.scanners = new ScannerCollection();

			IScanner entryPointScanner = this.BuildEntryPointScanner(new CharTokenizer(), new OccurrenceCollection());
			if (entryPointScanner != null)
				this.scanners.Add(entryPointScanner);
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public abstract HighlighterBase Create();

		/// <summary>
		/// Forces the highlighter to be reset.
		/// </summary>
		public void ForceReset()
		{
			this.scanners.Clear();

			IScanner entryPointScanner = this.BuildEntryPointScanner(new CharTokenizer(), new OccurrenceCollection());
			if (entryPointScanner != null)
				this.scanners.Add(entryPointScanner);
		}

		/// <summary>
		/// Builds a new chain of scanners.
		/// </summary>
		/// <param name="tokenizer">The tokenizer used by the scanners.</param>
		/// <param name="scannerResult">The scanner result.</param>
		/// <returns>
		/// The scanner at the start of the chain.
		/// </returns>
		public virtual IScanner BuildEntryPointScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			this.tokenizer = tokenizer;
			this.scannerResult = scannerResult;

			IScanner wordScanner = this.BuildWordScanner();

			IScanner stringScanner = this.BuildStringScanner();
			stringScanner.Child = wordScanner;

			IScanner commentLineScanner = this.BuildCommentLineScanner();
			commentLineScanner.Child = stringScanner;

			IScanner commentBlockScanner = this.BuildCommentBlockScanner();
			commentBlockScanner.Child = commentLineScanner;

			return commentBlockScanner;
		}

		/// <summary>
		/// Builds a word scanner.
		/// </summary>
		/// <returns>A <see cref="Damdai.Forms.WordScanner"/> object.</returns>
		protected virtual IScanner BuildWordScanner()
		{
			WordScanner scanner = new WordScanner(this.tokenizer, this.scannerResult);
			return scanner;
		}

		/// <summary>
		/// Builds a string scanner.
		/// </summary>
		/// <returns>A <see cref="Damdai.Forms.StringLineScanner"/> object.</returns>
		protected virtual IScanner BuildStringScanner()
		{
			StringLineScanner scanner = new StringLineScanner(this.tokenizer, this.scannerResult);
			scanner.StringNode.ForeColor = Color.DarkRed;
			scanner.StringNode.Entities.Add(new StringEntity("\"", "\"", "\\"));
			return scanner;
		}

		/// <summary>
		/// Builds a comment line scanner.
		/// </summary>
		/// <returns>A <see cref="Damdai.Forms.CommentLineScanner"/> object.</returns>
		protected virtual IScanner BuildCommentLineScanner()
		{
			CommentLineScanner scanner = new CommentLineScanner(this.tokenizer, this.scannerResult);
			scanner.CommentLineNode.ForeColor = Color.Green;
			scanner.CommentLineNode.Entities.Add("//");
			return scanner;
		}

		/// <summary>
		/// Builds a comment line scanner.
		/// </summary>
		/// <returns>A <see cref="Damdai.Forms.CommentBlockScanner"/> object.</returns>
		protected virtual IScanner BuildCommentBlockScanner()
		{
			CommentBlockScanner scanner = new CommentBlockScanner(this.tokenizer, this.scannerResult);
			scanner.CommentBlockNode.ForeColor = Color.Green;
			scanner.CommentBlockNode.Entities.Add(new Entity("/*", "*/"));
			return scanner;
		}

		/// <summary>
		/// Parses the source code.
		/// </summary>
		/// <param name="source">The source code to parse.</param>
		/// <returns>The parsed source code.</returns>
		public virtual string Parse(string source)
		{
			if (this.parser == null)
				throw new Exception("There is no parser set for this highlighter.");

			// Clean up.
			this.tokenizer.Reset();
			this.scannerResult.Clear();
			for (int i = 0; i < this.scanners.Count; i++)
				this.ResetScanner(this.scanners[i]);
			
			this.tokenizer.Tokenize(source);
			while (this.tokenizer.MoveNext())
			{
				for (int i = 0; i < this.scanners.Count; i++)
				{
					this.scanners[i].Scan(this.tokenizer.GetNextTokens(1));
				}
			}

            // Sort the collection based on the start index of the occurrence, to simplify the job of the parser.
            this.scannerResult.Sort(delegate(Occurrence lhs, Occurrence rhs)
            {
                return lhs.Start - rhs.Start;
            });

			return this.parser.Parse(source, this.scannerResult);
		}

		/// <summary>
		/// Resets a chain of scanners.
		/// </summary>
		/// <param name="scanner">The <see cref="Damdai.Forms.IScanner"/> implementation scanner which should be resetted.</param>
		private void ResetScanner(IScanner scanner)
		{
			scanner.Reset();
			if (scanner.Child != null)
				this.ResetScanner(scanner.Child);
		}
	}
}