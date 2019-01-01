using System;
using System.Text;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents the abstract base class for a tokenizer.
	/// </summary>
	public abstract class TokenizerBase
	{
		private int position;
		private string source;
		private string[] tokens;

		/// <summary>
		/// Gets the current position of the iterator.
		/// </summary>
		public int Position
		{
			get
			{
				return this.position;
			}
		}

		/// <summary>
		/// Gets or sets the source code which the tokenizer represents.
		/// </summary>
		public string Source
		{
			get
			{
				return this.source;
			}
			set
			{
				if (value != this.source)
				{
					this.source = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the tokens.
		/// </summary>
		protected string[] Tokens
		{
			get
			{
				return this.tokens;
			}
			set
			{
				if (value != this.tokens)
				{
					this.tokens = value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.TokenizerBase"/> class.
		/// </summary>
		public TokenizerBase()
		{
			this.position = -1;
		}

		/// <summary>
		/// Resets the tokenizer.
		/// </summary>
		public virtual void Reset()
		{
			this.position = -1;
			this.source = null;
			this.tokens = null;
		}

		/// <summary>
		/// Tokenizes the source code.
		/// </summary>
		/// <param name="source">The source code to tokenize.</param>
		public abstract void Tokenize(string source);

		/// <summary>
		/// Moves to the next token.
		/// </summary>
		/// <returns>Whether the iterator could be moved or not.</returns>
		public virtual bool MoveNext()
		{
			if ((this.position + 1) >= this.tokens.Length)
			{
				this.position = -1;
				return false;
			}
			else
			{
				this.position++;
				return true;
			}
		}

		/// <summary>
		/// Moves the iterator to the specified position.
		/// </summary>
		/// <param name="position">The new position of the iterator.</param>
		public virtual void MoveTo(int position)
		{
			this.position = position;
		}

		/// <summary>
		/// Gets the next token(s) without moving from position.
		/// </summary>
		/// <param name="count">The number of tokens which should be returned.</param>
		/// <returns>The tokens.</returns>
		public virtual string GetNextTokens(int count)
		{
			if (count <= 0)
				throw new ArgumentException("Can not be zero or less.", "count");
			if ((this.position + count) >= this.tokens.Length)
				throw new ArgumentException("The sum of the current position and count is equal to or higher than the number of tokens.", "count");

			StringBuilder nextTokens = new StringBuilder();
			for (int i = 0; i < count; i++)
			{
				nextTokens.Append(this.tokens[this.position + i]);
			}
			return nextTokens.ToString();
		}

		/// <summary>
		/// Sets the position of the iterator.
		/// </summary>
		/// <param name="position">The new position of the iterator.</param>
		protected void SetPosition(int position)
		{
			this.position = position;
		}
	}
}