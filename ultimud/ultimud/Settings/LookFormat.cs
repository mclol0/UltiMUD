using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("Customize the output of Character.GenerateLookDescription script function.")]
    [Obfuscation(Exclude = true)]
    public class LookFormat
    {
        string generalFormat_ = "{color:brightcyan}{location}{color:reset} [{weather}]\r\n{map}\r\n{description}\r\n{objects}";
        string objectFormat_ = "{name}";
        string pcFormat_ = "{color:brightwhite}{name}{color:reset} ({color:gray}{position}{color:reset})";
        string npcFormat_ = "{name} ({color:gray}{position}{color:reset})";
        string itemformatplural_ = "{count} {name}";
        string pcformatplural_ = "{count} {color:brightwhite}{name}{color:reset}";
        string npcformatplural_ = "{count} {name}";

        #region Properties
        [Description("General format template for look descriptions.")]
        public string GeneralFormat
        {
            get { return this.generalFormat_; }
            set { this.generalFormat_ = value; }
        }

        [Description(@"The output of items in the Character.GenerateLookDescription function.
  {name} - Item name.
  {distance} - Item distance.
  {position} - Item's position.
  {direction} - Item direction (North).
  {dir} - Item direction (N).
  {stat:[name]} - Item's current and max stat values. (Hp: 23/40)
  {stat:[name].current} - Item's current stat value. (23)
  {stat:[name].max} - Item's max stat value. (40)
  {stat:[name].percent} - Displays a percentage of item's current stat. (55%)
  {stat:[name].meter} - Displays a meter of item's current stat. (******    )
  {count}")]
        [DefaultValue("{name}")]
        public string ItemFormat
        {
            get { return this.objectFormat_; }
            set { this.objectFormat_ = value; }
        }

        [Description(@"The output of players in the Character.GenerateLookDescription function.
  {name} - PC name.
  {distance} - PC distance.
  {position} - PC's position.
  {direction} - PC direction (North).
  {dir} - PC direction (N).
  {stat:[name]} - PC's current and max stat values. (Hp: 23/40)
  {stat:[name].current} - PC's current stat value. (23)
  {stat:[name].max} - PC's max stat value. (40)
  {stat:[name].percent} - Displays a percentage of PC's current stat. (55%)
  {stat:[name].meter} - Displays a meter of PC's current stat. (******    )
  {count}")]
        [DefaultValue("{color:brightwhite}{name}{color:reset} ({color:gray}{position}{color:reset})")]
        public string PcFormat
        {
            get { return this.pcFormat_; }
            set { this.pcFormat_ = value; }
        }

        [Description(@"The output of NPCs in the Character.GenerateLookDescription function.
  {name} - NPC name.
  {distance} - NPC distance.
  {position} - NPC's position.
  {direction} - NPC direction (North).
  {dir} - NPC direction (N).
  {stat:[name]} - NPC's current and max stat values. (Hp: 23/40)
  {stat:[name].current} - NPC's current stat value. (23)
  {stat:[name].max} - NPC's max stat value. (40)
  {stat:[name].percent} - Displays a percentage of NPC's current stat. (55%)
  {stat:[name].meter} - Displays a meter of NPC's current stat. (******    )
  {count}")]
        [DefaultValue("{name} ({color:gray}{position}{color:reset})")]
        public string NpcFormat
        {
            get { return this.npcFormat_; }
            set { this.npcFormat_ = value; }
        }

        [DefaultValue("{count} {name}")]
        public string ItemFormatPlural
        {
            get { return this.itemformatplural_; }
            set { this.itemformatplural_ = value; }
        }

        [DefaultValue("{count} {color:brightwhite}{name}{color:reset}")]
        public string PcFormatPlural
        {
            get { return this.pcformatplural_; }
            set { this.pcformatplural_ = value; }
        }
        
        [DefaultValue("{count} {name}")]
        public string NpcFormatPlural
        {
            get { return this.npcformatplural_; }
            set { this.npcformatplural_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        internal string FormatObject(Character ch, Object o, int count)
        {
            string str = null;
            if (o is Npc)
                str = (count > 1 && !string.IsNullOrEmpty(this.NpcFormatPlural) ? this.NpcFormatPlural : this.NpcFormat);
            else if (o is Character)
                str = (count > 1 && !string.IsNullOrEmpty(this.PcFormatPlural) ? this.PcFormatPlural : this.PcFormat);
            else
                str = (count > 1 && !string.IsNullOrEmpty(this.ItemFormatPlural) ? this.ItemFormatPlural : this.ItemFormat);

            if (string.IsNullOrEmpty(str))
                return null;

            str = Telnet.Colorize(str).Replace("{count}", count.ToString());
            if (count > 1)
            {
                str = str.Replace("{name}", (!string.IsNullOrEmpty(o.Plural) ? o.Plural : o.Name));
                str = String.ReplaceVars(str, "n/a");
            }
            else
            {
                str = str.Replace("{name}", o.Name);
                str = str.Replace("{position}", o.Position.ToString());
                if (ch != null && str.Contains("{distance}"))
                    str = str.Replace("{distance}", ch.Distance(o).ToString());
                if (o.Location != null && str.Contains("{location}"))
                    str = str.Replace("{location}", o.Location.Point.X + "," + o.Location.Point.Y);
                if (ch != null && o.Location != null && str.Contains("{direction}"))
                    str = str.Replace("{direction}", ch.GetDirection(o.Location.Point).ToString());
                if (ch != null && o.Location != null && str.Contains("{dir}"))
                    str = str.Replace("{dir}", ((Area.ShortDirection)ch.GetDirection(o.Location.Point)).ToString());
                if (o.Stats != null)
                    str = Stat.ResolveVars(str, o.Stats.Values);
            }

            return str;
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }

        [System.Runtime.Serialization.OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            if (string.IsNullOrEmpty(this.generalFormat_))
                this.generalFormat_ = "{color:brightcyan}{location}{color:reset} [{weather}]\r\n{map}\r\n{description}\r\n{objects}";
        }
    }
}
