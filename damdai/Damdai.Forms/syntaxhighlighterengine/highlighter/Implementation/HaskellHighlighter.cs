using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Haskell syntax highlighter.
	/// </summary>
	public class HaskellHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.HaskellHighlighter"/> class.
		/// </summary>
		public HaskellHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.HaskellHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public HaskellHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Haskell";
			this.FullName = "Haskell";
			this.TagValues.AddRange(new String[] { "hs" });
			this.FileExtensions.Add("hs");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new HaskellHighlighter(this.Parser);
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
				"data",
				"Bool",
				"Char",
				"Double",
				"Either",
				"FilePath",
				"Float",
				"Int",
				"Integer",
				"IO",
				"IOError",
				"Maybe",
				"Ordering",
				"ReadS",
				"ShowS",
				"String",
				"EQ",
				"False",
				"GT",
				"Just",
				"Left",
				"LT",
				"Nothing",
				"Right",
				"True",
				"Bounded",
				"Enum",
				"Eq",
				"Floating",
				"Fractional",
				"Functor",
				"Integral",
				"Monad",
				"Num",
				"Ord",
				"Read",
				"Real",
				"RealFloat",
				"RealFrac",
				"Show",
				"abs",
				"sequence",
				"acos",
				"acosh",
				"all",
				"and",
				"any",
				"appendFile",
				"applyM",
				"asTypeOf",
				"asin",
				"asinh",
				"atan",
				"atan2",
				"atanh",
				"break",
				"catch",
				"ceiling",
				"compare",
				"concat",
				"concatMap",
				"const",
				"cos",
				"cosh",
				"curry",
				"cycle",
				"decodeFloat",
				"div",
				"divMod",
				"drop",
				"dropWhile",
				"elem",
				"encodeFloat",
				"enumFrom",
				"enumFromThen",
				"enumFromThenTo",
				"enumFromTo",
				"error",
				"even",
				"exp",
				"exponent",
				"fail",
				"flip",
				"floatDigits",
				"floatRadix",
				"floatRange",
				"floor",
				"foldl",
				"foldl1",
				"foldr",
				"foldr1",
				"fromEnum",
				"fromInteger",
				"fromIntegral",
				"fromRational",
				"fst",
				"gcd",
				"getChar",
				"getContents",
				"getLine",
				"head",
				"id",
				"init",
				"interact",
				"ioError",
				"isDenormalized",
				"isIEEE",
				"isInfinite",
				"isNaN",
				"isNegativeZero",
				"iterate",
				"last",
				"lcm",
				"length",
				"lex",
				"lines",
				"log",
				"logBase",
				"lookup",
				"map",
				"mapM",
				"mapM_",
				"max",
				"maxBound",
				"maximum",
				"maybe",
				"min",
				"minBound",
				"minimum",
				"mod",
				"negate",
				"not",
				"notElem",
				"null",
				"odd",
				"or",
				"otherwise",
				"pi",
				"pred",
				"print",
				"product",
				"properFraction",
				"putChar",
				"putStr",
				"putStrLn",
				"quot",
				"quotRem",
				"read",
				"readFile",
				"readIO",
				"readList",
				"readLn",
				"readParen",
				"reads",
				"readsPrec",
				"realToFrac",
				"recip",
				"rem",
				"repeat",
				"replicate",
				"return",
				"reverse",
				"round",
				"scaleFloat",
				"scanl",
				"scanl1",
				"scanr",
				"scanr1",
				"seq",
				"sequence_",
				"show",
				"showChar",
				"showList",
				"showParen",
				"showString",
				"shows",
				"showsPrec",
				"significand",
				"signum",
				"sin",
				"sinh",
				"snd",
				"span",
				"splitAt",
				"sqrt",
				"subtract",
				"succ",
				"sum",
				"tail",
				"take",
				"takeWhile",
				"tan",
				"tanh",
				"toEnum",
				"toInteger",
				"toRational",
				"truncate",
				"uncurry",
				"undefined",
				"unlines",
				"until",
				"unwords",
				"unzip",
				"unzip3",
				"userError",
				"words",
				"writeFile",
				"zip",
				"zip3",
				"zipWith",
				"zipWith3"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}