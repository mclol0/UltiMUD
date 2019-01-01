using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Visual Basic Script syntax highlighter.
	/// </summary>
	public class VBSHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.VBSHighlighter"/> class.
		/// </summary>
		public VBSHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.VBSHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public VBSHighlighter(IParser parser) : base(parser)
		{
			this.Name = "VisualBasicScript";
			this.FullName = "Visual Basic Script";
			this.TagValues.AddRange(new String[] { "vbs" });
			this.FileExtensions.Add("vbs");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new VBSHighlighter(this.Parser);
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
				"application", "asperror", "objectcontext", "request", "response", "server", "session","scripting", "createobject","<%","%>",
				"cstr","abs","array","asc","atn","cbool","cbyte","ccur","cdate","cdbl","chr","cint","write", "each", "nothing", "add", "form", "querystring", "servervariables",
				"in","datevalue","day","eval","exp","filter","fix","formatcurrency","formatdatetime","formatnumber","formatpercent","getlocale","getobject","getref","hex","hour","inputbox","instr","instrrev","int","isarray","isdate","isempty","isnull","isnumeric","isobject","join","lbound","lcase","left","len","loadpicture","log","ltrim","mid","minute","month","monthname","msgbox","now","oct","replace","rgb","right","rnd","round","rtrim","scriptengine","scriptenginebuildversion","scriptenginemajorversion","scriptengineminorversion","second","setlocale","sgn","sin","space","split","sqr","strcomp","string","strreverse","tan","time","timer","timeserial","timevalue","trim","ubound","ucase","vartype","weekday","weekdayname","year","call","class","const","dim","do","loop","erase","execute","exit","for","next","foreach","next","function","if","on","error","option","explicit","private","property","get","let","set","public","randomize","redim","rem","select","case","set","sub","while","with","with","and","eqv","imp","is","mod","not","or","xor","dictionary","drive","drives","err","file","files","filesystemobject","folder","folders","match","matches","regexp","textstream",
				"then","else","end","wend",
				"Abandon","AddHeader","AppendToLog","BinaryRead","BinaryWrite","Clear","Contents.Remove","Contents.RemoveAll","Application","Session","CreateObject","Execute","Flush","GetLastError","HTMLEncode","MapPath","Redirect","SetAbort","SetComplete","Transfer","URLEncode"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}