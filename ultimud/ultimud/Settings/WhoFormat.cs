using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class WhoFormat : Formatter
    {
        string footer_;

        [Description("Optional footer text displayed at the bottom of the WHO list. Supports {count} for player count.")]
        public string Footer
        {
            get { return this.footer_; }
            set { this.footer_ = value; }
        }

        [Obfuscation(Exclude = false)]
        public override string[] FormatFields(object o)
        {
            User user = o as User;
            Character ch = (user != null ? user.Character : o as Character);
            if (ch == null)
                return new string[] { "", "", "", "" };

            if (this.Fields != null && this.Fields.Count > 0)
            {
                string[] result = new string[this.Fields.Count];
                for (int i = 0; i < this.Fields.Count; ++i)
                {
                    string val = this.Fields[i].Value ?? string.Empty;
                    val = val.Replace("{race}", ch.Race != null ? ch.Race.Name : "None");
                    val = val.Replace("{job}", ch.Job != null ? ch.Job.Name : "None");
                    val = String.ReplaceVars(val, ch);
                    result[i] = val;
                }
                return result;
            }

            string displayName = ch.Name ?? string.Empty;
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.Title))
                    displayName += " " + user.Title;
                if (user.Flags.Get((int)User.UserFlags.Administrator))
                    displayName += " [ADMIN]";
                if (user.IsAfk)
                    displayName += " [AFK]";
            }

            return new string[]
            {
                displayName,
                ch.Race != null ? ch.Race.Name : "None",
                ch.Job != null ? ch.Job.Name : "None",
                ch.Area != null ? ch.Area.Name : "Nowhere"
            };
        }

        [Obfuscation(Exclude = false)]
        public override string ToString(string sortColumn, object o)
        {
            StringTable table;
            if (this.Columns != null && this.Columns.Count > 0 && this.Fields != null && this.Fields.Count > 0)
            {
                table = new StringTable(this.Columns.ToArray());
                // columns set via constructor
            }
            else
            {
                table = new StringTable("Player", "Race", "Job", "Location");
            }

            int count = 0;
            if (Server.Instance != null && Server.Instance.Users != null)
            {
                foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
                {
                    if (u != null && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != null)
                    {
                        table.AddRow(this.FormatFields(u));
                        count++;
                    }
                }
            }

            if (!string.IsNullOrEmpty(sortColumn))
                table.Sort(sortColumn);
            else if (table.Columns.Count > 0)
                table.Sort(table.Columns[0].Name);

            string output = table.ToString();
            if (!string.IsNullOrEmpty(this.footer_))
                output += "\r\n" + Telnet.Colorize(this.footer_.Replace("{count}", count.ToString()));
            else
                output += "\r\nPlayers online: " + count;

            return output;
        }
    }
}
