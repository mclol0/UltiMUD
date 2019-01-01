using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using LuaInterface;
using Damdai;
using Damdai.TypeConverters;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("Customize what happens when players and NPCs are killed.")]
    [Obfuscation(Exclude = true)]
    public class DeathSettings
    {
        String message_ = new String("You have been killed!", "You have been killed!", null, null, "{1} has been killed!", null);
        Telnet.Color color_ = Telnet.Color.BrightRed;
        Area.Range npcBroadcastRange_ = Area.Range.Nearby;
        Area.Range pcBroadcastRange_ = Area.Range.World;
        bool restore_ = true;
        Location transferLocation_ = new Location();
        List<StatModifier> victimStatMods_ = new List<StatModifier>();
        List<StatModifier> killerStatMods_ = new List<StatModifier>();
        int lostItemPercentage_;
        int lostMoneyPercentage_;
        string script_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_ = new Lua();

        #region Properties
        [Description("The message that is displayed when a player kills another player.")]
        public String Message
        {
            get { return this.message_; }
            set { this.message_ = value; }
        }

        [Description("The color of the message.")]
        [DefaultValue(Telnet.Color.BrightRed)]
        public Telnet.Color Color
        {
            get { return this.color_; }
            set { this.color_ = value; }
        }

        [Description("Should the NPC death message be broadcast to the world, area, room, etc.?")]
        [DefaultValue(Area.Range.Nearby)]
        public Area.Range NpcBroadcastRange
        {
            get { return this.npcBroadcastRange_; }
            set { this.npcBroadcastRange_ = value; }
        }

        [Description("Should the PC death message be broadcast to the world, area, room, etc.?")]
        [DefaultValue(Area.Range.World)]
        public Area.Range PcBroadcastRange
        {
            get { return this.pcBroadcastRange_; }
            set { this.pcBroadcastRange_ = value; }
        }

        [Category("Victim")]
        [Description("The victim's stats to modify if they die. For example, reducing their current exp stat.\r\n\r\nObjects: actor (killer), target (victim)")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> VictimStatModifications
        {
            get { return this.victimStatMods_; }
            set { this.victimStatMods_ = value; }
        }

        [Category("Victim")]
        [Description("Should the victim be restored after they are killed?")]
        [DefaultValue(true)]
        public bool Restore
        {
            get { return this.restore_; }
            set { this.restore_ = value; }
        }

        [Category("Victim")]
        [Description("Where to transfer players when they die.")]
        public Location TransferLocation
        {
            get { return this.transferLocation_; }
            set { this.transferLocation_ = value; }
        }

        [Category("Killer")]
        [Description("The killer's stats to modify if they kill someone. For example, increasing their current exp stat.\r\n\r\nObjects: actor (killer), target (victim)")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> KillerStatModifications
        {
            get { return this.killerStatMods_; }
            set { this.killerStatMods_ = value; }
        }

        [Category("Victim")]
        [Description("The percentage of carried items the victim will lose. Items will either go to the killer, or fall to the ground if no killer is present.")]
        [DefaultValue(0)]
        public int LostItemPercentage
        {
            get { return this.lostItemPercentage_; }
            set
            {
                this.lostItemPercentage_ = value;
                this.lostItemPercentage_ = System.Math.Min(this.lostItemPercentage_, 100);
                this.lostItemPercentage_ = System.Math.Max(this.lostItemPercentage_, 0);
            }
        }

        [Category("Victim")]
        [Description("The percentage of carried money the victim will lose. Money will either go to the killer, or fall to the ground if no killer is present.")]
        [DefaultValue(0)]
        public int LostMoneyPercentage
        {
            get { return this.lostMoneyPercentage_; }
            set
            {
                this.lostMoneyPercentage_ = value;
                this.lostMoneyPercentage_ = System.Math.Min(this.lostMoneyPercentage_, 100);
                this.lostMoneyPercentage_ = System.Math.Max(this.lostMoneyPercentage_, 0);
            }
        }

        [Description("This script is executed when a player or npc is killed.\r\n\r\nObjects: victim, killer\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(StringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string OnKilled
        {
            get { return this.script_; }
            set { this.script_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public void Apply(Object victim, Object killer)
        {
            object[] rvals = this.DoScript(victim, killer);
            if (rvals != null && rvals[0].ToString() == "cancel")
                return;

            this.BroadcastMessage(victim, killer);
            this.ApplyStatMods(victim, killer);

            if (!(victim is Npc))
            {
                int amount = victim.GetChildren<Item>().Count * this.LostItemPercentage / 100;
                for (int x = 0; x < amount; ++x)
                {
                    List<Item> items = victim.GetChildren<Item>();
                    items[Server.RandomNumber.Next(items.Count)].Parent = (killer is Character ? killer : victim.Area);
                }

                Location loc = this.TransferLocation;
                Character ch = victim as Character;
                if (ch != null)
                {
                    Race race = ch.Race;
                    if (race != null && race.DeathLocation != null && race.DeathLocation.IsValid)
                        loc = race.DeathLocation;
                }

                if (loc != null && loc.IsValid)
                    victim.Location = loc;

                if (this.Restore)
                    victim.Restore(false);

                victim.Position = Object.Positions.Stopped;
            }
        }
        
        [Obfuscation(Exclude = false)]
        void BroadcastMessage(Object victim, Object killer)
        {
            victim.Send(this.Message.ToFirstPerson(this.Color, victim, killer));
            Telnet.Color[] colors = new Telnet.Color[3] { this.Color, this.Color, this.Color };
            switch ((victim is Npc ? this.NpcBroadcastRange : this.PcBroadcastRange))
            {
                case Area.Range.Room:
                    victim.Area.Send(this.Message, victim, killer, 0, new Function<bool, IObject>[] { Object.CanHear }, colors);
                    break;
                case Area.Range.Nearby:
                    victim.Area.Send(this.Message, victim, killer, (int)Area.Range.Nearby, new Function<bool, IObject>[] { Object.CanHear }, colors);
                    break;
                case Area.Range.Far:
                    victim.Area.Send(this.Message, victim, killer, (int)Area.Range.Far, new Function<bool, IObject>[] { Object.CanHear }, colors);
                    break;
                case Area.Range.Area:
                    victim.Area.Send(this.Message, victim, killer, -1, new Function<bool, IObject>[] { Object.CanHear }, colors);
                    break;
                case Area.Range.World:
                    Server.Instance.Echo(this.Message.ToThirdPersonAtFirstPerson(this.Color, victim, killer), victim);
                    break;
            }
        }
        
        [Obfuscation(Exclude = false)]
        void ApplyStatMods(Object victim, Object killer)
        {
            StatModifier.Apply(this.VictimStatModifications, victim, killer, victim, null, false);
            if (killer != null)
                StatModifier.Apply(this.KillerStatModifications, killer, killer, victim, null, false);
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }
        
        [Obfuscation(Exclude = false)]
        public object[] DoScript(Object victim, Object killer)
        {
            if (string.IsNullOrEmpty(this.script_))
                return null;

            try
            {
                lock (this.lua_)
                {
                    string str = this.script_;
                    this.lua_["victim"] = victim;
                    if (killer != null) this.lua_["killer"] = killer;
                    else str = str.Insert(0, "killer=nil\r\n");
                    return this.lua_.DoString(str);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR: DeathSettings");
                Debug.WriteLine(this.script_, "Script");
                Debug.WriteLine(e.ToString(), "StackTrace");
#if DEBUG
                victim.Send(e.Message);
#endif
            }
            return null;
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        void OnDeserialized(StreamingContext context)
        {
            if (this.victimStatMods_ == null) this.victimStatMods_ = new List<StatModifier>();
            if (this.killerStatMods_ == null) this.killerStatMods_ = new List<StatModifier>();
            this.lua_ = new Lua();
        }
    }
}
