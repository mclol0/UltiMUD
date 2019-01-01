using System;
using System.Collections;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a register.
	/// </summary>
	public class Register
	{
		/// <summary>
		/// Gets an instance of the register as a singleton.
		/// </summary>
		public static readonly Register Instance = new Register();

		private HighlighterCollection highlighters;
		private ScannerCollection scanners;

		/// <summary>
		/// Gets the registered highlighters.
		/// </summary>
		public HighlighterCollection Highlighters
		{
			get
			{
				return this.highlighters;
			}
		}

		/// <summary>
		/// Gets the registered scanners.
		/// </summary>
		public ScannerCollection Scanners
		{
			get
			{
				return this.scanners;
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.Register"/> class.
		/// </summary>
		static Register()
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.Register"/> class.
		/// </summary>
		private Register()
		{
			this.highlighters = new HighlighterCollection();
			this.scanners = new ScannerCollection();
			this.InitializeHighlighters();
			this.InitializeScanners(new CharTokenizer());
		}

		/// <summary>
		/// Registers the highlighters.
		/// </summary>
		private void InitializeHighlighters()
		{
			this.AddHighlighter(new XMLHighlighter());
			this.AddHighlighter(new CHighlighter());
			this.AddHighlighter(new CobolHighlighter());
			this.AddHighlighter(new ColdFusionHighlighter());
			this.AddHighlighter(new CPPHighlighter());
            this.AddHighlighter(new CSSHighlighter());
			this.AddHighlighter(new CSharpHighlighter());
			this.AddHighlighter(new EiffelHighlighter());
			this.AddHighlighter(new FortranHighlighter());
			this.AddHighlighter(new HaskellHighlighter());
			this.AddHighlighter(new JavaHighlighter());
			this.AddHighlighter(new JavaScriptHighlighter());
			this.AddHighlighter(new JScriptHighlighter());
			this.AddHighlighter(new MercuryHighlighter());
			this.AddHighlighter(new MSILHighlighter());
			this.AddHighlighter(new PascalHighlighter());
			this.AddHighlighter(new PerlHighlighter());
			this.AddHighlighter(new PHPHighlighter());
			this.AddHighlighter(new PythonHighlighter());
			this.AddHighlighter(new RubyHighlighter());
			this.AddHighlighter(new SQLHighlighter());
			this.AddHighlighter(new VBHighlighter());
            this.AddHighlighter(new VBSHighlighter());
		}

		/// <summary>
		/// Registers the scanners.
		/// </summary>
		private void InitializeScanners(TokenizerBase tokenizer)
		{
			this.scanners.Add(new CommentBlockScanner());
			this.scanners.Add(new CommentLineScanner());
			this.scanners.Add(new StringBlockScanner());
			this.scanners.Add(new StringLineScanner());
			this.scanners.Add(new WordScanner());
			this.scanners.Add(new XmlScanner());
			this.scanners.Add(new PHPScanner());
		}

		/// <summary>
		/// Adds a highlighter to the register.
		/// </summary>
		/// <param name="highlighter">The highlighter to add.</param>
		private void AddHighlighter(HighlighterBase highlighter)
		{
			this.highlighters.Add(highlighter);
		}
	}
}