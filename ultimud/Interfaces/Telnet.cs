using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public static class Telnet
    {
        public enum Color
        {
            Reset,
            Black,
            DarkRed,
            DarkGreen,
            DarkYellow,
            DarkBlue,
            DarkMagenta,
            DarkCyan,
            DarkWhite,
            Gray,
            Red,
            Green,
            Yellow,
            Blue,
            Magenta,
            Cyan,
            White,
            Blink,
        };

        public enum BgColor
        {
            BgReset,
            BgBlack,
            BgDarkRed,
            BgDarkGreen,
            BgDarkYellow,
            BgDarkBlue,
            BgDarkMagenta,
            BgDarkCyan,
            BgDarkWhite,
            BgRed,
            BgGreen,
            BgYellow,
            BgBlue,
            BgMagenta,
            BgCyan,
            BgWhite,
        };

        public class ColorCode
        {
            static public string Reset = "[0;0m";
            static public string Black = "[0;30m";
            static public string DarkRed = "[0;31m";
            static public string DarkGreen = "[0;32m";
            static public string DarkYellow = "[0;33m";
            static public string DarkBlue = "[0;34m";
            static public string DarkMagenta = "[0;35m";
            static public string DarkCyan = "[0;36m";
            static public string DarkWhite = "[0;37m";
            static public string Gray = "[1;30m";
            static public string Red = "[1;31m";
            static public string Green = "[1;32m";
            static public string Yellow = "[1;33m";
            static public string Blue = "[1;34m";
            static public string Magenta = "[1;35m";
            static public string Cyan = "[1;36m";
            static public string White = "[1;37m";

            //Background colors.
            static public string BgBlack = "[0;40m";
            static public string BgDarkRed = "[0;41m";
            static public string BgDarkGreen = "[0;42m";
            static public string BgDarkYellow = "[0;43m";
            static public string BgDarkBlue = "[0;44m";
            static public string BgDarkMagenta = "[0;45m";
            static public string BgDarkCyan = "[0;46m";
            static public string BgDarkWhite = "[0;47m";
            static public string BgReset = "[0;49m";
            static public string BgRed = "[1;41m";
            static public string BgGreen = "[1;42m";
            static public string BgYellow = "[1;43m";
            static public string BgBlue = "[1;44m";
            static public string BgMagenta = "[1;45m";
            static public string BgCyan = "[1;46m";
            static public string BgWhite = "[1;47m";

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
                    null, null, new IObject[] { }) as string;
            }
            catch (Exception)
            {
                Trace.WriteLine("Invalid color code: " + color, "ERROR");
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
                case Color.DarkRed: return System.Drawing.Color.DarkRed;
                case Color.DarkGreen: return System.Drawing.Color.DarkGreen;
                case Color.DarkYellow: return System.Drawing.Color.FromArgb(80, 80, 0);
                case Color.DarkBlue: return System.Drawing.Color.DarkBlue;
                case Color.DarkMagenta: return System.Drawing.Color.DarkMagenta;
                case Color.DarkCyan: return System.Drawing.Color.DarkCyan;
                case Color.DarkWhite: return System.Drawing.Color.LightGray;
                case Color.Gray: return System.Drawing.Color.Gray;
                case Color.Red: return System.Drawing.Color.Red;
                case Color.Green: return System.Drawing.Color.Green;
                case Color.Yellow: return System.Drawing.Color.Yellow;
                case Color.Blue: return System.Drawing.Color.Blue;
                case Color.Magenta: return System.Drawing.Color.Magenta;
                case Color.Cyan: return System.Drawing.Color.Cyan;
                case Color.White: return System.Drawing.Color.White;
                default: return System.Drawing.Color.White;
            }
        }
        [Obfuscation(Exclude = false)]
        public static System.Drawing.Color BgColorToSystemColor(BgColor c)
        {
            switch (c)
            {
                case BgColor.BgReset:
                case BgColor.BgBlack: return System.Drawing.Color.Black;
                case BgColor.BgDarkRed: return System.Drawing.Color.DarkRed;
                case BgColor.BgDarkGreen: return System.Drawing.Color.DarkGreen;
                case BgColor.BgDarkYellow: return System.Drawing.Color.FromArgb(80, 80, 0);
                case BgColor.BgDarkBlue: return System.Drawing.Color.DarkBlue;
                case BgColor.BgDarkMagenta: return System.Drawing.Color.DarkMagenta;
                case BgColor.BgDarkCyan: return System.Drawing.Color.DarkCyan;
                case BgColor.BgDarkWhite: return System.Drawing.Color.LightGray;
                case BgColor.BgRed: return System.Drawing.Color.Red;
                case BgColor.BgGreen: return System.Drawing.Color.Green;
                case BgColor.BgYellow: return System.Drawing.Color.Yellow;
                case BgColor.BgBlue: return System.Drawing.Color.Blue;
                case BgColor.BgMagenta: return System.Drawing.Color.Magenta;
                case BgColor.BgCyan: return System.Drawing.Color.Cyan;
                case BgColor.BgWhite: return System.Drawing.Color.White;
                default: return System.Drawing.Color.Black;
            }
        }
    }
}
