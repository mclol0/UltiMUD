using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace Ultimud
{
    [Obfuscation(Exclude = true)]
    public class Telnet
    {
        public const byte IAC = 0xFF;
        public const byte DONT = 0xFE;
        public const byte DO = 0xFD;
        public const byte WONT = 0xFC;
        public const byte WILL = 0xFB;
        public const byte SB = 0xFA;
        public const byte SE = 0xF0;
        public const byte OPT_ECHO = 0x01;

        public static readonly byte[] WillEcho = new byte[] { IAC, WILL, OPT_ECHO };
        public static readonly byte[] WontEcho = new byte[] { IAC, WONT, OPT_ECHO };

        [Obfuscation(Exclude = false)]
        public static void SetEcho(User user, bool echo)
        {
            if (user == null || user.Socket == null || !user.Socket.Connected)
                return;
            try
            {
                user.Socket.Send(echo ? WontEcho : WillEcho);
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Obfuscation(Exclude = false)]
        public static byte[] StripTelnetNegotiation(byte[] buf, int length, out int outLength)
        {
            outLength = 0;
            if (buf == null || length <= 0)
                return new byte[0];

            byte[] clean = new byte[length];
            int i = 0;
            while (i < length)
            {
                byte b = buf[i];
                if (b == IAC)
                {
                    if (i + 1 < length)
                    {
                        byte cmd = buf[i + 1];
                        if (cmd == IAC)
                        {
                            clean[outLength++] = IAC;
                            i += 2;
                        }
                        else if (cmd == SB)
                        {
                            i += 2;
                            while (i < length)
                            {
                                if (buf[i] == IAC && i + 1 < length && buf[i + 1] == SE)
                                {
                                    i += 2;
                                    break;
                                }
                                i++;
                            }
                        }
                        else if (cmd == WILL || cmd == WONT || cmd == DO || cmd == DONT)
                        {
                            i += 3;
                        }
                        else
                        {
                            i += 2;
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    clean[outLength++] = b;
                    i++;
                }
            }
            return clean;
        }

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
            static public string Reset = "\x1B[0;0m";
            static public string Black = "\x1B[0;30m";
            static public string Red = "\x1B[0;31m";
            static public string Green = "\x1B[0;32m";
            static public string Yellow = "\x1B[0;33m";
            static public string Blue = "\x1B[0;34m";
            static public string Magenta = "\x1B[0;35m";
            static public string Cyan = "\x1B[0;36m";
            static public string White = "\x1B[0;37m";
            static public string Gray = "\x1B[1;30m";
            static public string BrightRed = "\x1B[1;31m";
            static public string BrightGreen = "\x1B[1;32m";
            static public string BrightYellow = "\x1B[1;33m";
            static public string BrightBlue = "\x1B[1;34m";
            static public string BrightMagenta = "\x1B[1;35m";
            static public string BrightCyan = "\x1B[1;36m";
            static public string BrightWhite = "\x1B[1;37m";

            //Background colors.
            static public string BgBlack = "\x1B[0;40m";
            static public string BgRed = "\x1B[0;41m";
            static public string BgGreen = "\x1B[0;42m";
            static public string BgYellow = "\x1B[0;43m";
            static public string BgBlue = "\x1B[0;44m";
            static public string BgMagenta = "\x1B[0;45m";
            static public string BgCyan = "\x1B[0;46m";
            static public string BgWhite = "\x1B[0;47m";
            static public string BgDefault = "\x1B[0;49m";
            static public string BgBrightRed = "\x1B[1;41m";
            static public string BgBrightGreen = "\x1B[1;42m";
            static public string BgBrightYellow = "\x1B[1;43m";
            static public string BgBrightBlue = "\x1B[1;44m";
            static public string BgBrightMagenta = "\x1B[1;45m";
            static public string BgBrightCyan = "\x1B[1;46m";
            static public string BgBrightWhite = "\x1B[1;47m";

            static public string Blink = "\x1B[5m";
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

            MatchCollection matches = Regex.Matches(str, @"\{(color:)(?<color>([a-zA-Z]+))\}");
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

            MatchCollection matches = Regex.Matches(str, @"((\{color:[a-zA-Z]+\})|(\x1B\[[0-9\;]+m))");
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
