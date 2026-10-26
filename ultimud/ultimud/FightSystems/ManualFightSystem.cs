using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using System.Runtime.Serialization;
using LuaInterface;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class ManualFightSystem : IFightSystem
    {
        public enum ScriptType
        {
            OnNpcUpdate,
        }
        public enum RestoreTypes
        {
            None,
            Players,
            Npcs,
            All
        }

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Dictionary<Object, Object> participants_ = new Dictionary<Object, Object>();

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        System.Timers.Timer updateTimer_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Dictionary<Object, Queue<char>> combos_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_ = new Lua();
        
        Guid comboAbilityId_;
#pragma warning disable 0169, 0414
        int comboChance_; //Deprecated.
        int comboLength_; //Deprecated.
#pragma warning restore 0169, 0414
        List<Guid> comboTriggers_;
        Dictionary<ScriptType, string> scripts_ = new Dictionary<ScriptType, string>();
        RestoreTypes restoretype_ = RestoreTypes.None;
        string combochance_ = "return math.random(100) < 25";
        string combolength_ = "return 5";

        #region Properties
        [Browsable(false)]
        public Dictionary<Object, Object> Participants
        {
            get { return this.participants_; }
        }

        [DisplayName("Combo Ability")]
        [Description("The ability that is used during combos. This will normally be set to a general attack ability.")]
        [TypeConverter(typeof(Server.IdToNameConverter<Ability>))]
        [Editor(typeof(Server.SingleObjectSelector<Ability>), typeof(UITypeEditor))]
        public Guid ComboAbilityId
        {
            get { return this.comboAbilityId_; }
            set { this.comboAbilityId_ = value; }
        }

        [Description("The formula to determine if a combo will be triggered.\r\n\r\nObjects: actor, target\r\nReturns: bool\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [DefaultValue("return math.random(100) < 25")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string ComboChance
        {
            get { return this.combochance_; }
            set { this.combochance_ = value; }
        }

        [Description("The formula to determine the combo length when a combo is triggered.\r\n\r\nObjects: actor, target\r\nReturns: int\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [DefaultValue("return 5")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        public string ComboLength
        {
            get { return this.combolength_; }
            set { this.combolength_ = value; }
        }

        [Description("The abilities that can trigger combos when successful.")]
        [Editor(typeof(Server.MultiObjectSelector<Ability>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public List<Guid> ComboTriggers
        {
            get { return this.comboTriggers_; }
            set { this.comboTriggers_ = value; }
        }

        [Editor(typeof(StringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Scripts")]
        [Description("Objects: npc, target, fight\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        public string OnNpcUpdate
        {
            get
            {
                string script;
                if (!this.scripts_.TryGetValue(ScriptType.OnNpcUpdate, out script))
                    this.scripts_.Add(ScriptType.OnNpcUpdate, null);
                return script;
            }
            set
            {
                if (this.scripts_.ContainsKey(ScriptType.OnNpcUpdate))
                    this.scripts_[ScriptType.OnNpcUpdate] = value;
                else
                    this.scripts_.Add(ScriptType.OnNpcUpdate, value);
            }
        }

        [Description("Who will be restored when this fight is finished or expires.")]
        [DefaultValue(RestoreTypes.None)]
        public RestoreTypes RestoreType
        {
            get { return this.restoretype_; }
            set { this.restoretype_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        void updateTimer__Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Update();
        }

        [Obfuscation(Exclude = false)]
        public bool CanUseCommand(User user, Command command)
        {
            return true;
        }

        [Obfuscation(Exclude = false)]
        public bool CanUseAbility(Object actor, Ability ability)
        {
            return true;
        }

        [Obfuscation(Exclude = false)]
        public bool ProcessInput(User user, string input)
        {
            Queue<char> combo = null;
            if (this.combos_.TryGetValue(user.Character, out combo))
            {
                bool found = false;
                Object target = this.Participants[user.Character];
                if (combo.Dequeue().ToString() == input && this.Participants.ContainsKey(target))
                {
                    found = true;
                    if (target.CurrentAction is RecoverAction)
                    {
                        Ability ability = Server.Instance.Get<Ability>(this.ComboAbilityId);
                        if (ability != null)
                            ability.Hit(user.Character, target, user.Character.GetEquippedItem(ability.BodyPart));
                    }
                    else
                    {
                        user.Send(String.Capitalize(target.Name) + " has already recovered.");
                        combo.Clear();
                    }
                }

                if (combo.Count < 1)
                    this.combos_.Remove(user.Character);

                return found;
            }

            return false;
        }

        [Obfuscation(Exclude = false)]
        public void Start()
        {
            if (this.updateTimer_ != null)
                this.updateTimer_.Start();
            this.lua_ = new Lua();
        }
        
        public void Finish(string message)
        {
            if (this.updateTimer_ != null)
                this.updateTimer_.Stop();
            Object[] participants = new Object[this.participants_.Keys.Count];
            this.participants_.Keys.CopyTo(participants, 0);
            foreach (Object o in participants)
            {
                o.Send(message);
                this.RemoveParticipant(o);
            }
        }

        [Obfuscation(Exclude = false)]
        public void Merge(IFightSystem other)
        {
            if (other == null || other == this)
                return;

            ManualFightSystem otherManual = other as ManualFightSystem;
            if (otherManual != null && otherManual.updateTimer_ != null)
                otherManual.updateTimer_.Stop();

            Dictionary<Object, Object> copy = new Dictionary<Object, Object>(other.Participants);
            other.Participants.Clear();
            foreach (KeyValuePair<Object, Object> pair in copy)
            {
                pair.Key.DieEvent -= other.RemoveParticipant;
                if (otherManual != null)
                    pair.Key.AbilityPerformedEvent -= otherManual.o_AbilityPerformedEvent;
                this.AddParticipant(pair.Key, pair.Value);
            }
        }

        [Obfuscation(Exclude = false)]
        public void Update()
        {
            Object[] participants = new Object[this.Participants.Keys.Count];
            this.Participants.Keys.CopyTo(participants, 0);
            foreach (Object o in participants)
            {
                if ((DateTime.Now - o.LastAggressiveAct).TotalSeconds > 60)
                {
                    o.Send("You are no longer involved in a fight.");
                    this.RemoveParticipant(o);
                    if (this.restoretype_ == RestoreTypes.All || (this.restoretype_ == RestoreTypes.Npcs && o is Npc) || (this.restoretype_ == RestoreTypes.Players && o is Character && !(o is Npc)))
                        o.Restore(false);
                }
            }

            Server.Instance.FlushOutput();
        }

        [Obfuscation(Exclude = false)]
        public void Update(Npc npc)
        {
            if (!npc.Incapacitated)
            {
                Object target = null;
                if (!this.participants_.TryGetValue(npc, out target))
                {
                    npc.FightSystem = null;
                    return;
                }

                object[] rvals = this.DoScript(ScriptType.OnNpcUpdate, npc, target);
                if (rvals != null && rvals[0].ToString() == "cancel")
                    return;

                if (npc.CurrentAction == null)
                {
                    if (this.CheckNpcCombo(npc))
                        return;
                    if (npc.AttackDelay.Update())
                        return;
                    if (!this.Attack(npc, target) && !npc.Location.Equals(target.Location))
                    {
                        Stat speedStat = (Server.Instance.Settings.SpeedStat != Guid.Empty ? npc.GetStat(Server.Instance.Settings.SpeedStat) : null) ?? npc.GetStat("speed");
                        npc.ReplaceCurrentAction(new TravelAction(npc, target, (speedStat == null ? 3 : (int)speedStat.Current)));
                    }
                }
            }
        }
        
        public bool Attack(Npc npc, Object target)
        {
            Ability ability = npc.GetRandomAggressiveAbility(target);
            if (ability != null)
            {
                ability.OnPerform(npc, target, true, npc.GetEquippedItem(ability.BodyPart));
                return true;
            }
            return false;
        }

        [Obfuscation(Exclude = false)]
        public void AddParticipant(Object o, Object target)
        {
            lock (this.Participants)
            {
                if (o is Character && !this.Participants.ContainsKey(o))
                {
                    this.Participants.Add(o, target);
                    o.DieEvent += this.RemoveParticipant;
                    o.AbilityPerformedEvent += o_AbilityPerformedEvent;
                    o.FightSystem = this;
                }
            }
            target.FightSystem = this;
        }

        [Obfuscation(Exclude = false)]
        public void AddParticipants(Object o, Object target)
        {
            this.AddParticipant(o, target);
            this.AddParticipant(target, o);
        }
        
        public void RemoveParticipant(Object o)
        {
            if (this.participants_ == null)
            {
                o.DieEvent -= this.RemoveParticipant;
                return;
            }

            this.participants_.Remove(o);
            o.FightSystem = null;
            o.DieEvent -= this.RemoveParticipant;
            o.AbilityPerformedEvent -= this.o_AbilityPerformedEvent;
            if (o is Npc)
                o.Stop(false);

            lock (this.Participants)
            {
                if (this.Participants.Count == 1)
                {
                    using (Dictionary<Object, Object>.Enumerator enumerator = this.Participants.GetEnumerator())
                    {
                        enumerator.MoveNext();
                        this.RemoveParticipant(enumerator.Current.Key);
                    }
                }
            }
        }

        [Obfuscation(Exclude = false)]
        void o_AbilityPerformedEvent(Ability ability, Object actor, Object target, Ability.Status status)
        {
            //Debug.WriteLine("Ability performed: " + status.ToString());
            switch (status)
            {
                case Ability.Status.Successful:
                    if (target != actor)
                        this.participants_[actor] = target;
                    this.CheckCombo(actor, target, ability.Id);
                    break;
                case Ability.Status.Preparing:
                    if (ability.Style == Ability.Styles.Aggressive && target != null && target is Character)
                    {
                        Character ch = target as Character;
                        if (ch.User != null)
                        {
                            ch.User.SendHelp("You have " + ability.GetPrepareTime(actor as Character) + " seconds to defend!");
                            string str = null;
                            for (int x = 0; x < ability.Defense.Count; ++x)
                            {
                                Ability a = Server.Instance.Get<Ability>(ability.Defense[x]);
                                if (a != null)
                                {
                                    if (!string.IsNullOrEmpty(str))
                                        str += (x == ability.Defense.Count - 1 ? " or " : ", ");
                                    str += a.Name;
                                }
                            }
                            ch.User.SendHelp("Defend using " + str + ".");
                        }
                    }
                    break;
            }
        }

        [Obfuscation(Exclude = false)]
        public object Clone()
        {
            ManualFightSystem newFightSystem = this.MemberwiseClone() as ManualFightSystem;
            newFightSystem.participants_ = new Dictionary<Object, Object>();
            newFightSystem.combos_ = new Dictionary<Object, Queue<char>>();
            newFightSystem.updateTimer_ = new System.Timers.Timer();
            newFightSystem.updateTimer_.Elapsed += new System.Timers.ElapsedEventHandler(newFightSystem.updateTimer__Elapsed);
            newFightSystem.updateTimer_.Interval = 2000;
            newFightSystem.scripts_ = this.scripts_;
            return newFightSystem;
        }

        [Obfuscation(Exclude = false)]
        void DoCombo(Object actor, Object target)
        {
            actor.RemoveAction(typeof(RecoverAction));
            String msg = new String(
                "You are stunned!", "You are stunned!",
                "{1} is stunned!", "{1} is stunned!", 
                "{1} is stunned!", null);
            actor.Area.Send(msg, target, actor, 0);
            target.ReplaceCurrentAction(new RecoverAction(target, 5, false));

            string str = "{color:brightcyan}Combo! {color:brightblue}( {color:brightred}";
            Queue<char> combo = new Queue<char>();

            for (int x = 0; x < this.GetComboLength(actor, target); ++x)
            {
                char c = (char)Server.RandomNumber.Next((int)'a', ((int)'z')+1);
                combo.Enqueue(c);
                str += c + " ";
            }
            str += "{color:brightblue})";
            actor.Send(str);
            if (actor is Character)
            {
                Character ch = actor as Character;
                if (ch.User != null)
                    ch.User.SendHelp("Enter each letter one at a time.");
            }
            Debug.Assert(!this.combos_.ContainsKey(actor), actor.Name + " is already performing a combo.");
            this.combos_.Add(actor, combo);
            this.Participants[actor] = target;
        }

        [Obfuscation(Exclude = false)]
        public bool CheckDefend(Ability ability, Object actor, Object target)
        {
            if (target.Position == Object.Positions.Dead 
                || target.Position == Object.Positions.Recovering 
                || target.Position == Object.Positions.Sleeping
                || target.Position == Object.Positions.Unconscious)
                return false;

            AbilityAction targetsAction = target.CurrentAction as AbilityAction;
            foreach (Guid abilityId in ability.Defense)
            {
                Ability defenseAbility = Server.Instance.Get<Ability>(abilityId);
                if (defenseAbility != null && (!(target is Character) || ((Character)target).Abilities.ContainsKey(defenseAbility.Id)))
                {
                    if (defenseAbility.PrepareTime > 0)
                    {
                        if (target is Npc)
                        {
                            Npc npc = target as Npc;
                            int level;
                            if (npc.Abilities.TryGetValue(defenseAbility.Id, out level))
                                if (Server.RandomNumber.Next(100) <= level)
                                    targetsAction = new AbilityAction(defenseAbility, target, actor, null);
                        }

                        if (targetsAction != null &&
                            targetsAction.Target == actor &&
                            targetsAction.Ability == defenseAbility &&
                            targetsAction.Finish())
                            return true;
                    }
                    else if (defenseAbility.OnPerform(target, actor, false, null))
                        return true;
                }
            }

            return false;
        }

        [Obfuscation(Exclude = false)]
        void CheckCombo(Object actor, Object target, Guid abilityId)
        {
            if (actor is Character && 
                target is Character &&
                ((Character)actor).Abilities.ContainsKey(this.comboAbilityId_) &&
                !this.combos_.ContainsKey(actor) &&
                target.Position != Object.Positions.Dead && 
                target.Location.Equals(actor.Location) &&
                this.ComboTriggers != null &&
                this.ComboTriggers.Contains(abilityId) &&
                this.TriggerCombo(actor, target))
            {
                this.DoCombo(actor, target);
            }
        }

        [Obfuscation(Exclude = false)]
        bool CheckNpcCombo(Npc npc)
        {
            Queue<char> combo = null;
            if (this.combos_.TryGetValue(npc, out combo))
            {
                Object target = this.Participants[npc];
                combo.Dequeue();
                if (this.Participants.ContainsKey(target))
                {
                    Ability ability = Server.Instance.Get<Ability>(this.ComboAbilityId);
                    if (ability != null)
                        ability.Hit(npc, target, npc.GetEquippedItem(ability.BodyPart));
                }

                if (combo.Count < 1)
                    this.combos_.Remove(npc);

                return true;
            }

            return false;
        }

        [Obfuscation(Exclude = false)]
        public bool ConvertAlias(User user)
        {
            return !this.combos_.ContainsKey(user.Character);
        }

        [Obfuscation(Exclude = false)]
        object[] DoScript(ScriptType type, Npc npc, Object target)
        {
            string script;
            if (!this.scripts_.TryGetValue(type, out script) || string.IsNullOrEmpty(script))
                return null;

            try
            {
                lock (this.lua_)
                {
                    string str = script;
                    if (npc != null) this.lua_["npc"] = npc;
                    else str = str.Insert(0, "npc=nil\r\n");
                    if (target != null) this.lua_["target"] = target;
                    else str = str.Insert(0, "target=nil\r\n");
                    this.lua_["fight"] = this;
                    return this.lua_.DoString(str);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR: FightSystem: " + type);
                Debug.WriteLine(script, "Script");
                Debug.WriteLine(e.ToString(), "StackTrace");
            }

            return null;
        }
        
        public void RestoreNpcs()
        {
            foreach (Object o in this.participants_.Keys)
                if (o is Npc)
                    o.Restore(false);
        }
        
        public void RestoreAll()
        {
            foreach (Object o in this.participants_.Keys)
                o.Restore(false);            
        }

        [Obfuscation(Exclude = false)]
        int GetComboLength(Object actor, Object target)
        {
            lock (this.lua_)
            {
                this.lua_["actor"] = actor;
                this.lua_["target"] = target;
                return Convert.ToInt32(this.lua_.DoString(this.ComboLength)[0]);
            }
        }

        [Obfuscation(Exclude = false)]
        bool TriggerCombo(Object actor, Object target)
        {
            lock (this.lua_)
            {
                this.lua_["actor"] = actor;
                this.lua_["target"] = target;
                return Convert.ToBoolean(this.lua_.DoString(this.ComboChance)[0]);
            }
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            if (this.comboTriggers_ == null)
                this.comboTriggers_ = new List<Guid>();
            if (this.scripts_ == null)
                this.scripts_ = new Dictionary<ScriptType, string>();
            if (this.comboChance_ != 0) {
                this.ComboChance = "math.random(100) < " + this.comboChance_;
                this.comboChance_ = 0;
            }
            if (this.comboLength_ != 0)
            {
                this.ComboLength = this.comboLength_.ToString();
                this.comboLength_ = 0;
            }
        }
    }
}
