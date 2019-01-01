using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public class Telnet
    {
        public enum Color
        {
            Reset,
            Black,
            Red,
            Green,
            Yellow,
            Blue,
            Magenta,
            Cyan,
            White,
            Gray,
            BrightRed,
            BrightGreen,
            BrightYellow,
            BrightBlue,
            BrightMagenta,
            BrightCyan,
            BrightWhite,
            Blink,
        };

        public enum BgColor
        {
            BgDefault,
            BgBlack,
            BgRed,
            BgGreen,
            BgYellow,
            BgBlue,
            BgMagenta,
            BgCyan,
            BgWhite,
            BgBrightRed,
            BgBrightGreen,
            BgBrightYellow,
            BgBrightBlue,
            BgBrightMagenta,
            BgBrightCyan,
            BgBrightWhite,
        };

        public class ColorCode
        {
            static public string Reset = "[0;0m";
            static public string Black = "[0;30m";
            static public string Red = "[0;31m";
            static public string Green = "[0;32m";
            static public string Yellow = "[0;33m";
            static public string Blue = "[0;34m";
            static public string Magenta = "[0;35m";
            static public string Cyan = "[0;36m";
            static public string White = "[0;37m";
            static public string Gray = "[1;30m";
            static public string BrightRed = "[1;31m";
            static public string BrightGreen = "[1;32m";
            static public string BrightYellow = "[1;33m";
            static public string BrightBlue = "[1;34m";
            static public string BrightMagenta = "[1;35m";
            static public string BrightCyan = "[1;36m";
            static public string BrightWhite = "[1;37m";

            //Background colors.
            static public string BgBlack = "[0;40m";
            static public string BgRed = "[0;41m";
            static public string BgGreen = "[0;42m";
            static public string BgYellow = "[0;43m";
            static public string BgBlue = "[0;44m";
            static public string BgMagenta = "[0;45m";
            static public string BgCyan = "[0;46m";
            static public string BgWhite = "[0;47m";
            static public string BgDefault = "[0;49m";
            static public string BgBrightRed = "[1;41m";
            static public string BgBrightGreen = "[1;42m";
            static public string BgBrightYellow = "[1;43m";
            static public string BgBrightBlue = "[1;44m";
            static public string BgBrightMagenta = "[1;45m";
            static public string BgBrightCyan = "[1;46m";
            static public string BgBrightWhite = "[1;47m";

            static public string Blink = "[5m";
        }

        [Obfuscation(Exclude = false)]
        static string GetColorCode(string color)
        {
            try
            {
                return typeof(Telnet.ColorCode).InvokeMember(color,
                    BindingFlags.DeclaredOnly |
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.IgnoreCase |
                    BindingFlags.GetField,
                    null, null, new object[] { }) as string;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
            return string.Empty;
        }
        [Obfuscation(Exclude = false)]
        static public string GetColorCode(Color color)
        {
            return GetColorCode(color.ToString());
        }
        [Obfuscation(Exclude = false)]
        static public string GetColorCode(BgColor color)
        {
            return GetColorCode(color.ToString());
        }
        [Obfuscation(Exclude = false)]
        static public string GetColorCode(int color)
        {
            return GetColorCode((Color)color);
        }
        [Obfuscation(Exclude = false)]
        public static string Colorize(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            MatchCollection matches = Regex.Matches(str, @"\{(color:)(?<color>([a-z]+))\}");
            foreach (Match match in matches)
            {
                str = str.Replace(match.Value, GetColorCode(match.Groups["color"].Value));
            }

            if (matches.Count > 0)
            {
                str += Telnet.ColorCode.Reset;
            }

            return str;
        }
        public static string Decolorize(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            MatchCollection matches = Regex.Matches(str, @"((\{color:[a-zA-Z]+\})|(\[[0-9\;]+m))");
            foreach (Match match in matches)
            {
                str = str.Replace(match.Value, null);
            }

            return str;
        }
        [Obfuscation(Exclude = false)]
        public static System.Drawing.Color ColorToSystemColor(Color c)
        {
            switch (c)
            {
                case Color.Reset: return System.Drawing.Color.White;
                case Color.Black: return System.Drawing.Color.FromArgb(40, 40, 40);
                case Color.Red: return System.Drawing.Color.DarkRed;
                case Color.Green: return System.Drawing.Color.DarkGreen;
                case Color.Yellow: return System.Drawing.Color.FromArgb(80, 80, 0);
                case Color.Blue: return System.Drawing.Color.DarkBlue;
                case Color.Magenta: return System.Drawing.Color.DarkMagenta;
                case Color.Cyan: return System.Drawing.Color.DarkCyan;
                case Color.White: return System.Drawing.Color.LightGray;
                case Color.Gray: return System.Drawing.Color.Gray;
                case Color.BrightRed: return System.Drawing.Color.Red;
                case Color.BrightGreen: return System.Drawing.Color.Green;
                case Color.BrightYellow: return System.Drawing.Color.Yellow;
                case Color.BrightBlue: return System.Drawing.Color.Blue;
                case Color.BrightMagenta: return System.Drawing.Color.Magenta;
                case Color.BrightCyan: return System.Drawing.Color.Cyan;
                case Color.BrightWhite: return System.Drawing.Color.White;
                default: return System.Drawing.Color.White;
            }
        }
        [Obfuscation(Exclude = false)]
        public static System.Drawing.Color BgColorToSystemColor(BgColor c)
        {
            switch (c)
            {
                case BgColor.BgDefault:
                case BgColor.BgBlack: return System.Drawing.Color.Black;
                case BgColor.BgRed: return System.Drawing.Color.DarkRed;
                case BgColor.BgGreen: return System.Drawing.Color.DarkGreen;
                case BgColor.BgYellow: return System.Drawing.Color.FromArgb(80, 80, 0);
                case BgColor.BgBlue: return System.Drawing.Color.DarkBlue;
                case BgColor.BgMagenta: return System.Drawing.Color.DarkMagenta;
                case BgColor.BgCyan: return System.Drawing.Color.DarkCyan;
                case BgColor.BgWhite: return System.Drawing.Color.LightGray;
                case BgColor.BgBrightRed: return System.Drawing.Color.Red;
                case BgColor.BgBrightGreen: return System.Drawing.Color.Green;
                case BgColor.BgBrightYellow: return System.Drawing.Color.Yellow;
                case BgColor.BgBrightBlue: return System.Drawing.Color.Blue;
                case BgColor.BgBrightMagenta: return System.Drawing.Color.Magenta;
                case BgColor.BgBrightCyan: return System.Drawing.Color.Cyan;
                case BgColor.BgBrightWhite: return System.Drawing.Color.White;
                default: return System.Drawing.Color.Black;
            }
        }
    }
}
