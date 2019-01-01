using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;
using Damdai;
using LuaInterface;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Npc : Character
    {
        public enum Flags
        {
            Aggressive,
        }
        public enum TeachTriggers
        {
            OnRequest,
            OnDefeat
        }
        public enum ScriptType
        {
            OnDefeat,
            OnSpawn,
            OnUpdate
        }

        #region Members
        NpcReset reset_;
        bool leaveCorpse_ = true;
        BitArray npcFlags_ = new BitArray(16);
        Timer attackdelay_ = new Timer(new TimeSpan(0, 0, 2));
        TeachTriggers teachtrigger_ = TeachTriggers.OnRequest;
        Dictionary<Guid, int> teachableabilities_ = new Dictionary<Guid, int>();
        Dictionary<ScriptType, Guid> scripts_ = new Dictionary<ScriptType, Guid>();
        List<TextTrigger> texttriggers_ = new List<TextTrigger>();
        Guid blueprintid_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Timer updateTimer_ = new Timer(new TimeSpan(0, 0, 20));

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_ = new Lua();
        #endregion

        #region Properties
        [Browsable(false)]
        public NpcReset Reset
        {
            get { return this.reset_; }
            set { this.reset_ = value; }
        }

        [Description("The abilities that this NPC can teach. If TeachTrigger is OnRequest, value is cost to learn the corresponding ability. If TeachTrigger is OnDefeat, value is the chance of the killer learning the corresponding ability.")]
        [Editor(typeof(CharacterAbilitySelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public Dictionary<Guid, int> TeachableAbilities
        {
            get { return this.teachableabilities_; }
            set { this.teachableabilities_ = value; }
        }

        [Description("When should this NPC teach its abilities to others? If set to OnDeath, a random ability will be selected from the list of TeachableAbilities.")]
        public TeachTriggers TeachTrigger
        {
            get { return this.teachtrigger_; }
            set { this.teachtrigger_ = value; }
        }

        [Description(@"Text trigger scripts are invoked when the NPC sees text that matches the specified regex string. For help, press f1.")]
        [Category("Scripts")]
        public List<TextTrigger> TextTriggers
        {
            get { return this.texttriggers_; }
            set { this.texttriggers_ = value; }
        }

        [Browsable(false)]
        Dictionary<ScriptType, Guid> Scripts
        {
            get { return this.scripts_; }
        }

        [Editor(typeof(Script.NpcScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Description("This script is executed when this npc is killed.\r\n\r\nObjects: npc, killer\r\nReturns: nothing\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Category("Scripts")]
        public Guid OnDefeat
        {
            get
            {
                Guid id;
                if (!this.Scripts.TryGetValue(ScriptType.OnDefeat, out id))
                    this.Scripts.Add(ScriptType.OnDefeat, Guid.Empty);
                return id;
            }
            set
            {
                if (this.Scripts.ContainsKey(ScriptType.OnDefeat))
                    this.Scripts[ScriptType.OnDefeat] = value;
                else
                    this.Scripts.Add(ScriptType.OnDefeat, value);
            }
        }

        [Editor(typeof(Script.NpcScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Description("This script is executed when this npc is spawned.\r\n\r\nObjects: npc\r\nReturns: nothing\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Category("Scripts")]
        public Guid OnSpawn
        {
            get
            {
                Guid id;
                if (!this.Scripts.TryGetValue(ScriptType.OnSpawn, out id))
                    this.Scripts.Add(ScriptType.OnSpawn, Guid.Empty);
                return id;
            }
            set
            {
                if (this.Scripts.ContainsKey(ScriptType.OnSpawn))
                    this.Scripts[ScriptType.OnSpawn] = value;
                else
                    this.Scripts.Add(ScriptType.OnSpawn, value);
            }
        }
        
        [Editor(typeof(Script.NpcScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Description("Objects: npc\r\nReturns: nothing\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Category("Scripts")]
        public Guid OnUpdate
        {
            get
            {
                Guid id;
                if (!this.Scripts.TryGetValue(ScriptType.OnUpdate, out id))
                    this.Scripts.Add(ScriptType.OnUpdate, Guid.Empty);
                return id;
            }
            set
            {
                if (this.Scripts.ContainsKey(ScriptType.OnUpdate))
                    this.Scripts[ScriptType.OnUpdate] = value;
                else
                    this.Scripts.Add(ScriptType.OnUpdate, value);
            }
        }

        [Browsable(false)]
        public bool CanTeach
        {
            get
            {
                if (this.TeachableAbilities == null || 
                    this.TeachableAbilities.Count == 0 || 
                    this.TeachTrigger != TeachTriggers.OnRequest)
                {
                    return false;
                }

                bool found = false;
                foreach (Guid abilityId in this.TeachableAbilities.Keys)
                {
                    if (Server.Instance.Get<Ability>(abilityId) != null)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
        }

        [Description("Whether or not this NPC will leave a corpse when it is killed. Corpses decompose after 5 minutes.")]
        [DefaultValue(true)]
        public bool LeaveCorpse
        {
            get { return this.leaveCorpse_; }
            set { this.leaveCorpse_ = value; }
        }

        [DisplayName("Money")]
        public new int Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }

        [Description("Various flags regarding the behavior of this NPC.")]
        [Browsable(false)]
        public BitArray NpcFlags
        {
            get
            {
                if (this.npcFlags_ == null)
                {
                    this.npcFlags_ = new BitArray(16);
                }
                return this.npcFlags_;
            }
            set { this.npcFlags_ = value; }
        }

        [Description("The delay between this NPC's attacks.")]
        public Timer AttackDelay
        {
            get { return this.attackdelay_; }
            set { this.attackdelay_ = value; }
        }

#if DEBUG
        public Timer UpdateTimer
        {
            get { return this.updateTimer_; }
        }
#endif
        #endregion

        #region Methods
        public Npc()
        {
        }
        ~Npc()
        {
            if (this.lua_ != null)
                this.lua_.Dispose();
        }

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Npc)))
                Server.Instance.DataSets.Add(typeof(Npc), new Dictionary<Guid, IIdentifiable>());
        }
        public void Init(Npc other)
        {
            base.Init(other);
            this.Reset = other.Reset;
            this.updateTimer_ = new Timer(new TimeSpan(0, 0, 20));
            this.lua_ = new LuaInterface.Lua();
            if (other.npcFlags_ != null) this.npcFlags_ = new BitArray(other.npcFlags_);
            this.attackdelay_ = new Timer(other.attackdelay_.Length);
        }
        
        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if (this.FightSystem != null) this.FightSystem.Update(this);
            else if (!this.updateTimer_.Update() && this.Area != null && this.Position != Positions.Dead && this.Master == null)
            {
                this.DoScript(ScriptType.OnUpdate, null);
                this.updateTimer_.Reset();
            }
            base.Update();
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return (!string.IsNullOrEmpty(this.Name) ? this.Name : base.ToString());
        }
        
        public static Npc Create(Npc blueprint)
        {
            Npc npc = blueprint.MemberwiseClone() as Npc;
            npc.Init(blueprint);
            npc.blueprintid_ = blueprint.Id;
            return npc;
        }
        
        public override bool CanMove(Point location, bool force)
        {
            if (!force && this.Reset != null && this.Reset.Boundary != null && this.reset_.Boundary.Points.Count > 0 && !this.Reset.Boundary.Contains(location))
                return false;

            Area.Layer l = this.Area.GetLayer<Area.Layer>(location, true);
            if (l != null && l.LayerFlags.Get((int)Area.Layer.LayerFlag.NoNpc))
                return false;

            return base.CanMove(location, force);
        }
        
        public void DoRandomMove()
        {
            if (this.Position == Positions.Stopped && this.CurrentAction == null)
                this.DoCommand("travel random 0");
        }

        public override void Die(Object killer)
        {
            if (!this.LeaveCorpse || this.Position == Positions.Dead)
            {
                //Corpse decomposed.
                if (!this.LeaveCorpse)
                    this.DoScript(ScriptType.OnDefeat, killer);
                base.Die(killer);
                if (this.LeaveCorpse)
                    this.DropItems(true);
                this.Parent = null;
                if (!this.leaveCorpse_)
                    this.CheckTeach(killer as Character);
            }
            else
            {
                this.DoScript(ScriptType.OnDefeat, killer);
                base.Die(killer);
                this.CheckTeach(killer as Character);
                this.TurnIntoCorpse();
            }
        }

        [Obfuscation(Exclude = false)]
        void TurnIntoCorpse()
        {
            this.LifeSpan = new Damdai.Timer(new TimeSpan(0, 5, 0));
            this.Name = "the corpse of " + this.Name;
            this.DeathMessage = "{1} decomposes.";
            this.ObjectFlags.Set((int)ObjFlags.Container, true);
            this.MapColor = Telnet.Color.Gray;
            Dictionary<BodyParts, Item> dict = new Dictionary<BodyParts,Item>(this.Equipment);
            foreach (Item item in dict.Values)
                if (!this.Remove(item, true))
                    this.Drop(item, false);

            if (this.Value > 0)
            {
                Item money = new Item();
                money.Name = "a pile of money";
                money.Value = this.Value;
                money.ObjectFlags.Set((int)ObjFlags.Money, true);
                money.MapColor = Telnet.Color.BrightYellow;
                money.MapSymbol = "$";
                money.Parent = this;
                this.Value = 0;
            }
        }

        [Obfuscation(Exclude = false)]
        public override void Send(string str, IObject actor)
        {
            this.CheckTriggers(str, actor);
        }
        
        [Obfuscation(Exclude = false)]
        void CheckTriggers(string str, IObject actor)
        {
            str = Telnet.Decolorize(str);
            if (this.FightSystem == null && this.NpcFlags.Get((int)Flags.Aggressive) && Regex.IsMatch(str, @"^[\w ]+ arrives"))
            {
                Ability ability = this.GetRandomAggressiveAbility(actor);
                if (ability != null)
                    this.ReplaceCurrentAction(new CommandAction(ability.Name + " " + actor.Name, 2, this));
            }

            if (this.TextTriggers != null)
            {
                foreach (TextTrigger trigger in this.TextTriggers)
                {
                    if (!string.IsNullOrEmpty(trigger.Response) && trigger.IsMatch(str, actor))
                    {
                        if (this.CurrentAction != null)
                        {
                            switch (trigger.BusyAction)
                            {
                                case TextTrigger.BusyActions.Ignore: continue;
                                case TextTrigger.BusyActions.Interrupt: 
                                    this.Stop(false);
                                    this.Actions.Clear();
                                    break;
                            }
                        }

                        try
                        {
                            string strscript = string.Empty;
                            this.lua_["npc"] = this;
                            if (actor == null) strscript += "actor=nil\r\n";
                            else this.lua_["actor"] = actor;
                            this.lua_.DoString(strscript + trigger.Response.Replace("{1}", this.Name).Replace("{2}", actor.Name));
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.Message, "ERROR: Npc: TextTrigger");
                            Debug.WriteLine(trigger.Response, "Script");
                            Debug.WriteLine(e.ToString(), "StackTrace");
                        }

                        return;
                    }
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        void CheckTeach(Character killer)
        {
            if (killer != null && this.TeachableAbilities != null && this.TeachableAbilities.Count > 0 && this.TeachTrigger == TeachTriggers.OnDefeat)
            {
                foreach (KeyValuePair<Guid, int> kvp in this.TeachableAbilities)
                {
                    Guid id = kvp.Key;
                    int chance = kvp.Value;
                    Ability ability = Server.Instance.Get<Ability>(id);
                    if (ability != null)
                    {
                        if (!killer.Abilities.ContainsKey(id) && killer.CanUse(ability, true) && Server.RandomNumber.Next(100) <= chance)
                        {
                            killer.Abilities.Add(id, 0);
                            killer.Send(Server.Instance.Settings.GetColorCode(Settings.Text.AbilityImprovements) + "You have learned a new ability: " + ability.Name);
                            break;
                        }
                    }
                    else
                        Debug.WriteLine("Ability not found: " + id, "WARNING");
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void DoScript(ScriptType type, Object killer)
        {
            Guid scriptid;
            if (!this.Scripts.TryGetValue(type, out scriptid) || scriptid == Guid.Empty)
                return;

            Script script = Server.Instance.Get<Script>(scriptid);
            if (script == null)
            {
                Trace.WriteLine("Removing invalid script: " + type, "Npc");
                this.Scripts[type] = Guid.Empty;
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(script.Code))
                    return;

                string str = string.Empty;
                this.lua_["npc"] = this;
                if (killer == null) str = "killer=nil\r\n";
                else this.lua_["killer"] = killer;
                str += script.Code;
                this.lua_.DoString(str);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR: Npc: " + type + " (" + this.Name + ")");
                Debug.WriteLine(script.Code, "Script");
                Debug.WriteLine(type.ToString(), "ScriptType");
                Debug.WriteLine(this.Name, "Npc");
                Debug.WriteLine(e.ToString(), "StackTrace");
            }
        }
        
        public bool IsTargetOutOfRange(Object target)
        {
            foreach (Guid aid in this.Abilities.Keys)
            {
                Ability a = Server.Instance.Get<Ability>(aid);
                if (a != null && a.CheckRange(this, target))
                    return false;
            }
            return true;
        }
        
        public static bool IsNpc(Object o)
        {
            return (o is Npc);
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal new void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);
            this.updateTimer_ = new Timer(new TimeSpan(0, 0, 20));
            this.lua_ = new LuaInterface.Lua();
            if (this.attackdelay_ == null) this.attackdelay_ = new Timer(new TimeSpan(0, 0, 2));
            if (this.teachableabilities_ == null) this.teachableabilities_ = new Dictionary<Guid, int>();
            if (this.texttriggers_ == null) this.texttriggers_ = new List<TextTrigger>();
            if (this.scripts_ == null) this.scripts_ = new Dictionary<ScriptType, Guid>();

            if (this.blueprintid_ != Guid.Empty)
            {
                Server.Instance.Loaded += delegate()
                {
                    Npc blueprint = Server.Instance.Get<Npc>(this.blueprintid_);
                    if (blueprint != null)
                    {
                        this.TeachableAbilities = blueprint.TeachableAbilities;
                        this.scripts_ = blueprint.Scripts;
                        this.TextTriggers = blueprint.TextTriggers;
                        this.AttackDelay = blueprint.AttackDelay;
                        this.NpcFlags = blueprint.NpcFlags;
                    }
                };
            }
        }
        #endregion
    }
}
