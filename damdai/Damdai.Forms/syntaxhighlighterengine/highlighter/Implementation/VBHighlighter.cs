using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Visual Basic syntax highlighter.
	/// </summary>
	public class VBHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.VBHighlighter"/> class.
		/// </summary>
		public VBHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.VBHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public VBHighlighter(IParser parser) : base(parser)
		{
			this.Name = "VisualBasic";
			this.FullName = "Visual Basic";
			this.TagValues.AddRange(new String[] { "vb" });
			this.FileExtensions.Add("vb");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new VBHighlighter(this.Parser);
        }

        /// <summary>
        /// Builds a comment line scanner.
        /// </summary>
        /// <returns></returns>
        protected override IScanner BuildCommentLineScanner()
        {
            CommentLineScanner scanner = new CommentLineScanner(this.Tokenizer, this.ScannerResult);
            scanner.CommentLineNode.ForeColor = Color.Green;
            scanner.CommentLineNode.Entities.Add("'");
            return scanner;
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
			scanner.WordNodes[0].IgnoreCase = true;
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
"AddHandler", "AddressOf", "Alias", "And",
"AndAlso", "As", "Boolean", "ByRef",
"Byte", "ByVal", "Call", "Case",
"Catch", "CBool", "CByte", "CChar",
"CDate", "CDbl", "CDec", "Char",
"CInt", "Class", "CLng", "CObj",
"Const", "Continue", "CSByte", "CShort",
"CSng", "CStr", "CType", "CUInt",
"CULng", "CUShort", "Date", "Decimal",
"Declare", "Default", "Delegate", "Dim",
"DirectCast", "Do", "Double", "Each",
"Else", "ElseIf", "End", "EndIf",
"Enum", "Erase", "Error", "Event",
"Exit", "False", "Finally", "For",
"Friend", "Function", "Get", "GetType",
"Global", "GoSub", "GoTo", "Handles",
"If", "Implements", "Imports", "In",
"Inherits", "Integer", "Interface", "Is",
"IsNot", "Let", "Lib", "Like",
"Long", "Loop", "Me", "Mod",
"Module", "MustInherit", "MustOverride", "MyBase",
"MyClass", "Namespace", "Narrowing", "New",
"Next", "Not", "Nothing", "NotInheritable",
"NotOverridable", "Object", "Of", "On",
"Operator", "Option", "Optional", "Or",
"OrElse", "Overloads", "Overridable", "Overrides",
"ParamArray", "Partial", "Private", "Property",
"Protected", "Public", "RaiseEvent", "ReadOnly",
"ReDim", "REM", "RemoveHandler", "Resume",
"Return", "SByte", "Select", "Set",
"Shadows", "Shared", "Short", "Single",
"Static", "Step", "Stop", "String",
"Structure", "Sub", "SyncLock", "Then",
"Throw", "To", "True", "Try",
"TryCast", "TypeOf", "UInteger", "ULong",
"UShort", "Using", "Variant", "Wend",
"When", "While", "Widening", "With",
"WithEvents", "WriteOnly", "Xor"};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}