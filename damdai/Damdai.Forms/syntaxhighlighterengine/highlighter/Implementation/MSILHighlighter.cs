using System;
using System.Drawing;

namespace Damdai.Forms
{
	/// <summary>
	/// Represents a MSIL syntax highlighter.
	/// </summary>
	public class MSILHighlighter : HighlighterBase
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.MSILHighlighter"/> class.
		/// </summary>
		public MSILHighlighter() : this(null)
		{
			//
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="Damdai.Forms.MSILHighlighter"/> class.
		/// </summary>
		/// <param name="parser">The parser which will be used to parse the source code.</param>
		public MSILHighlighter(IParser parser) : base(parser)
		{
			this.Name = "MSIL";
			this.FullName = "MSIL";
			this.TagValues.AddRange(new String[] { "pe" });
			this.FileExtensions.Add("pe");
		}

        /// <summary>
        /// Creates a new instance (clone) of this highlighter.
        /// </summary>
        /// <returns></returns>
        public override HighlighterBase Create()
        {
            return new MSILHighlighter(this.Parser);
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
				".#line", ".addon", ".assembly", ".cctor", ".class", ".corflags", ".ctor", ".custom", ".data", ".emitbyte", ".entrypoint", ".event", ".export", ".field",
				".file", ".fire", ".get", ".hash", ".import", ".language", ".line", ".locale", ".localized", ".locals", ".manifestres", ".maxstack", ".method", ".module",
				".mresource", ".namespace", ".os", ".other", ".override", ".pack", ".param", ".pdirect", ".permission", ".permissionset", ".processor", ".property", ".publickey",
				".publickeytoken", ".removeon", ".set", ".size", ".subsystem", ".try", ".ver", ".vtable", ".vtentry", ".vtfixup", ".zeroinit", "abstract", "add", "add.ovf", "add.ovf.un",
				"algorithm", "alignment", "and", "ansi", "any", "arglist", "array", "as", "assembly", "assert", "at", "auto", "autochar", "beforefieldinit", "beq", "beq.s", "bge", "bge.s",
				"bge.un", "bge.un.s", "bgt", "bgt.s", "bgt.un", "bgt.un.s", "ble", "ble.s", "ble.un", "ble.un.s", "blob", "blob_object", "blt", "blt.s", "blt.un", "blt.un.s", "bne.un",
				"bne.un.s", "bool", "box", "br", "br.s", "break", "brfalse", "brfalse.s", "brinst", "brinst.s", "brnull", "brnull.s", "brtrue", "brtrue.s", "brzero", "brzero.s", "bstr",
				"bytearray", "byvalstr", "call", "calli", "callmostderived", "callvirt", "carray", "castclass", "catch", "cdecl", "ceq", "cf", "cgt", "cgt.un", "char", "cil", "ckfinite",
				"class", "clsid", "clt", "clt.un", "const", "conv.i", "conv.i1", "conv.i2", "conv.i4", "conv.i8", "conv.ovf.i", "conv.ovf.i.un", "conv.ovf.i1", "conv.ovf.i1.un", "conv.ovf.i2",
				"conv.ovf.i2.un", "conv.ovf.i4", "conv.ovf.i4.un", "conv.ovf.i8", "conv.ovf.i8.un", "conv.ovf.u", "conv.ovf.u.un", "conv.ovf.u1", "conv.ovf.u1.un", "conv.ovf.u2", "conv.ovf.u2.un",
				"conv.ovf.u4", "conv.ovf.u4.un", "conv.ovf.u8", "conv.ovf.u8.un", "conv.r.un", "conv.r4", "conv.r8", "conv.u", "conv.u1", "conv.u2", "conv.u4", "conv.u8", "cpblk", "cpobj", "currency",
				"custom", "date", "decimal", "default", "default", "demand", "deny", "div", "div.un", "dup", "endfault", "endfilter", "endfinally", "endmac", "enum", "error", "explicit", "extends",
				"extern", "false", "famandassem", "family", "famorassem", "fastcall", "fastcall", "fault", "field", "filetime", "filter", "final", "finally", "fixed", "float", "float32", "float64",
				"forwardref","fromunmanaged",  "handler", "hidebysig", "hresult", "idispatch", "il", "illegal", "implements", "implicitcom", "implicitres", "import", "in", "inheritcheck", "init",
				"initblk", "initobj", "initonly", "instance", "int", "int16", "int32", "int64", "int8", "interface", "internalcall", "isinst", "iunknown", "jmp", "lasterr", "lcid", "ldarg", "ldarg.0",
				"ldarg.1", "ldarg.2", "ldarg.3", "ldarg.s", "ldarga", "ldarga.s", "ldc.i4", "ldc.i4.0", "ldc.i4.1", "ldc.i4.2", "ldc.i4.3", "ldc.i4.4", "ldc.i4.5", "ldc.i4.6", "ldc.i4.7", "ldc.i4.8",
				"ldc.i4.M1", "ldc.i4.m1", "ldc.i4.s", "ldc.i8", "ldc.r4", "ldc.r8", "ldelem.i", "ldelem.i1", "ldelem.i2", "ldelem.i4", "ldelem.i8", "ldelem.r4", "ldelem.r8", "ldelem.ref", "ldelem.u1",
				"ldelem.u2", "ldelem.u4", "ldelem.u8", "ldelema", "ldfld", "ldflda", "ldftn", "ldind.i", "ldind.i1", "ldind.i2", "ldind.i4", "ldind.i8", "ldind.r4", "ldind.r8", "ldind.ref", "ldind.u1",
				"ldind.u2", "ldind.u4", "ldind.u8", "ldlen", "ldloc", "ldloc.0", "ldloc.1", "ldloc.2", "ldloc.3", "ldloc.s", "ldloca", "ldloca.s", "ldnull", "ldobj", "ldsfld", "ldsflda", "ldstr",
				"ldtoken", "ldvirtftn", "leave", "leave.s", "linkcheck", "literal", "localloc", "lpstr", "lpstruct", "lptstr", "lpvoid", "lpwstr", "managed", "marshal", "method", "mkrefany", "modopt",
				"modreq", "mul", "mul.ovf", "mul.ovf.un", "native", "neg", "nested", "newarr", "newobj", "newslot", "noappdomain", "noinlining", "nomachine", "nomangle", "nometadata", "noncasdemand",
				"noncasinheritance", "noncaslinkdemand", "nop", "noprocess", "not", "not_in_gc_heap", "notremotable", "notserialized", "null", "nullref", "object", "objectref", "opt", "optil", "or",
				"out", "permitonly", "pinned", "pinvokeimpl", "pop", "prefix1", "prefix2", "prefix3", "prefix4", "prefix5", "prefix6", "prefix7", "prefixref", "prejitdeny", "prejitgrant", "preservesig",
				"private", "Compilercontrolled", "protected", "public", "readonly", "record", "refany", "refanytype", "refanyval", "rem", "rem.un", "reqmin", "reqopt", "reqrefuse", "reqsecobj", "request",
				"ret", "rethrow", "retval", "rtspecialname", "runtime", "safearray", "sealed", "sequential", "serializable", "shl", "shr", "shr.un", "sizeof", "special", "specialname", "starg", "starg.s",
				"static", "stdcall", "stdcall", "stelem.i", "stelem.i1", "stelem.i2", "stelem.i4", "stelem.i8", "stelem.r4", "stelem.r8", "stelem.ref", "stfld", "stind.i", "stind.i1", "stind.i2",
				"stind.i4", "stind.i8", "stind.r4", "stind.r8"
			};

			Array.Sort(keywordList);
			Array.Reverse(keywordList);

			return keywordList;
		}
	}
}