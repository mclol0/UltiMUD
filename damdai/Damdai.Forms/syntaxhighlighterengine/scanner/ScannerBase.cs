using System;
using System.Xml;

namespace Damdai.Forms
{
	/// <summary>
	/// Provides an abstract base class for scanners.
	/// </summary>
	public abstract class ScannerBase : IScanner
	{
		private IScanner child;
		private ScannerCollection dependencies;
		private bool enabled;
		private string id;
		private TokenizerBase tokenizer;
		private OccurrenceCollection scannerResult;

		/// <summary>
		/// Gets or sets the child scanner.
		/// </summary>
		/// <remarks>
		/// The child scanner is part of the chain of responsibility. In case a scanner did not find occurrences of the node(s) it 
		/// represents, this scanner may pass the responsibility on to its child.
		/// </remarks>
		public IScanner Child
		{
			get
			{
				return this.child;
			}
			set
			{
				if (value != this.child)
				{
					this.child = value;
				}
			}
		}

		/// <summary>
		/// Gets a collection of dependant <see cref="Damdai.Forms.IScanner"/> objects.
		/// </summary>
		public ScannerCollection Dependencies
		{
			get
			{
				return this.dependencies;
			}
		}

		/// <summary>
		/// Gets or sets whether the scanner is enabled or not.
		/// </summary>
		/// <remarks>
		/// An <see cref="Damdai.Forms.IScanner"/> implementation might want to disable dependant scanners to make sure 
		/// the syntax highlighting will remain valid.
		/// </remarks>
		public bool Enabled
		{
			get
			{
				return this.enabled;
			}
			set
			{
				if (value != this.enabled)
				{
					this.enabled = value;
				}
			}
		}

		/// <summary>
		/// Gets the identification of a scanner.
		/// </summary>
		public string ID
		{
			get
			{
				return this.id;
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.
		/// </summary>
		public TokenizerBase Tokenizer
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
		/// Gets or sets the scanner result.
		/// </summary>
		public OccurrenceCollection ScannerResult
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
		/// Initializes a new instance of a <see cref="Damdai.Forms.ScannerBase"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public ScannerBase(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			this.dependencies = new ScannerCollection();
			this.enabled = true;
			this.id = "Unnamed";
			this.tokenizer = tokenizer;
			this.scannerResult = scannerResult;
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public abstract IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult);

		/// <summary>
		/// Loads the state of the scanner.
		/// </summary>
		/// <param name="state">An <see cref="System.Object"/> that contains the state of the scanner.</param>
		public virtual void LoadState(object state)
		{
			//
		}

		/// <summary>
		/// Saves the current state of the scanner.
		/// </summary>
		/// <param name="container">The container which will contain the state.</param>
		/// <returns>An <see cref="System.Object"/> that contains the state of the scanner.</returns>
		public virtual object SaveState(object container)
		{
			XmlElement scannerElement = ((XmlDocument)container).CreateElement("scanner");
			scannerElement.SetAttribute("name", this.id);
			return scannerElement;
		}

		/// <summary>
		/// Sets the ID of the scanner.
		/// </summary>
		/// <param name="id">The ID of the scanner.</param>
		protected void SetID(string id)
		{
			this.id = id;
		}

		/// <summary>
		/// Scans a token.
		/// </summary>
		/// <remarks>
		/// An <see cref="Damdai.Forms.IScanner"/> implementation will generally have a reference to a 
		/// <see cref="Damdai.Forms.NodeCollection"/> which will be used to store results of a scan.
		/// </remarks>
		/// <param name="token">A token from the source code.</param>
		public abstract void Scan(string token);

		/// <summary>
		/// Resets the scanner.
		/// </summary>
		public virtual void Reset()
		{
		}
	}
}