using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a JavaScript syntax highlighter.
	/// </summary>
	public class JavaScriptHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.JavaScriptHighlighter"/> class.
		/// </summary>
		public JavaScriptHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.JavaScriptHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public JavaScriptHighlighter(IParser parser) : base(parser)
		{
			this.Name = "JavaScript";
			this.FullName = "JavaScript";
			this.TagValues.AddRange(new String[] { "js", "javascript" });
			this.FileExtensions.Add("js");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new JavaScriptHighlighter(this.Parser);
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
				"break",
				"case",
				"continue",
				"default",
				"delete",
				"do",
				"else",
				"export",
				"false",
				"for",
				"function",
				"if",
				"import",
				"in",
				"new",
				"null",
				"return",
				"switch",
				"this",
				"true",
				"typeof",
				"var",
				"void",
				"while",
				"with",
				"catch",
				"class",
				"const",
				"debugger",
				"enum",
				"extends",
				"finally",
				"super",
				"throw",
				"try",
				"abstract",
				"boolean",
				"byte",
				"char",
				"double",
				"final",
				"float",
				"goto",
				"implements",
				"instanceof",
				"int",
				"interface",
				"long",
				"native",
				"package",
				"private",
				"protected",
				"public",
				"short",
				"static",
				"synchronized",
				"throws",
				"transient",
				"alert",
				"arguments",
				"Array",
				"blur",
				"Boolean",
				"callee",
				"caller",
				"captureEvents",
				"clearInterval",
				"clearTimeout",
				"close",
				"closed",
				"confirm",
				"constructor",
				"Date",
				"defaulStatus",
				"document",
				"escape",
				"eval",
				"find",
				"focus",
				"frames",
				"Function",
				"history",
				"home",
				"Infinity",
				"innerHeight",
				"innerWidth",
				"isFinite",
				"isNaN",
				"java",
				"length",
				"location",
				"locationbar",
				"Math",
				"menubar",
				"moveBy",
				"moveTo",
				"name",
				"NaN",
				"netscape",
				"Number",
				"Ojbect",
				"open",
				"opener",
				"outerHeight",
				"outerWidth",
				"Pakcage",
				"pageXOffset",
				"pageYOffset",
				"parent",
				"parseFloat",
				"parseInt",
				"personalbar",
				"print",
				"prompt",
				"prototype",
				"RegExp",
				"releaseEvents",
				"resizeBy",
				"resizeTo",
				"routeEvent",
				"scroll",
				"scrollbars",
				"scrollBy",
				"scrollTo",
				"self",
				"setInterval",
				"setTimeout",
				"status",
				"statusbar",
				"stop",
				"String",
				"toolbar",
				"top",
				"toString",
				"unescape",
				"unwatch",
				"valueOf",
				"watch",
				"window"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}