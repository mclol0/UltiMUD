using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class StatsFormat : Formatter
    {
        public StatsFormat()
        {
        }

        [Obfuscation(Exclude = false)]
        public override string[] FormatFields(object o)
        {
            if (o is Stat)
            {
                Stat stat = (Stat)o;
                StatBlueprint bp = stat.Blueprint;
                long pct = (stat.Max > 0 ? (stat.Current * 100 / stat.Max) : 0);
                return new string[]
                {
                    bp != null ? bp.Name : "Unknown",
                    stat.Current + " / " + stat.Max,
                    pct + "%"
                };
            }

            User user = o as User;
            Character ch = (user != null ? user.Character : o as Character);
            if (ch == null)
                return new string[0];

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

            return new string[]
            {
                ch.Name ?? string.Empty,
                ch.Race != null ? ch.Race.Name : "None",
                ch.Job != null ? ch.Job.Name : "None",
                ch.Position.ToString(),
                ch.Value.ToString(Server.Instance != null ? Server.Instance.Settings.NumberFormat : "")
            };
        }

        [Obfuscation(Exclude = false)]
        public override string ToString(string sortColumn, object o)
        {
            User user = o as User;
            Character ch = (user != null ? user.Character : o as Character);
            if (ch == null)
                return string.Empty;

            string numFmt = (Server.Instance != null && Server.Instance.Settings != null) ? Server.Instance.Settings.NumberFormat : "";
            string header =
                "Name: " + ch.Name + "\r\n" +
                "Race: " + (ch.Race != null ? ch.Race.Name : "None") + "\r\n" +
                "Job:  " + (ch.Job != null ? ch.Job.Name : "None") + "\r\n" +
                "Pos:  " + ch.Position.ToString() + "\r\n" +
                "Gold: " + ch.Value.ToString(numFmt);

            if (ch.Stats == null || ch.Stats.Count == 0)
                return header;

            StringTable table = new StringTable("Stat", "Value", "%");
            table.Columns[1].Alignment = StringTable.Column.Align.Right;
            table.Columns[2].Alignment = StringTable.Column.Align.Right;

            lock (ch.Stats)
            {
                foreach (Stat stat in ch.Stats.Values)
                {
                    StatBlueprint bp = stat.Blueprint;
                    long pct = (stat.Max > 0 ? (stat.Current * 100 / stat.Max) : 0);
                    if (bp != null)
                        table.AddRow(bp.Name, stat.Current.ToString(numFmt) + "/" + stat.Max.ToString(numFmt), pct + "%");
                }
            }

            if (!string.IsNullOrEmpty(sortColumn))
                table.Sort(sortColumn);
            else
                table.Sort("Stat");

            return header + "\r\n\r\n" + table.ToString();
        }
    }
}
