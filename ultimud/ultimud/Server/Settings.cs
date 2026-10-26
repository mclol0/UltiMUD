using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public partial class Settings
    {
        public enum Flag
        {
            Newlocked,
        }

        public enum Text
        {
            Help,
            Server,
            StatIncrease,
            StatDecrease,
            AbilityImprovements,
            AbilityPrepare1,
            AbilityPrepare2,
            AbilityPrepare3,
            AbilityPerform1,
            AbilityPerform2,
            AbilityPerform3,
            AbilitySuccess1,
            AbilitySuccess2,
            AbilitySuccess3,
            AbilityMiss1,
            AbilityMiss2,
            AbilityMiss3,
            AbilityFail1,
            AbilityFail2,
            AbilityFail3,
            MapColorSelf,
            MapColorPlayer,
        }

        #region Members
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        int port_;

        string titleScreen_ = "Welcome to UltiMUD";
        BitArray flags_ = new BitArray(16);
        Dictionary<Text, Telnet.Color> colors_ = new Dictionary<Text, Telnet.Color>();
        string defaultPrompt_ = "return user.Character.Name .. ' ({color:gray}' .. user.Character.Position:ToString() .. '{color:reset})> '";
        int inventoryLimit_;
        string motd_;
        Guid speedStat_;
        int mapSize_ = 11;
        string name_ = "Unnamed";
        string description_;
        LevelUpSettings levelUpSettings_ = new LevelUpSettings();
        PvpSettings pvpSettings_ = new PvpSettings();
        DeathSettings deathSettings_ = new DeathSettings();
        LookFormat lookFormat_ = new LookFormat();
        bool numberCommas_ = false;
        WhoFormat whoFormat_ = new WhoFormat();
        InfoChannel infoChannel_ = new InfoChannel();
        AbilitiesFormat abilitiesFormat_ = new AbilitiesFormat();
        StatsFormat statsFormat_ = new StatsFormat();
        bool simplemovement_;
        EqFormat eqformat_ = new EqFormat();
        string selectracehelp_ = "Your race affects your initial stats and location, as well as what abilities and items you can use.";
        string selectraceprompt_ = "Select your race: ";
        string selectjobhelp_ = "Your job affects your initial stats and location, as well as what abilities and items you can use.";
        string selectjobprompt_ = "Select your job: ";
        #endregion

        #region Properties
        [Category("Appearance")]
        [Description("The text that users first see when connecting to the server.")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string TitleScreen
        {
            get { return this.titleScreen_; }
            set { this.titleScreen_ = value; }
        }

        [Description("Various toggleable server settings.")]
        [Editor(typeof(FlagEditor<Flag>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<Flag>))]
        public BitArray Flags
        {
            get { return this.flags_; }
            set { this.flags_ = value; }
        }

        [Category("Appearance")]
        [Description("Modify color settings here.")]
        [Editor(typeof(ChannelColorEditor), typeof(UITypeEditor))]
        public Dictionary<Text, Telnet.Color> Colors
        {
            get { return this.colors_; }
            set { this.colors_ = value; }
        }

        [Category("Appearance")]
        [Description("This script generates the player prompt. The prompt is a line of text that is always visible to the player, which is used to display important information, such as stats and position.\r\n\r\nObjects: user\r\nReturns: string\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [DefaultValue("return user.Character.Name .. ' ({color:gray}' .. user.Character.Position:ToString() .. '{color:reset})> '")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string Prompt
        {
            get { return this.defaultPrompt_; }
            set { this.defaultPrompt_ = value; }
        }

        [Description("The maximum number of items allowed in a player's inventory. A character's inventory is a holding area for items that are in their posession, but aren't currently being used. A value of 0 means unlimited.")]
        [DefaultValue(0)]
        public int InventoryLimit
        {
            get { return this.inventoryLimit_; }
            set { this.inventoryLimit_ = value; }
        }

        [Category("Appearance")]
        [Description("The 'Message Of The Day'. This text will be displayed to a user whenever they login.")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string MOTD
        {
            get { return this.motd_; }
            set { this.motd_ = value; }
        }

        [Category("Movement")]
        [Description("The stat that determines an object's maximum speed.")]
        [Editor(typeof(StatSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Stat.IdToNameConverter))]
        public Guid SpeedStat
        {
            get { return this.speedStat_; }
            set { this.speedStat_ = value; }
        }

        [Category("Movement")]
        [Description("When true, movement commands move one step immediately instead of starting continuous travel.")]
        [DefaultValue(false)]
        public bool SimpleMovement
        {
            get { return this.simplemovement_; }
            set { this.simplemovement_ = value; }
        }

        [Category("Appearance")]
        [Description("The size of the in-game map that players see when using the LOOK command.")]
        [DefaultValue(11)]
        public int MapSize
        {
            get { return this.mapSize_; }
            set { this.mapSize_ = (value % 2 == 0 ? value + 1 : value); }
        }

        [Category("Appearance")]
        [Description("The name of this MUD.")]
        public string Name
        {
            get { return this.name_; }
            set
            {
                value = Telnet.Decolorize(value);
                bool cancel = false;
                if (NameChangedEvent != null)
                    cancel = NameChangedEvent(this.name_, value);
                if (!cancel)
                    this.name_ = value;
            }
        }

        [Category("Appearance")]
        [Description("A small paragraph describing this MUD.")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]        
        public string Description
        {
            get { return this.description_; }
            set { this.description_ = value; }
        }

        [Browsable(false)]
        public int Port
        {
            get { return this.port_; }
            set { this.port_ = value; }
        }

        public PvpSettings PvpSettings
        {
            get { return this.pvpSettings_; }
            set { this.pvpSettings_ = value; }
        }
        public DeathSettings DeathSettings
        {
            get { return this.deathSettings_; }
            set { this.deathSettings_ = value; }
        }
        public LevelUpSettings LevelUpSettings
        {
            get { return this.levelUpSettings_; }
            set { this.levelUpSettings_ = value; }
        }

        [Category("Appearance")]
        public LookFormat LookFormat
        {
            get { return this.lookFormat_; }
            set { this.lookFormat_ = value; }
        }

        [Category("Appearance")]
        public WhoFormat WhoFormat
        {
            get { return this.whoFormat_; }
            set { this.whoFormat_ = value; }
        }

        [Category("Appearance")]
        public StatsFormat StatsFormat
        {
            get { return this.statsFormat_; }
            set { this.statsFormat_ = value; }
        }

        [Category("Appearance")]
        public EqFormat EqFormat
        {
            get { return this.eqformat_; }
            set { this.eqformat_ = value; }
        }

        [Category("Appearance")]
        public AbilitiesFormat AbilitiesFormat
        {
            get { return this.abilitiesFormat_; }
            set { this.abilitiesFormat_ = value; }
        }

        [Category("Appearance")]
        [Description("Display numbers with commas? (123,456,789)")]
        [DefaultValue(false)]
        public bool NumberCommas
        {
            get { return this.numberCommas_; }
            set { this.numberCommas_ = value; }
        }

        [Browsable(false)]
        public string NumberFormat
        {
            get { return (this.NumberCommas ? "n0" : ""); }
        }

        public InfoChannel InfoChannel
        {
            get { return this.infoChannel_; }
            set { this.infoChannel_ = value; }
        }

        [Description("The text that is displayed race selection during creation.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string SelectRaceHelp
        {
            get { return this.selectracehelp_; }
            set { this.selectracehelp_ = value; }
        }

        [Description("The prompt that is displayed at race selection during creation.")]
        public string SelectRacePrompt
        {
            get { return this.selectraceprompt_; }
            set { this.selectraceprompt_ = value.Trim() + " "; }
        }
        
        [Description("The text that is displayed job selection during creation.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string SelectJobHelp
        {
            get { return this.selectjobhelp_; }
            set { this.selectjobhelp_ = value; }
        }
        
        [Description("The prompt that is displayed at job selection during creation.")]
        public string SelectJobPrompt
        {
            get { return this.selectjobprompt_; }
            set { this.selectjobprompt_ = value.Trim() + " "; }
        }
        #endregion

        #region Events
        [Obfuscation(Exclude = false)]
        public delegate bool NameChangedHandler(string oldName, string newName);

        [Obfuscation(Exclude = false)]
        [field: NonSerialized]
        static public event NameChangedHandler NameChangedEvent;
        #endregion

        #region Methods
        public Settings()
        {
            this.EnsureColorsInitialized();

            foreach (int val in Enum.GetValues(typeof(Flag)))
                this.Flags.Set(val, false);
        }

        void EnsureColorsInitialized()
        {
            if (this.colors_ == null)
                this.colors_ = new Dictionary<Text, Telnet.Color>();
            else
                this.colors_.OnDeserialization(null);

            if (!this.Colors.ContainsKey(Text.AbilityPrepare1)) this.Colors.Add(Text.AbilityPrepare1, Telnet.Color.BrightBlue);
            if (!this.Colors.ContainsKey(Text.AbilityPrepare2)) this.Colors.Add(Text.AbilityPrepare2, Telnet.Color.BrightBlue);
            if (!this.Colors.ContainsKey(Text.AbilityPrepare3)) this.Colors.Add(Text.AbilityPrepare3, Telnet.Color.Reset);
            if (!this.Colors.ContainsKey(Text.AbilityPerform1)) this.Colors.Add(Text.AbilityPerform1, Telnet.Color.Green);
            if (!this.Colors.ContainsKey(Text.AbilityPerform2)) this.Colors.Add(Text.AbilityPerform2, Telnet.Color.Green);
            if (!this.Colors.ContainsKey(Text.AbilityPerform3)) this.Colors.Add(Text.AbilityPerform3, Telnet.Color.Reset);
            if (!this.Colors.ContainsKey(Text.AbilitySuccess1)) this.Colors.Add(Text.AbilitySuccess1, Telnet.Color.BrightMagenta);
            if (!this.Colors.ContainsKey(Text.AbilitySuccess2)) this.Colors.Add(Text.AbilitySuccess2, Telnet.Color.BrightRed);
            if (!this.Colors.ContainsKey(Text.AbilitySuccess3)) this.Colors.Add(Text.AbilitySuccess3, Telnet.Color.Reset);
            if (!this.Colors.ContainsKey(Text.AbilityMiss1)) this.Colors.Add(Text.AbilityMiss1, Telnet.Color.Gray);
            if (!this.Colors.ContainsKey(Text.AbilityMiss2)) this.Colors.Add(Text.AbilityMiss2, Telnet.Color.Gray);
            if (!this.Colors.ContainsKey(Text.AbilityMiss3)) this.Colors.Add(Text.AbilityMiss3, Telnet.Color.Reset);
            if (!this.Colors.ContainsKey(Text.AbilityFail1)) this.Colors.Add(Text.AbilityFail1, Telnet.Color.Gray);
            if (!this.Colors.ContainsKey(Text.AbilityFail2)) this.Colors.Add(Text.AbilityFail2, Telnet.Color.Gray);
            if (!this.Colors.ContainsKey(Text.AbilityFail3)) this.Colors.Add(Text.AbilityFail3, Telnet.Color.Reset);
            if (!this.Colors.ContainsKey(Text.AbilityImprovements)) this.Colors.Add(Text.AbilityImprovements, Telnet.Color.BrightGreen);
            if (!this.Colors.ContainsKey(Text.StatIncrease)) this.Colors.Add(Text.StatIncrease, Telnet.Color.BrightGreen);
            if (!this.Colors.ContainsKey(Text.StatDecrease)) this.Colors.Add(Text.StatDecrease, Telnet.Color.BrightRed);
            if (!this.Colors.ContainsKey(Text.Help)) this.Colors.Add(Text.Help, Telnet.Color.Cyan);
            if (!this.Colors.ContainsKey(Text.Server)) this.Colors.Add(Text.Server, Telnet.Color.BrightRed);
            if (!this.Colors.ContainsKey(Text.MapColorPlayer)) this.Colors.Add(Text.MapColorPlayer, Telnet.Color.BrightRed);
            if (!this.Colors.ContainsKey(Text.MapColorSelf)) this.Colors.Add(Text.MapColorSelf, Telnet.Color.BrightCyan);
        }
        
        public string GetColorCode(Text t)
        {
            if (this.Colors != null && this.Colors.ContainsKey(t))
                return Telnet.GetColorCode((Telnet.Color)this.Colors[t]);
            else
                return Telnet.ColorCode.Reset;
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }
        
        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            this.EnsureColorsInitialized();
            if (this.InfoChannel == null)
                this.InfoChannel = new InfoChannel();
            if (this.whoFormat_ == null)
                this.whoFormat_ = new WhoFormat();
            if (this.abilitiesFormat_ == null)
                this.abilitiesFormat_ = new AbilitiesFormat();
            if (this.statsFormat_ == null)
                this.statsFormat_ = new StatsFormat();
            if (this.eqformat_ == null)
                this.eqformat_ = new EqFormat();
            if (string.IsNullOrEmpty(this.selectracehelp_))
                selectracehelp_ = "Your race affects your initial stats and location, as well as what abilities and items you can use.";
            if (string.IsNullOrEmpty(this.selectraceprompt_))
                selectraceprompt_ = "Select your race: ";
            if (string.IsNullOrEmpty(this.selectjobhelp_))
                selectjobhelp_ = "Your job affects your initial stats and location, as well as what abilities and items you can use.";
            if (string.IsNullOrEmpty(this.selectjobprompt_))
                selectjobprompt_ = "Select your job: ";
        }
        #endregion
    }
}
