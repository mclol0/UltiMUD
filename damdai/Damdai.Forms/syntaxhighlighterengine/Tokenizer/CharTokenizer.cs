using System;
using System.Text;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a character tokenizer.
	/// </summary>
	public class CharTokenizer : TokenizerBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.CharTokenizer"/> class.
		/// </summary>
		public CharTokenizer()
		{
			//
		}

		/// <summary>
		/// Tokenizes the source code.
		/// </summary>
		/// <param name="source">The source code to tokenize.</param>
		public override void Tokenize(string source)
		{
			this.Source = source;
		}

		/// <summary>
		/// Moves to the next token.
		/// </summary>
		/// <returns>Whether the iterator could be moved or not.</returns>
		public override bool MoveNext()
		{
			if ((this.Position + 1) >= this.Source.Length)
			{
				this.SetPosition(-1);
				return false;
			}
			else
			{
				this.SetPosition(this.Position + 1);
				return true;
			}
		}

		/// <summary>
		/// Gets the next token(s) without moving from position.
		/// </summary>
		/// <param name="count">The number of tokens which should be returned.</param>
		/// <returns>The tokens.</returns>
		public override string GetNextTokens(int count)
		{
			if (count <= 0)
				throw new ArgumentException("Can not be zero or less.", "count");
			if ((this.Position + count) > this.Source.Length)
				throw new ArgumentException("The sum of the current position and count is equal to or higher than the number of tokens.", "count");

			StringBuilder nextTokens = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				nextTokens.Append(this.Source[this.Position + i]);
			}
			return nextTokens.ToString();
		}
	}
}