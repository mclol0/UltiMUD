using System;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a php scanner.
	/// </summary>
	public class PHPScanner : ScannerBase
	{
		private OccurrenceCollection scannerResult;
		private XmlScanner xmlScanner;
		private bool inScriptBlock;

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.PHPScanner"/> class.
		/// </summary>
		public PHPScanner() : base(null, null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.PHPScanner"/> class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		public PHPScanner(TokenizerBase tokenizer, OccurrenceCollection scannerResult) : base(tokenizer, scannerResult)
		{
			this.scannerResult = scannerResult;
			this.xmlScanner = new XmlScanner(this.Tokenizer, this.scannerResult);
			this.xmlScanner.Match += new MatchEventHandler(this.xmlScanner_Match);
			this.SetID("PHPScanner");
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.
		/// </summary>
		/// <param name="tokenizer">The <see cref="Damdai.Forms.TokenizerBase"/> which is used to tokenize the source code.</param>
		/// <param name="scannerResult">The <see cref="Damdai.Forms.OccurrenceCollection"/> which will contain the scanner result.</param>
		/// <returns>A new instance of a <see cref="Damdai.Forms.IScanner"/> implementation class.</returns>
		public override IScanner Create(TokenizerBase tokenizer, OccurrenceCollection scannerResult)
		{
			return new PHPScanner(tokenizer, scannerResult);
		}

		/// <summary>
		/// Scans a token.
		/// </summary>
		/// <remarks>
		/// An <see cref="Damdai.Forms.IScanner"/> implementation will generally have a reference to a 
		/// <see cref="Damdai.Forms.NodeCollection"/> which will be used to store results of a scan.
		/// </remarks>
		/// <param name="token">A token from the source code.</param>
		public override void Scan(string token)
		{
			if (!this.Enabled)
			{
				if (this.Child != null)
				{
					this.Child.Scan(token);
				}
			}
			else
			{
				// Highlight as XML until a script block was found.
				if (!this.inScriptBlock)
				{
					// Find script block.
					this.xmlScanner.Scan(token);
				}
				else
				{
					if ((this.Tokenizer.Position + 2) <= this.Tokenizer.Source.Length && this.Tokenizer.GetNextTokens(2) == "?>")
					{
						this.inScriptBlock = false;
						this.xmlScanner.Scan(token);
					}
					else if (this.Child != null)
						this.Child.Scan(token);
				}
			}
		}

		/// <summary>
		/// Handles the match event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void xmlScanner_Match(object sender, MatchEventArgs e)
		{
			if (e.Type == MatchType.StartIdentifier)
			{
				this.inScriptBlock = true;
			}
			else if (e.Type == MatchType.EndIdentifier)
			{
				this.inScriptBlock = false;
			}
		}

		/// <summary>
		/// Resets the scanner.
		/// </summary>
		public override void Reset()
		{
			this.inScriptBlock = false;
		}

	}
}