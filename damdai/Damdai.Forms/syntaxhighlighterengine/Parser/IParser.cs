using System;
using System.Collections.Generic;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents an interface to enable parsers to parse source code.
	/// </summary>
	public interface IParser
	{
		/// <summary>
		/// Parses source code.
		/// </summary>
		/// <param name="source">The source code which will be parsed.</param>
		/// <param name="scannerResult">The result returned by the scanners after scanning the source code.</param>
		/// <returns>The highlighted source code.</returns>
		string Parse(string source, IList<Occurrence> scannerResult);
	}
}