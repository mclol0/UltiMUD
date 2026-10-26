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
    [Description("Customize the output of the EQUIPMENT command.")]
    [Obfuscation(Exclude = true)]
    public class EqFormat : Formatter
    {
        [Obfuscation(Exclude = false)]
        public override string[] FormatFields(object o)
        {
            if (o is KeyValuePair<Character.BodyParts, Item>)
            {
                KeyValuePair<Character.BodyParts, Item> pair = (KeyValuePair<Character.BodyParts, Item>)o;
                return new string[]
                {
                    "<" + pair.Key.ToString() + ">",
                    pair.Value != null ? pair.Value.Name : "nothing"
                };
            }
            return new string[] { "", "" };
        }

        [Obfuscation(Exclude = false)]
        public override string ToString(string sortColumn, object o)
        {
            User user = o as User;
            Character ch = (user != null ? user.Character : o as Character);
            if (ch == null)
                return string.Empty;

            if (ch.Equipment == null || ch.Equipment.Count == 0)
                return "You are not wearing any equipment.";

            StringTable table = new StringTable("Location", "Item");
            foreach (KeyValuePair<Character.BodyParts, Item> pair in ch.Equipment)
            {
                table.AddRow("<" + pair.Key.ToString() + ">", pair.Value != null ? pair.Value.Name : "nothing");
            }

            if (!string.IsNullOrEmpty(sortColumn))
                table.Sort(sortColumn);

            return table.ToString();
        }
    }
}
