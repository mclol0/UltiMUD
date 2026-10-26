using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class AbilitiesFormat : Formatter
    {
        [Obfuscation(Exclude = false)]
        public override string[] FormatFields(object o)
        {
            if (o is KeyValuePair<Guid, int>)
            {
                KeyValuePair<Guid, int> pair = (KeyValuePair<Guid, int>)o;
                Ability ability = Server.Instance != null ? Server.Instance.Get<Ability>(pair.Key) : null;
                if (ability != null)
                    return new string[] { ability.Name, pair.Value.ToString(), ability.Shortcut ?? string.Empty };
                return new string[] { "Unknown", pair.Value.ToString(), string.Empty };
            }
            if (o is Ability)
            {
                Ability ability = (Ability)o;
                return new string[] { ability.Name, "1", ability.Shortcut ?? string.Empty };
            }
            return new string[] { "", "", "" };
        }

        [Obfuscation(Exclude = false)]
        public override string ToString(string sortColumn, object o)
        {
            User user = o as User;
            Character ch = (user != null ? user.Character : o as Character);
            if (ch == null || ch.Abilities == null || ch.Abilities.Count == 0)
                return "You know no abilities.";

            StringTable table = new StringTable("Ability", "Level", "Shortcut");
            table.Columns[1].Alignment = StringTable.Column.Align.Right;

            foreach (KeyValuePair<Guid, int> pair in ch.Abilities)
            {
                Ability ability = Server.Instance != null ? Server.Instance.Get<Ability>(pair.Key) : null;
                if (ability != null)
                    table.AddRow(ability.Name, pair.Value.ToString(), ability.Shortcut ?? string.Empty);
            }

            if (!string.IsNullOrEmpty(sortColumn))
                table.Sort(sortColumn);
            else
                table.Sort("Ability");

            return table.ToString();
        }
    }
}
