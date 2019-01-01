using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ultimud
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public interface IServerSettings
    {
        string Name { get; set; }
        string Description { get; set; }
        string TitleScreen { get; set; }
        string MOTD { get; set; }

        int InventoryLimit { get; set; }
        int MapSize { get; set; }

        bool NumberCommas { get; set; }

        IScript Prompt { get; set; }
        //IScript OnDeath { get; set; }
        //IScript OnLevelUp { get; set; }

        IPvpSettings PvpSettings { get; }
        IInfoChannel InfoChannel { get; }

        Dictionary<Globals.Text, Telnet.Color> Colors { get; set; }
        BitArray Flags { get; set; }

        string GetColorCode(Globals.Text t);
        void SetColor(Globals.Text text, Telnet.Color color);
    }
}
