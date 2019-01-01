using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a Perl syntax highlighter.
	/// </summary>
	public class PerlHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.PerlHighlighter"/> class.
		/// </summary>
		public PerlHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.PerlHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public PerlHighlighter(IParser parser) : base(parser)
		{
			this.Name = "Perl";
			this.FullName = "Perl";
			this.TagValues.AddRange(new String[] { "pl" });
			this.FileExtensions.Add("pl");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new PerlHighlighter(this.Parser);
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
				"if",
				"else",
				"abs",
				"accept",
				"alarm",
				"atan2",
				"bind",
				"binmode",
				"bless",
				"caller",
				"chdir",
				"chmod",
				"chomp",
				"chop",
				"chown",
				"chr",
				"chroot",
				"close",
				"closedir",
				"connect",
				"continue",
				"cos",
				"crypt",
				"dbmclose",
				"dbmopen",
				"defined",
				"delete",
				"die",
				"do",
				"dump",
				"each",
				"eof",
				"eval",
				"exec",
				"exists",
				"exit",
				"exp",
				"fcntl",
				"fileno",
				"flock",
				"fork",
				"format",
				"formline",
				"getc",
				"getlogin",
				"getpeername",
				"getpgrp",
				"getppid",
				"getpriority",
				"getpwnam",
				"getgrnam",
				"gethostbyname",
				"getnetbyname",
				"getprotobyname",
				"getpwuid",
				"getgrgid",
				"getservbyname",
				"gethostbyaddr",
				"getnetbyaddr",
				"getprotobynumber",
				"getservbyport",
				"getpwent",
				"getgrent",
				"gethostent",
				"getnetent",
				"getprotoent",
				"getservent",
				"setpwent",
				"setgrent",
				"sethostent",
				"setnetent",
				"setprotoent",
				"setservent",
				"endpwent",
				"endgrent",
				"endhostent",
				"endnetent",
				"endprotoent",
				"endservent",
				"getsockname",
				"getsockopt",
				"glob",
				"gmtime",
				"goto",
				"grep",
				"grep",
				"hex",
				"hex",
				"import",
				"index",
				"int",
				"ioctl",
				"join",
				"keys",
				"kill",
				"last",
				"lc",
				"lcfirst",
				"length",
				"link",
				"listen",
				"local",
				"localtime",
				"lock",
				"log",
				"log",
				"lstat",
				"lstat",
				"lstat",
				"map",
				"mkdir",
				"msgctl",
				"msgget",
				"msgrcv",
				"msgsnd",
				"my",
				"next",
				"next",
				"no",
				"Module",
				"oct",
				"open",
				"opendir",
				"ord",
				"our",
				"pack",
				"package",
				"pipe",
				"pop",
				"pos",
				"print",
				"printf",
				"prototype",
				"push",
				"quotemeta",
				"quotemeta",
				"rand",
				"read",
				"readdir",
				"readline",
				"readlink",
				"readpipe",
				"recv",
				"redo",
				"ref",
				"rename",
				"require",
				"reset",
				"return",
				"reverse",
				"rewinddir",
				"rindex",
				"rmdir",
				"scalar",
				"seek",
				"seekdir",
				"select",
				"select",
				"semctl",
				"semget",
				"semop",
				"send",
				"setpgrp",
				"setpriority",
				"setsockopt",
				"shift",
				"shift",
				"shmctl",
				"shmget",
				"shmread",
				"shmwrite",
				"shutdown",
				"sin",
				"sleep",
				"socket",
				"socketpair",
				"sort",
				"splice",
				"split",
				"sqrt",
				"sqrt",
				"srand",
				"srand",
				"stat",
				"study",
				"sub",
				"substr",
				"symlink",
				"syscall",
				"sysopen",
				"sysread",
				"sysseek",
				"system",
				"syswrite",
				"tell",
				"telldir",
				"tie",
				"tied",
				"time",
				"times",
				"truncate",
				"uc",
				"ucfirst",
				"umask",
				"undef",
				"unlink",
				"unpack",
				"untie",
				"unshift",
				"use",
				"Module",
				"utime",
				"values",
				"vec",
				"wait",
				"waitpid",
				"wantarray",
				"warn",
				"write",
				"TIESCALAR",
				"LIST",
				"FETCH",
				"STORE",
				"UNTIE",
				"DESTROY",
				"TIEARRAY",
				"LIST",
				"FETCH",
				"STORE",
				"FETCHSIZE",
				"STORESIZE",
				"EXTEND",
				"EXISTS",
				"DELETE",
				"CLEAR",
				"PUSH",
				"LIST",
				"POP",
				"SHIFT",
				"UNSHIFT",
				"LIST",
				"SPLICE",
				"offset",
				"length",
				"LIST",
				"UNTIE",
				"DESTROY",
				"USER",
				"HOME",
				"CLOBBER",
				"LIST",
				"TIEHASH",
				"LIST",
				"FETCH",
				"key",
				"STORE",
				"key",
				"value",
				"DELETE",
				"key",
				"CLEAR",
				"EXISTS",
				"key",
				"FIRSTKEY",
				"NEXTKEY",
				"lastkey",
				"UNTIE",
				"DESTROY",
				"TIEHANDLE",
				"LIST",
				"WRITE",
				"LIST",
				"PRINT",
				"LIST",
				"PRINTF",
				"LIST",
				"READ",
				"LIST",
				"READLINE",
				"GETC",
				"CLOSE",
				"UNTIE",
				"DESTROY"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}