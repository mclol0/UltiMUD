using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public class String
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class StringRef
        {
            string value_;

            public string Value
            {
                get { return this.value_; }
                set { this.value_ = value; }
            }

            public StringRef()
            {
            }
            public StringRef(string str)
            {
                this.Value = str;
            }

            [Obfuscation(Exclude = false)]
            public override string ToString()
            {
                return this.Value;
            }
            
            [Obfuscation(Exclude = false)]
            public static StringRef[] ToArray(params string[] strings)
            {
                List<StringRef> list = new List<StringRef>();
                foreach (string str in strings)
                {
                    list.Add(new StringRef(str));
                }
                return list.ToArray();
            }
        }

        string firstPerson_;
        string secondPerson_;
        string thirdPerson_;
        string thirdPerson2_;
        string firstPersonTargetLocation_;
        string secondPersonTargetLocation_;
        string thirdPerson3_;

        #region Properties
        [Description("The message that is displayed to the actor if they aren't in the same room as the target.")]
        public string FirstPerson
        {
            get { return this.firstPerson_; }
            set { this.firstPerson_ = value; }
        }
        
        [Description("The message that is displayed to the actor if they are in the same room as the target.")]
        public string FirstPersonAtSecondPerson
        {
            get { return this.firstPersonTargetLocation_; }
            set { this.firstPersonTargetLocation_ = value; }
        }
        
        [Description("The message that is displayed to the target if they aren't in the same room as the actor.")]
        public string SecondPerson
        {
            get { return this.secondPersonTargetLocation_; }
            set { this.secondPersonTargetLocation_ = value; }
        }
        
        [Description("The message that is displayed to the target if they are in the same room as the actor.")]
        public string SecondPersonAtFirstPerson
        {
            get { return this.secondPerson_; }
            set { this.secondPerson_ = value; }
        }
        
        [Description("The message that is displayed to everyone else if they aren't in the same room as the actor or target.")]
        public string ThirdPerson
        {
            get { return this.thirdPerson3_; }
            set { this.thirdPerson3_ = value; }
        }
        
        [Description("The message that is displayed to everyone else at the actor's location.")]
        public string ThirdPersonAtFirstPerson
        {
            get { return this.thirdPerson_; }
            set { this.thirdPerson_ = value; }
        }

        [Description("The message that is displayed to everyone else at the target's location.")]
        public string ThirdPersonAtSecondPerson
        {
            get { return this.thirdPerson2_; }
            set { this.thirdPerson2_ = value; }
        }

        [Browsable(false)]
        public bool Empty
        {
            get { return (
                string.IsNullOrEmpty(this.firstPerson_) && 
                string.IsNullOrEmpty(this.secondPerson_) && 
                string.IsNullOrEmpty(this.thirdPerson_) &&
                string.IsNullOrEmpty(this.firstPersonTargetLocation_) &&
                string.IsNullOrEmpty(this.secondPersonTargetLocation_) &&
                string.IsNullOrEmpty(this.thirdPerson2_)); }
        }
        #endregion

        public String()
        {
        }
        public String(string p1Actor, string p1Target, string p2Actor, string p2Target, string p3Actor, string p3Target, string p3)
        {
            this.firstPerson_ = p1Actor;
            this.secondPerson_ = p2Actor;
            this.thirdPerson_ = p3Actor;
            this.firstPersonTargetLocation_ = p1Target;
            this.secondPersonTargetLocation_ = p2Target;
            this.thirdPerson2_ = p3Target;
            this.thirdPerson3_ = p3;
        }
        public String(string p1Actor, string p1Target, string p2Actor, string p2Target, string p3Actor, string p3Target)
            : this(p1Actor, p1Target, p2Actor, p2Target, p3Actor, p3Target, null)
        {
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return this.FirstPerson;
        }
        
        [Obfuscation(Exclude = false)]
        public string ToFirstPerson(Telnet.Color color, IObject actor, IObject target)
        {
            return Telnet.GetColorCode(color) + String.Capitalize(this.ReplaceVariables(this.FirstPerson, actor, target, Utils.Perspective.FirstPerson));
        }
        
        [Obfuscation(Exclude = false)]
        public string ToFirstPerson(IObject actor, IObject target)
        {
            return this.ToFirstPerson(Telnet.Color.Reset, actor, target);
        }
        
        [Obfuscation(Exclude = false)]
        public string ToSecondPersonAtFirstPerson(Telnet.Color color, IObject actor, IObject target)
        {
            return Telnet.GetColorCode(color) + String.Capitalize(this.ReplaceVariables(this.SecondPersonAtFirstPerson, actor, target, Utils.Perspective.SecondPerson));
        }
        
        [Obfuscation(Exclude = false)]
        public string ToSecondPersonAtFirstPerson(IObject actor, IObject target)
        {
            return this.ToSecondPersonAtFirstPerson(Telnet.Color.Reset, actor, target);
        }
        
        [Obfuscation(Exclude = false)]
        public string ToThirdPersonAtFirstPerson(Telnet.Color color, IObject actor, IObject target)
        {
            return Telnet.GetColorCode(color) + String.Capitalize(this.ReplaceVariables(this.ThirdPersonAtFirstPerson, actor, target, Utils.Perspective.ThirdPerson));
        }
        
        [Obfuscation(Exclude = false)]
        public string ToThirdPersonAtFirstPerson(IObject actor, IObject target)
        {
            return this.ToThirdPersonAtFirstPerson(Telnet.Color.Reset, actor, target);
        }
        
        [Obfuscation(Exclude = false)]
        public string ToFirstPersonAtSecondPerson(Telnet.Color color, IObject actor, IObject target)
        {
            return Telnet.GetColorCode(color) + String.Capitalize(this.ReplaceVariables(this.FirstPersonAtSecondPerson, actor, target, Utils.Perspective.FirstPerson));
        }
        
        [Obfuscation(Exclude = false)]
        public string ToFirstPersonAtSecondPerson(IObject actor, IObject target)
        {
            return this.ToFirstPersonAtSecondPerson(Telnet.Color.Reset, actor, target);
        }
        
        [Obfuscation(Exclude = false)]
        public string ToSecondPerson(Telnet.Color color, IObject actor, IObject target)
        {
            return Telnet.GetColorCode(color) + String.Capitalize(this.ReplaceVariables(this.SecondPerson, actor, target, Utils.Perspective.SecondPerson));
        }
        
        [Obfuscation(Exclude = false)]
        public string ToSecondPerson(IObject actor, IObject target)
        {
            return this.ToSecondPerson(Telnet.Color.Reset, actor, target);
        }
        
        [Obfuscation(Exclude = false)]
        public string ToThirdPersonAtSecondPerson(Telnet.Color color, IObject actor, IObject target)
        {
            return Telnet.GetColorCode(color) + String.Capitalize(this.ReplaceVariables(this.ThirdPersonAtSecondPerson, actor, target, Utils.Perspective.ThirdPerson));
        }
        
        [Obfuscation(Exclude = false)]
        public string ToThirdPersonAtSecondPerson(IObject actor, IObject target)
        {
            return this.ToThirdPersonAtSecondPerson(Telnet.Color.Reset, actor, target);
        }
        
        [Obfuscation(Exclude = false)]
        public string ToThirdPerson(Telnet.Color color, IObject actor, IObject target)
        {
            return Telnet.GetColorCode(color) + String.Capitalize(this.ReplaceVariables(this.ThirdPerson, actor, target, Utils.Perspective.ThirdPerson));
        }
        
        [Obfuscation(Exclude = false)]
        public string ToThirdPerson(IObject actor, IObject target)
        {
            return this.ToThirdPerson(Telnet.Color.Reset, actor, target);
        }
        
        [Obfuscation(Exclude = false)]
        public String Replace(string str1, string str2)
        {
            String newstr = new String();
            if (this.FirstPerson != null)
                newstr.FirstPerson = this.FirstPerson.Replace(str1, str2);
            if (this.SecondPersonAtFirstPerson != null)
                newstr.SecondPersonAtFirstPerson = this.SecondPersonAtFirstPerson.Replace(str1, str2);
            if (this.ThirdPersonAtFirstPerson != null)
                newstr.ThirdPersonAtFirstPerson = this.ThirdPersonAtFirstPerson.Replace(str1, str2);
            if (this.FirstPersonAtSecondPerson != null)
                newstr.FirstPersonAtSecondPerson = this.FirstPersonAtSecondPerson.Replace(str1, str2);
            if (this.SecondPerson != null)
                newstr.SecondPerson = this.SecondPerson.Replace(str1, str2);
            if (this.ThirdPersonAtSecondPerson != null)
                newstr.ThirdPersonAtSecondPerson = this.ThirdPersonAtSecondPerson.Replace(str1, str2);
            if (this.ThirdPerson != null)
                newstr.ThirdPerson = this.ThirdPerson.Replace(str1, str2);
            return newstr;
        }
        
        [Obfuscation(Exclude = false)]
        string ReplaceVariables(string str, IObject p1, IObject p2, Utils.Perspective perspective)
        {
            if (str != null)
            {
                if (p1 != null)
                {
                    str = str.Replace("{1}", (p1 == p2 && perspective == Utils.Perspective.FirstPerson ? "you" : p1.Name));
                    str = this.ParseFunctions("1", str, p1);
                }
                if (p2 != null)
                {
                    Character ch = p2 as Character;
                    str = str.Replace("{2}", (p1 == p2 ? (
                        perspective == Utils.Perspective.FirstPerson ? "yourself" : (ch == null ? "it" : ch.GenderFilter("it", "him", "her")) + "self") : p2.Name));
                    str = this.ParseFunctions("2", str, p2);
                }
            }
            return str;
        }
        
        [Obfuscation(Exclude = false)]
        string ParseFunctions(string var, string str, IObject o)
        {
            MatchCollection matches = Regex.Matches(str, @"\{" + var + @"\:(?<fun>([\w]+))\((?<args>([^\)]*))\)\}");
            foreach (Match match in matches)
            {
                string fun = match.Groups["fun"].Value.ToLower();
                string args = match.Groups["args"].Value;

                if (o is Character)
                {
                    Character ch = o as Character;
                    if (fun == "sex")
                        str = str.Replace(match.Value, ch.GenderFilter(args.Split(',')).Trim());
                }
            }

            return str;
        }

        public static string PrintLine(char c, int length)
        {
            string str = string.Empty;
            for (int x = 0; x < length; ++x)
                str += c;
            return str;
        }
        public static string PrintLine(string str, int length)
        {
            string line = string.Empty;
            while (line.Length < length)
                line += str;
            return line.Substring(0, length);
        }
        public static string Format(string str)
        {
            if (Length(str) <= 76)
                return str;

            string formattedString = string.Empty;
            string[] lines = System.Text.RegularExpressions.Regex.Split(str, "\r\n|\n");
            int lineCount = 0;
            for (int x = 0; x < lines.Length; ++x)
            {
                string line = lines[x];
                if (string.IsNullOrEmpty(line))
                {
                    formattedString += (lineCount == 0 ? "\r\n" : "\r\n\r\n");
                    lineCount = 0;
                    continue;
                }
                else if (line[0] == '\t')
                {
                    formattedString += line + "\r\n";
                    lineCount = 0;
                    continue;
                }

                if (Length(line) + lineCount > 76)
                {
                    string[] words = line.Split(" ".ToCharArray());
                    foreach (string word in words)
                    {
                        lineCount += Length(word) + 1;
                        if (lineCount >= 76)
                        {
                            formattedString += "\r\n";
                            lineCount = Length(word) + 1;
                        }
                        formattedString += word + " ";
                    }
                }
                else
                {
                    formattedString += line + (x < Length(line) - 1 ? "\r\n" : "");
                    lineCount = 0;
                }
            }
            return formattedString.TrimEnd('\r', '\n');
        }
        public static int Length(string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;
            return Telnet.Decolorize(str).Length;
        }
        public static string Summarize(string str, int length)
        {
            if (str.Length > length)
                return str.Substring(0, length - 3) + "...";
            return str;
        }
        public static string Capitalize(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                char c;
                if (str.StartsWith("{color:", StringComparison.CurrentCultureIgnoreCase))
                {
                    int pos = str.IndexOf('}') + 1;
                    c = str[pos];
                    if (char.IsUpper(c))
                        return str;
                    else
                        return str.Substring(0, pos) + char.ToUpper(c) + str.Substring(pos + 1);
                }
                else
                {
                    c = str[0];
                    if (c == 27)
                    {
                        int pos = str.IndexOf('m') + 1;
                        c = str[pos];
                        if (char.IsUpper(c))
                            return str;
                        else
                            return str.Substring(0, pos) + char.ToUpper(c) + str.Substring(pos + 1);
                    }
                    else
                    {
                        if (char.IsUpper(c))
                            return str;
                        else
                            return char.ToUpper(c) + str.Substring(1);
                    }
                }
            }
            else
                return str;
        }

        [Obfuscation(Exclude = false)]
        public static string ToString<T>(List<T> list)
        {
            string str = string.Empty;
            foreach (T t in list)
                str += t + ", ";
            return str.Trim(' ', ',');
        }
        
        public static string GetArgument(ref string str)
        {
            string arg = string.Empty;
            if (str == null || str.Length == 0)
            {
                return arg;
            }
            else
            {
                if (str[0] == '"')
                {
                    int index = str.IndexOf('"', 1);
                    if (index != -1)
                    {
                        arg = str.Substring(0, index);
                        arg = arg.Replace("\"", "");
                        str = str.Remove(0, index + 1).Trim();
                        return arg;
                    }
                    else
                    {
                        arg = str;
                        str = "";
                        return arg;
                    }
                }
                else
                {
                    int index = str.IndexOf(' ');
                    if (index != -1)
                    {
                        arg = str.Substring(0, index);
                        str = str.Remove(0, index + 1).Trim();
                        return arg;
                    }
                    else
                    {
                        arg = str;
                        str = "";
                        return arg;
                    }
                }
            }
        }
        public static bool FindArgument(string str, string arg)
        {
            if (str == null || arg == null)
                return false;

            str = str.ToLower();
            arg = arg.ToLower();
            if (str.StartsWith(arg) || str.Contains(" " + arg))
                return true;
            else
                return false;
        }
        
        [Obfuscation(Exclude = false)]
        public static string HandleBackspace(string str)
        {
            str = str.TrimStart((char)8);
            int index = -1;
            while ((index = str.LastIndexOf((char)8)) > 0)
            {
                str = str.Remove(index - 1, 2);
            }
            return str;
        }
        
        [Obfuscation(Exclude = false)]
        public static string FixString(string str)
        {
            str = HandleBackspace(str);
            string newStr = string.Empty;
            foreach (char c in str)
                if ((char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c) || char.IsSymbol(c)))
                    newStr += c;
            return newStr;
        }
        
        [Obfuscation(Exclude = false)]
        public static string ReplaceVars(string str, string replacement)
        {
            MatchCollection vars = Regex.Matches(str, @"(?<var>(\{[a-zA-Z:\.]+\}))");
            foreach (Match var in vars)
                str = str.Replace(var.Value, replacement);
            return str;
        }
        
        [Obfuscation(Exclude = false)]
        public static string ReplaceChars(string str, string chars, char replacement)
        {
            foreach (char c in chars)
                str = str.Replace(c, replacement);
            return str;
        }
        
        public static string FormatNumber(int n)
        {
            return n.ToString(Server.Instance.Settings.NumberFormat);
        }
        public static string FormatNumber(long n)
        {
            return n.ToString(Server.Instance.Settings.NumberFormat);
        }
        public static string FormatNumber(uint n)
        {
            return n.ToString(Server.Instance.Settings.NumberFormat);
        }
        public static string FormatNumber(ulong n)
        {
            return n.ToString(Server.Instance.Settings.NumberFormat);
        }
    }
}
