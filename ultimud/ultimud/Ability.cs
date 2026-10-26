using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Damdai;
using Damdai.TypeConverters;
using LuaInterface;

namespace Ultimud
{
    [Serializable]
    [DefaultProperty("Name")]
    [Obfuscation(Exclude = true)]
    public class Ability : IIdentifiable, ICommand
    {
        #region Enums
        enum ScriptType
        {
            OnSuccess,
            OnPerform,
            OnPrepare,
            OnImprove,
            FailFormula,
            MissFormula,
            LevelUpFormula,
            AdditionalHitsFormula,
        }
        public enum Targets
        {
            Self,
            Others,
            All,
            //None
        }
        public enum Movements
        {
            None,
            KnockBack,
            ToActor,
            ToTarget
        }
        public enum Status
        {
            Preparing,
            Missed,
            Failed,
            Successful
        }
        public enum Stage
        {
            Prepare,
            Perform,
            Success,
            Miss,
            Fail
        }
        public enum Styles
        {
            None,
            Defensive,
            Aggressive
        }
        public enum TargetType
        {
            Player,
            Npc,
            Item,
        }
        #endregion

        #region Members
        Guid id_;
        string name_;
        int range_;
        int area_;
        int prepare_;
        int recover_;
        List<StatModifier> statCost_ = new List<StatModifier>();
        List<StatModifier> targetStatMods_ = new List<StatModifier>();
        String prepareMessage_ = new String();
        String performMessage_ = new String();
        String hitMessage_ = new String();
        String missMessage_ = new String();
        String failMessage_ = new String();
        List<Guid> requirements_ = new List<Guid>();
        List<StatModifier> improvements_ = new List<StatModifier>();
        List<Guid> defense_ = new List<Guid>();
#pragma warning disable 0169, 0414
        bool usesWeapon_; //Deprecated.
        int improvementRate_ = 5; //Deprecated.
#pragma warning restore 0169, 0414
        Targets target_ = Targets.Self;
#pragma warning disable 0169, 0414
        List<StatFactor> successFactors_; //Deprecated.
        List<StatFactor> missFactors_; //Deprecated.
#pragma warning restore 0169, 0414
        Character.BodyParts bodyPart_ = Character.BodyParts.None;
        List<StatusEffect> addTargetStatusEffects_ = new List<StatusEffect>();
        List<StatusEffect> removeTargetStatusEffects_ = new List<StatusEffect>();
#pragma warning disable 0169, 0414
        bool aggressive_; //Deprecated.
        bool defensive_; //Deprecated.
#pragma warning restore 0169, 0414
        List<Guid> raceRestrictions_ = new List<Guid>();
        List<Guid> jobRestrictions_ = new List<Guid>();
        bool canInterrupt_ = true;
        string shortcut_;
        Movements actorMovement_ = Movements.None;
        Movements targetMovement_ = Movements.None;
        Stage applyStatCostStage_ = Stage.Success;
        Guid groupId_;
        int maxLevel_ = 99;
        Styles style_ = Styles.None;
        bool auto_;
        bool limitRangeBySight_ = true;
#pragma warning disable 0169, 0414
        Dictionary<ScriptType, string> scripts_; //Deprecated.
        bool canfail_; //Deprecated.
#pragma warning restore 0169, 0414
        bool disablemenu_;
        BitArray targettypes_ = new BitArray(32);
        uint minrange_;
#pragma warning disable 0169, 0414
        List<string> itemtypes_; //Deprecated.
        string failformula_; //Deprecated.
        string missformula_; //Deprecated.
        string levelupformula_; //Deprecated.
#pragma warning restore 0169, 0414
        Dictionary<ScriptType, Guid> scripts2_ = new Dictionary<ScriptType, Guid>();
        List<Guid> itemtags_ = new List<Guid>();

        [NonSerialized]
        [Obfuscation(Exclude=false)]
        Lua lua_ = new Lua();
        #endregion

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Category("Appearance")]
        [Description("The name of this ability.")]
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = Telnet.Decolorize(value); }
        }

        [Description("The maximum range of this ability. If -1, target can be anywhere in the game.")]
        [DefaultValue(0)]
        public int RangeMax
        {
            get { return this.range_; }
            set { this.range_ = value; }
        }

        [Description("The minimum range of this ability.")]
        [DefaultValue(0)]
        public uint RangeMin
        {
            get { return this.minrange_; }
            set { this.minrange_ = value; }
        }

        [Description("The area effect of this ability from the point of impact.")]
        [DefaultValue(0)]
        public int AreaEffect
        {
            get { return this.area_; }
            set { this.area_ = value; }
        }

        [Description("The time it takes, in seconds, to perform this ability.")]
        [DefaultValue(0)]
        public int PrepareTime
        {
            get { return this.prepare_; }
            set { this.prepare_ = value; }
        }

        [Description("The time it takes, in seconds, to recover after performing this ability.")]
        [DefaultValue(0)]
        public int RecoverTime
        {
            get { return this.recover_; }
            set { this.recover_ = value; }
        }

        [Category("Stat Cost")]
        [Description("The amount to subtract from user's stats when this ability is performed.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> StatCost
        {
            get { return this.statCost_; }
            set { this.statCost_ = value; }
        }

        [Description("The amount to modify target's stats when this ability is successfull.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> TargetStatMods
        {
            get { return this.targetStatMods_; }
            set { this.targetStatMods_ = value; }
        }

        [Category("Requirements")]
        [DisplayName("Abilities")]
        [Description("The abilities that must be known before this one can be learned or used.")]
        [Editor(typeof(Server.MultiObjectSelector<Ability>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public List<Guid> Requirements
        {
            get { return this.requirements_; }
            set { this.requirements_ = value; }
        }

        [Description("Chance of raising user's stats when they improve in this ability.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> Improvements
        {
            get { return this.improvements_; }
            set { this.improvements_ = value; }
        }

        [Category("Requirements")]
        [Description("The tags that must be present on the equipped item in order to use this ability.")]
        [Editor(typeof(Server.MultiObjectSelector<Tag>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Tag>))]
        public List<Guid> ItemTags
        {
            get { return this.itemtags_; }
            set { this.itemtags_ = value; }
        }

        [Category("Output Messages")]
        [DisplayName("Prepare")]
        [Description("The message that is displayed when the user prepares to perform this ability.")]
        public String PrepareMessage
        {
            get { return this.prepareMessage_; }
            set { this.prepareMessage_ = value; }
        }

        [Category("Output Messages")]
        [DisplayName("Perform")]
        [Description("The message that is displayed when the user performs this ability.")]
        public String PerformMessage
        {
            get { return this.performMessage_; }
            set { this.performMessage_ = value; }
        }

        [Category("Output Messages")]
        [DisplayName("Success")]
        [Description("The message that is displayed when this ability is successfully performed.")]
        public String SuccessMessage
        {
            get { return this.hitMessage_; }
            set { this.hitMessage_ = value; }
        }

        [Category("Output Messages")]
        [DisplayName("Miss")]
        [Description("The message that is displayed when this ability misses its intended target.")]
        public String MissMessage
        {
            get { return this.missMessage_; }
            set { this.missMessage_ = value; }
        }

        [Category("Output Messages")]
        [DisplayName("Fail")]
        [Description("The message that is displayed when the user fails to correctly perform this ability.")]
        public String FailMessage
        {
            get { return this.failMessage_; }
            set { this.failMessage_ = value; }
        }

        [Description("The abilities that can be used to defend against this ability.")]
        [Editor(typeof(Server.MultiObjectSelector<Ability>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public List<Guid> Defense
        {
            get { return this.defense_; }
            set { this.defense_ = value; }
        }

        [Description("Whether this ability can be used to target yourself, others, both, or nothing.")]
        [DefaultValue(Targets.Self)]
        public Targets Target
        {
            get { return this.target_; }
            set { this.target_ = value; }
        }

        [Description("The part of the body that this ability is performed with.")]
        [DefaultValue(Character.BodyParts.None)]
        public Character.BodyParts BodyPart
        {
            get { return this.bodyPart_; }
            set { this.bodyPart_ = value; }
        }

        [Category("Status Effects")]
        [DisplayName("AddToTarget")]
        [Description("The status effects that this ability can add to target.")]
        [TypeConverter(typeof(ListConverter<StatusEffect>))]
        public List<StatusEffect> AddTargetStatusEffects
        {
            get { return this.addTargetStatusEffects_; }
            set { this.addTargetStatusEffects_ = value; }
        }

        [Category("Status Effects")]
        [DisplayName("RemoveFromTarget")]
        [Description("The status effects that this ability can remove from target.")]
        [TypeConverter(typeof(ListConverter<StatusEffect>))]
        public List<StatusEffect> RemoveTargetStatusEffects
        {
            get { return this.removeTargetStatusEffects_; }
            set { this.removeTargetStatusEffects_ = value; }
        }

        [DefaultValue(Styles.None)]
        public Styles Style
        {
            get { return this.style_; }
            set { this.style_ = value; }
        }

        [Description("Can this ability be interrupted once started?")]
        [DefaultValue(true)]
        [DisplayName("IsInterruptable")]
        public bool CanInterrupt
        {
            get { return this.canInterrupt_; }
            set { this.canInterrupt_ = value; }
        }

        [Category("Requirements")]
        [DisplayName("Race")]
        [Description("Which races can use this ability? If blank, all races will be able to use it.")]
        [Editor(typeof(Server.MultiObjectSelector<Race>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Race>))]
        public List<Guid> RaceRestrictions
        {
            get { return this.raceRestrictions_; }
            set { this.raceRestrictions_ = value; }
        }

        [Category("Requirements")]
        [DisplayName("Job")]
        [Description("Which jobs can use this ability? If blank, all jobs will be able to use it.")]
        [Editor(typeof(Server.MultiObjectSelector<Job>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Job>))]
        public List<Guid> JobRestrictions
        {
            get { return this.jobRestrictions_; }
            set { this.jobRestrictions_ = value; }
        }

        [Description("The command shortcut for this ability. All abilities can be performed by entering the ability name. If this property is set, the ability can also be performed by entering the shortcut.")]
        public string Shortcut
        {
            get { return this.shortcut_; }
            set { this.shortcut_ = value; }
        }

        [Description("Where to move actor if this ability is successful.")]
        [DefaultValue(Movements.None)]
        public Movements ActorMovement
        {
            get { return this.actorMovement_; }
            set { this.actorMovement_ = value; }
        }

        [Description("Where to move target if this ability is successful.")]
        [DefaultValue(Movements.None)]
        public Movements TargetMovement
        {
            get { return this.targetMovement_; }
            set { this.targetMovement_ = value; }
        }

        [Category("Stat Cost")]
        [DisplayName("ApplyOn")]
        [Description("At what point in this ability should the StatCost be applied?")]
        [DefaultValue(Stage.Success)]
        public Stage ApplyStatCostStage
        {
            get { return this.applyStatCostStage_; }
            set { this.applyStatCostStage_ = value; }
        }

        [Category("Appearance")]
        [DisplayName("Group")]
        [Description("The ability group that this ability belongs to.")]
        [Editor(typeof(Server.SingleObjectSelector<AbilityGroup>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<AbilityGroup>))]
        public Guid GroupId
        {
            get { return this.groupId_; }
            set { this.groupId_ = value; }
        }

        [Description("The maximum attainable level for this ability.")]
        [DefaultValue(99)]
        public int MaxLevel
        {
            get { return this.maxLevel_; }
            set { this.maxLevel_ = value; }
        }

        [Description("Is this an automated ability that players can't use directly?")]
        [DefaultValue(false)]
        public bool Auto
        {
            get { return this.auto_; }
            set { this.auto_ = value; }
        }

        [Description("If true, the range of this ability will be limited by a player's sight stat.")]
        [DefaultValue(true)]
        public bool LimitRangeBySight
        {
            get { return this.limitRangeBySight_; }
            set { this.limitRangeBySight_ = value; }
        }

        [Description("This script is executed when this ability is successful.\r\n\r\nObjects: actor, target, item, ability\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        public Guid OnSuccess
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.OnSuccess, out id))
                    this.scripts2_.Add(ScriptType.OnSuccess, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.OnSuccess))
                    this.scripts2_[ScriptType.OnSuccess] = value;
                else
                    this.scripts2_.Add(ScriptType.OnSuccess, value);
            }
        }

        [Description("Objects: actor, target, item, ability\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        public Guid OnPrepare
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.OnPrepare, out id))
                    this.scripts2_.Add(ScriptType.OnPrepare, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.OnPrepare))
                    this.scripts2_[ScriptType.OnPrepare] = value;
                else
                    this.scripts2_.Add(ScriptType.OnPrepare, value);
            }
        }

        [Description("This script is executed when this ability is performed.\r\n\r\nObjects: actor, target, item, ability\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        [DisplayName("OnPerform")]
        public Guid OnPerformScript
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.OnPerform, out id))
                    this.scripts2_.Add(ScriptType.OnPerform, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.OnPerform))
                    this.scripts2_[ScriptType.OnPerform] = value;
                else
                    this.scripts2_.Add(ScriptType.OnPerform, value);
            }
        }

        [Description("This script is executed when this ability is improved.\r\n\r\nObjects: actor, target, item, ability\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        public Guid OnImprove
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.OnImprove, out id))
                    this.scripts2_.Add(ScriptType.OnImprove, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.OnImprove))
                    this.scripts2_[ScriptType.OnImprove] = value;
                else
                    this.scripts2_.Add(ScriptType.OnImprove, value);
            }
        }

        [Description("The formula that is used to calculate whether or not this ability fails.\r\n\r\nObjects: actor, target, item, ability\r\nReturns: bool\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        public Guid FailFormula
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.FailFormula, out id))
                    this.scripts2_.Add(ScriptType.FailFormula, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.FailFormula))
                    this.scripts2_[ScriptType.FailFormula] = value;
                else
                    this.scripts2_.Add(ScriptType.FailFormula, value);
            }
        }
        [Description("The formula that is used to calculate whether or not this ability misses.\r\n\r\nObjects: actor, target, item, ability\r\nReturns: bool\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        public Guid MissFormula
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.MissFormula, out id))
                    this.scripts2_.Add(ScriptType.MissFormula, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.MissFormula))
                    this.scripts2_[ScriptType.MissFormula] = value;
                else
                    this.scripts2_.Add(ScriptType.MissFormula, value);
            }
        }

        [Description("The formula that is used to calculate the chance of leveling-up this ability.\r\n\r\nObjects: actor, target, item, ability\r\nReturns: bool\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        public Guid LevelUpFormula
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.LevelUpFormula, out id))
                    this.scripts2_.Add(ScriptType.LevelUpFormula, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.LevelUpFormula))
                    this.scripts2_[ScriptType.LevelUpFormula] = value;
                else
                    this.scripts2_.Add(ScriptType.LevelUpFormula, value);
            }
        }

        [Description("The formula that is used to calculate how many times this ability hits when successful.\r\n\r\nObjects: actor, target, item, ability\r\nReturns: int\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.AbilityScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        [Category("Scripts")]
        public Guid AdditionalHitsFormula
        {
            get
            {
                Guid id;
                if (!this.scripts2_.TryGetValue(ScriptType.AdditionalHitsFormula, out id))
                    this.scripts2_.Add(ScriptType.AdditionalHitsFormula, Guid.Empty);
                return id;
            }
            set
            {
                if (this.scripts2_.ContainsKey(ScriptType.AdditionalHitsFormula))
                    this.scripts2_[ScriptType.AdditionalHitsFormula] = value;
                else
                    this.scripts2_.Add(ScriptType.AdditionalHitsFormula, value);
            }
        }

        [DefaultValue(false)]
        [Description("Overrides the menu system, requiring the entire command to be entered at once.")]
        public bool DisableMenu
        {
            get { return this.disablemenu_; }
            set { this.disablemenu_ = value; }
        }

        [Description("The types of objects that can be targeted with this ability.")]
        [Editor(typeof(FlagEditor<TargetType>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<TargetType>))]
        public BitArray TargetTypes
        {
            get { return this.targettypes_; }
            set { this.targettypes_ = value; }
        }
        #endregion

        #region Methods
        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Ability)))
                Server.Instance.DataSets.Add(typeof(Ability), new Dictionary<Guid, IIdentifiable>());
        }

        [Obfuscation(Exclude = false)]
        public ICommand Get(string str)
        {
            Dictionary<Guid, IIdentifiable> dataset = Server.Instance.DataSets[typeof(Ability)];
            lock (dataset)
            {
                foreach (Ability a in dataset.Values)
                    if (!string.IsNullOrEmpty(a.Name) && (a.Name.StartsWith(str, StringComparison.CurrentCultureIgnoreCase) || (a.Shortcut != null && a.Shortcut.StartsWith(str, StringComparison.CurrentCultureIgnoreCase))))
                        return a as ICommand;
            }
            return null;
        }
        
        [Obfuscation(Exclude = false)]
        public bool ProcessCommand(User user)
        {
            if (user.Character.Incapacitated)
            {
                user.Send("You can't use any abilities in your current state.");
                return true;
            }
            if (this.auto_)
            {
                user.Send("That is an automatic ability that can't be used directly.");
                return true;
            }
            if (!user.Character.CanUse(this, true))
            {
                user.Send("You can't use that ability.");
                return true;
            }
            if (!user.Character.Abilities.ContainsKey(this.Id))
            {
                user.Send("You don't know that ability.");
                return true;
            }

            user.InputHandler = this.ProcessCommand;

            switch (this.Target)
            {
                case Targets.Self:
                    this.OnPerform(user.Character, user.Character, true, user.Character.GetEquippedItem(this.BodyPart));
                    break;
                case Targets.Others:
                case Targets.All:
                    if ((this.style_ == Styles.Defensive || this.style_ == Styles.Aggressive) && string.IsNullOrEmpty(user.Command) && user.Character.FightSystem != null)
                    {
                        Object target = user.Character.FightSystem.Participants[user.Character];
                        if (target == null)
                            user.Character.Send("Invalid target.");
                        else
                            this.OnPerform(user.Character, target, true, user.Character.GetEquippedItem(this.BodyPart));
                        return true;
                    }
                    else
                    {
                        Object target = null;
                        if (this.disablemenu_)
                            target = this.FindTarget(user.GetArgument(), user);
                        else if (Command.GetArguments(user, this.SelectTarget))
                            target = user.GetCommandArg<Object>(0);
                        else
                            return false;

                        Stat sight = user.Character.GetStat("sight");
                        if (target == null)
                            user.Send("Invalid target.");
                        else
                            this.OnPerform(user.Character, target, true, user.Character.GetEquippedItem(this.BodyPart));

                        return true;
                    }
            }

            return true;
        }

        [Obfuscation(Exclude = false)]
        bool SelectTarget(User user)
        {
            string arg = null;
            if (user.CurrentArg != null)
                user.CurrentCommandArgs.Add(user.GetCurrentArg());
            else if (!string.IsNullOrEmpty((arg = user.GetArgument())))
                user.CurrentCommandArgs.Add(this.FindTarget(arg, user));
            else
            {
                Menu menu = new Menu(this.SelectTarget, null);

                if (this.Target == Targets.Self || this.Target == Targets.All)
                    menu.AddOption(user.Character.Name);

                if (this.Target == Targets.All || this.Target == Targets.Others)
                {
                    List<Object> targets = this.GetTargetSelection(user);
                    foreach (Object o in targets)
                    {
                        if (this.Style == Styles.Aggressive && o.ObjectFlags.Get((int)Object.ObjFlags.Unattackable))
                            continue;
                        menu.AddOption(new Menu.Option(o.Name, o));
                    }
                }

                if (menu.Options == null)
                {
                    user.Send("There are no targets here.");
                    throw new AbortCommandException();
                }

                user.Menus.Enqueue(menu);
                return false;
            }
            return true;
        }

        [Obfuscation(Exclude = false)]
        List<Object> GetTargetSelection(User user)
        {
            List<Object> targets = new List<Object>();
            targets.Add(user.Character);
            int[] range = this.GetRange(user.Character);
            bool found = false;
            if (this.targettypes_.Get((int)TargetType.Player))
            {
                targets.AddRange(user.Character.NearbyObjects<Player>(range, this.limitRangeBySight_, targets.ToArray()).ConvertAll<Object>(delegate(Player o) { return o as Object; }));
                found = true;
            }
            if (this.targettypes_.Get((int)TargetType.Item))
            {
                targets.AddRange(user.Character.NearbyObjects<Item>(range, this.limitRangeBySight_, targets.ToArray()).ConvertAll<Object>(delegate(Item o) { return o as Object; }));
                found = true;
            }
            if (this.targettypes_.Get((int)TargetType.Npc))
            {
                targets.AddRange(user.Character.NearbyObjects<Npc>(range, this.limitRangeBySight_, targets.ToArray()).ConvertAll<Object>(delegate(Npc o) { return o as Object; }));
                found = true;
            }

            if (!found)
                targets.AddRange(user.Character.NearbyObjects<Object>(range, this.limitRangeBySight_, targets.ToArray()));

            targets.Remove(user.Character);
            return targets;
        }
        
        [Obfuscation(Exclude = false)]
        Object FindTarget(string name, User user)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            Object target = null;
            int[] range = this.GetRange(user.Character);
            bool found = false;
            if (range[1] == -1)
            {
                if (this.targettypes_.Get((int)TargetType.Player))
                {
                    target = Server.Instance.Find<Player>(name);
                    found = true;
                }
                if (target == null && this.targettypes_.Get((int)TargetType.Item))
                {
                    target = Server.Instance.Find<Item>(name);
                    found = true;
                }
                if (target == null && this.targettypes_.Get((int)TargetType.Npc))
                {
                    target = Server.Instance.Find<Npc>(name);
                    found = true;
                }
                if (!found)
                    target = Server.Instance.Find<Object>(name);
            }
            else
            {
                if (this.targettypes_.Get((int)TargetType.Player))
                {
                    target = user.Character.FindNearby<Character>(name, range, this.limitRangeBySight_);
                    found = true;
                }
                if (target == null && this.targettypes_.Get((int)TargetType.Item))
                {
                    target = user.Character.FindNearby<Item>(name, range, this.limitRangeBySight_);
                    found = true;
                }
                if (target == null && this.targettypes_.Get((int)TargetType.Npc))
                {
                    target = user.Character.FindNearby<Npc>(name, range, this.limitRangeBySight_);
                    found = true;
                }
                if (!found)
                    target = user.Character.FindNearby<Object>(name, range, this.limitRangeBySight_);
            }

            return target;
        }
        
        [Obfuscation(Exclude = false)]
        public bool CanPerform(Object actor, Object target, Ability ability, bool quiet)
        {
            if ((actor != null && actor.Area == null) || (target != null && target.Area == null))
                return false;

            string errorMsg = null;

            if(/*!this.CheckDefendable(actor, target, quiet)
                ||*/ (actor.FightSystem != null && !actor.FightSystem.CanUseAbility(actor, this))
                || !this.CheckStatCost(actor, target, ability, quiet))
            {
                return false;
            }

            Area.Layer actorLayer = actor.Area.GetLayer<Area.Layer>(actor.Location.Point, false);
            Area.Layer targetLayer = (target == null ? null : target.Area.GetLayer<Area.Layer>(target.Location.Point, false));
            Character chActor = actor as Character;
            Item item = null;
            if (chActor != null && !chActor.CanUse(this, true))
                errorMsg = "You can't use that ability.";
            else if (this.ItemTags.Count > 0 && chActor != null && ((item = chActor.GetEquippedItem(this.BodyPart)) == null || item.Tags.Count == 0 || !item.Tags.Exists(delegate(Guid tagid) { return this.ItemTags.Contains(tagid); })))
                errorMsg = "You need a certain type of item to perform that ability.";
            else if (this.Target == Targets.Self && actor != target)
                errorMsg = "You can only perform that ability on yourself.";
            else if (this.Target == Targets.Others && actor == target)
                errorMsg = "You can't perform that ability on yourself.";
            else if (!this.CheckRange(actor, target))
                errorMsg = "Target is out of range.";
            else if (this.Style == Styles.Aggressive)
            {
                if (!(target is Npc) && target is Character && !Server.Instance.Settings.PvpSettings.Allowed)
                    errorMsg = "Player vs. player combat is not allowed here.";
                else if (actor.Area.AreaFlags.Get((int)Area.AreaFlag.NoAttack) || actorLayer != null && actorLayer.LayerFlags.Get((int)Area.Layer.LayerFlag.NoAttack))
                    errorMsg = "You can't attack here.";
                else if (target != null && targetLayer != null && targetLayer.LayerFlags.Get((int)Area.Layer.LayerFlag.NoAttack))
                    errorMsg = "You can't attack that target at its current location.";
                else if (target != null && target.ObjectFlags.Get((int)Object.ObjFlags.Unattackable))
                    errorMsg = "You can't attack that target.";
                else if (target != null && target is Character)
                {
                    Character ch = target as Character;
                    if (ch.SafeTimer != null && ch.SafeTimer.Update())
                        errorMsg = "You can't attack them for another " + ch.SafeTimer.Length + ".";
                }
            }

            if (errorMsg != null)
            {
                if (!quiet) actor.Send(errorMsg);
                return false;
            }
            else
                return true;
        }
        
        [Obfuscation(Exclude = false)]
        public bool CheckStatCost(Object actor, Object target, Ability ability, bool quiet)
        {
            if (this.StatCost != null)
            {
                foreach (StatModifier statMod in Damdai.Collections.Util.CopyList<StatModifier>(this.StatCost))
                {
                    Stat stat = actor.GetStat(statMod.StatId);
                    if (stat == null)
                    {
                        if (!quiet) actor.Send("You can't perform this ability.");
                        return false;
                    }
                    else if ((stat.Current + statMod.Calculate(actor, target, ability)) < 0)
                    {
                        if (!quiet) actor.Send("You don't have enough " + stat.Name + " to perform this ability.");
                        return false;
                    }
                }
            }
            return true;
        }
        
        [Obfuscation(Exclude = false)]
        public bool CheckFail(Object actor, Object target, Item item)
        {
            object[] rvals = this.DoScript(ScriptType.FailFormula, actor, ref target, item);
            if (rvals != null && (bool)rvals[0])
            {
                actor.Area.Send(this.FailMessage, actor, target, 0, new Function<bool, IObject>[] { Object.CanHear },
                    new Telnet.Color[] {
                        Server.Instance.Settings.Colors[Settings.Text.AbilityFail1],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityFail2],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityFail3] });
                if (this.ApplyStatCostStage == Ability.Stage.Fail)
                    this.ApplyStatCost(actor, actor, target, item);
                return true;
            }
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public bool CheckMiss(Object actor, Object target, Item item)
        {
            object[] rvals = this.DoScript(ScriptType.MissFormula, actor, ref target, item);
            if (rvals != null && (bool)rvals[0])
            {
                actor.Area.Send(this.MissMessage, actor, target, 0, new Function<bool, IObject>[] { Object.CanHear },
                    new Telnet.Color[] {
                        Server.Instance.Settings.Colors[Settings.Text.AbilityMiss1],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityMiss2],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityMiss3] });
                if (this.ApplyStatCostStage == Ability.Stage.Miss)
                    this.ApplyStatCost(actor, actor, target, item);
                return true;
            }
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public bool CheckDefend(Object actor, Object target)
        {
            if (target != null && this.Defense.Count > 0 && actor.FightSystem != null)
                return actor.FightSystem.CheckDefend(this, actor, target);
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public void CheckImprove(Character actor, Object target, Item item)
        {
            if (actor == null || !actor.Abilities.ContainsKey(this.Id) || this.LevelUpFormula == Guid.Empty)
                return;
            int level = actor.Abilities[this.Id];
            if (level >= this.MaxLevel)
                return;

            object[] rvals = this.DoScript(ScriptType.LevelUpFormula, actor, ref target, item);
            if (rvals != null && (bool)rvals[0])
            {
                this.DoScript(ScriptType.OnImprove, actor, ref target, item);
                actor.Abilities[this.Id] = ++level;
                actor.Send(Server.Instance.Settings.GetColorCode(Settings.Text.AbilityImprovements) + "Your " + this.Name + " ability has improved!");
                StatModifier.Apply(this.Improvements, actor, actor, target, this, false);
            }
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return base.ToString();
        }
        
        [Obfuscation(Exclude = false)]
        public void ApplyTargetStatusEffects(Object target, Object actor)
        {
            foreach (StatusEffect status in this.RemoveTargetStatusEffects)
                target.RemoveStatusEffect(status.BlueprintId);
            foreach (StatusEffect status in this.AddTargetStatusEffects)
                status.Apply(target, actor);
        }
        
        [Obfuscation(Exclude = false)]
        public Dictionary<Stat, long> ApplyStatMods(Object applyto, Object actor, Object target, Item item)
        {
            Dictionary<Stat, long> modifiedStats = new Dictionary<Stat, long>();
            if (applyto == null)
                return modifiedStats;

            List<StatModifier> statMods = new List<StatModifier>(this.TargetStatMods);
            if (item != null)
                statMods.AddRange(item.TargetStatMods);

            foreach (StatModifier statMod in statMods)
            {
                Stat stat = applyto.GetStat(statMod.StatId);
                if (stat != null)
                {
                    long change = statMod.Apply(applyto, actor, target, this, true);
                    if (!modifiedStats.ContainsKey(stat))
                        modifiedStats.Add(stat, change);
                    else
                        modifiedStats[stat] += change;
                }
            }

            return modifiedStats;
        }
        
        [Obfuscation(Exclude = false)]
        bool CheckDefendable(Object actor, Object target, bool quiet)
        {
            string msg = null;
            if (this.Style == Styles.Defensive)
            {
                if (target == null)
                {
                    Debug.WriteLine("Target is null.", "WARNING");
                    return false;
                }

                if (target.CurrentAction == null && !this.auto_)
                    msg = target.Name + " isn't attacking you.";
                else if (target.CurrentAction is AbilityAction)
                {
                    AbilityAction abilityAction = target.CurrentAction as AbilityAction;
                    if (!abilityAction.Ability.Defense.Contains(this.Id))
                        msg = "You can't defend " + target.Name + "'s attack using this ability.";
                }

                if (msg != null)
                {
                    if(!quiet)
                        actor.Send(String.Capitalize(msg));
                    return false;
                }
            }
            return true;
        }
        
        [Obfuscation(Exclude = false)]
        public void ApplyMovements(Object actor, Object target)
        {
            switch (this.ActorMovement)
            {
                case Movements.KnockBack:
                    Area.Direction dir = actor.GetDirection(target.Location.Point);
                    actor.Move(Area.ReverseDirection(dir),
                        new String("You are knocked back!", "You are knocked back!", "{1} is knocked back!", "{1} is knocked back!", "{1} is knocked back!", null),
                        new String(null, null, "{1} slides by from the {dir}!", null, "{1} slides by from the {dir}!", null), true);
                    break;
                case Movements.ToTarget:
                    if (target != null)
                    {
                        actor.Location = new Location(target.Area, (target is Area.Layer ?
                            ((Area.Layer)target).GetNearestPoint(actor.Location.Point) : target.Location.Point));
                    }
                    break;
            }

            if (!target.ObjectFlags.Get((int)Object.ObjFlags.Immovable))
            {
                switch (this.TargetMovement)
                {
                    case Movements.KnockBack:
                        Area.Direction dir = target.GetDirection(actor.Location.Point);
                        target.Move(Area.ReverseDirection(dir),
                            new String("You are knocked back!", "You are knocked back!", "{1} is knocked back!", "{1} is knocked back!", "{1} is knocked back!", null),
                            new String(null, null, "{1} flies by from the {dir}, falling at your feet!", null, "{1} flies by from the {dir}, falling at your feet!", null), true);
                        break;
                    case Movements.ToActor:
                        if (actor != null)
                            target.Location = actor.Location;
                        break;
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void ApplyStatCost(Object applyto, Object actor, Object target, Item item)
        {
            StatModifier.Apply(this.StatCost, applyto, actor, target, this, true);
            if (item != null)
                StatModifier.Apply(item.AbilityStatMods, applyto, actor, target, this, true);
        }
        
        public void DamageEquipment(Object actor, Character target, Item item)
        {
            if (this.Style == Styles.Aggressive)
            {
                foreach (Item eq in target.Equipment.Values)
                {
                    if (Server.RandomNumber.Next(100) < 25 && this.TargetStatMods.Exists(delegate(StatModifier sm) { return eq.Stats.ContainsKey(sm.StatId); }))
                    {
                        this.Hit(actor, eq, item);
                        return;
                    }
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public bool OnPerform(Object actor, Object target, bool prepare, Item item)
        {
            if (!this.CanPerform(actor, target, this, false))
                return false;

            if (this.Style == Styles.Aggressive && actor is Character)
                ((Character)actor).SafeTimer = null;

            bool hit = false;

            if (prepare && this.GetPrepareTime(item) > 0)
            {
                if (actor.CurrentAction != null && actor.CurrentAction is AbilityAction && !((AbilityAction)actor.CurrentAction).Ability.CanInterrupt)
                {
                    actor.Send("You can't interrupt your current action.");
                    return false;
                }
                object[] rvals = this.DoScript(ScriptType.OnPrepare, actor, ref target, item);
                if (rvals != null && (string)rvals[0] == "cancel") return false;
                actor.ReplaceCurrentAction(new AbilityAction(this, actor, target, item));
                actor.AbilityPerformed(this, target, Status.Preparing);
                if (this.ApplyStatCostStage == Stage.Prepare)
                    this.ApplyStatCost(actor, actor, target, item);
                return false;
            }
            else
            {
                if(actor is Npc) ((Npc)actor).AttackDelay.Reset();
                object[] rvals = this.DoScript(ScriptType.OnPerform, actor, ref target, item);
                if (rvals != null && (string)rvals[0] == "cancel") return false;
                
                this.UpdateFightSystem(actor, target);
                String performmsg = this.PerformMessage;
                if (item != null)
                    performmsg = performmsg.Replace("{item}", item.Name);
                actor.Area.Send(performmsg, actor, target, 0, new Function<bool, IObject>[] { Object.CanHear },
                    new Telnet.Color[] {
                        Server.Instance.Settings.Colors[Settings.Text.AbilityPerform1],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityPerform2],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityPerform3] });

                if (!this.HandleDefensiveAbility(actor, target, item))
                    hit = this.TryHit(actor, target, item);

                if (this.Style != Styles.Defensive || !hit)
                {
                    int recoverTime = this.GetRecoverTime(item);
                    if (recoverTime > 0 && actor.Position > Object.Positions.Dead && !(actor.CurrentAction is RecoverAction))
                        actor.EnqueueAction(new RecoverAction(actor, recoverTime, false));
                }

                if(hit)
                    this.DoScript(ScriptType.OnSuccess, actor, ref target, item);
            }

            if (this.ApplyStatCostStage == Stage.Perform)
                this.ApplyStatCost(actor, actor, target, item);

            actor.AbilityPerformed(this, target, (hit ? Status.Successful : Status.Failed));
            return hit;
        }
        
        [Obfuscation(Exclude = false)]
        bool TryHit(Object actor, Object target, Item item)
        {
            if (!this.CheckFail(actor, target, item) && !this.CheckMiss(actor, target, item) && !this.CheckDefend(actor, target))
            {
                int hits = 1;
                object[] rvals = this.DoScript(ScriptType.AdditionalHitsFormula, actor, ref target, item);
                if (rvals != null)
                    hits += Convert.ToInt32(rvals[0]);
                for (int x = 0; x < hits; ++x)
                    this.Hit(actor, target, item);
                if (this.ApplyStatCostStage == Stage.Success)
                    this.ApplyStatCost(actor, actor, target, item);

                return true;
            }
            return false;
        }
        
        public void Hit(Object actor, Object target, Item item)
        {
            if (target == null)
                return;

            Dictionary<Stat, long> modifiedStats = this.ApplyStatMods(target, actor, target, item);
            string message = string.Empty;
            if (modifiedStats.Count > 0)
            {
                foreach (KeyValuePair<Stat, long> pair in modifiedStats)
                {
                    if (pair.Value != 0)
                    {
                        message += (pair.Value < 0 ? Telnet.GetColorCode(Server.Instance.Settings.Colors[Settings.Text.StatDecrease])
                            : pair.Value > 0 ? Telnet.GetColorCode(Server.Instance.Settings.Colors[Settings.Text.StatIncrease])
                            : "{color:gray}") + pair.Key.Name + " " + pair.Value.ToString(Server.Instance.Settings.NumberFormat) + "{color:reset}" + ", ";
                    }
                }
                if (!string.IsNullOrEmpty(message))
                    message =  Telnet.ColorCode.Reset + " [" + message.TrimEnd(',', ' ') + "]";
            }

            actor.Area.Send(this.SuccessMessage, actor, target, 0, new Function<bool, IObject>[] { Object.CanHear }, true, 
                new Telnet.Color[] {
                    Server.Instance.Settings.Colors[Settings.Text.AbilitySuccess1], 
                    Server.Instance.Settings.Colors[Settings.Text.AbilitySuccess2],
                    Server.Instance.Settings.Colors[Settings.Text.AbilitySuccess3] }, message);

            if (this.Style == Styles.Aggressive && target is Character)
                this.DamageEquipment(actor, target as Character, item);
            if (item != null)
                item.ApplyTargetStatusEffects(target);
            if (target.Position == Object.Positions.Sleeping)
                target.Position = Object.Positions.Stopped;

            this.ApplyTargetStatusEffects(target, actor);
            this.ApplyMovements(actor, target);
            this.CheckImprove(actor as Character, target, item);

            foreach (Stat s in modifiedStats.Keys)
                s.CheckTriggers(target, actor);
        }
        
        [Obfuscation(Exclude = false)]
        void UpdateFightSystem(Object actor, Object target)
        {
            if (target != null && actor != target)
            {
                switch (this.Style)
                {
                    case Styles.Aggressive:
                        actor.MergeFightSystems(target);
                        actor.LastAggressiveAct = target.LastAggressiveAct = DateTime.Now;
                        break;
                    case Styles.Defensive:
                        actor.LastAggressiveAct = target.LastAggressiveAct = DateTime.Now;
                        break;
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        bool HandleDefensiveAbility(Object actor, Object target, Item item)
        {
            if (this.Style == Styles.Defensive)
            {
                if (target == null || target.CurrentAction == null || !(target.CurrentAction is AbilityAction) || !((AbilityAction)target.CurrentAction).Executing)
                {
                    actor.Area.Send(this.MissMessage, actor, target, 0, new Function<bool, IObject>[] { Object.CanHear },
                        new Telnet.Color[] {
                            Server.Instance.Settings.Colors[Settings.Text.AbilityMiss1],
                            Server.Instance.Settings.Colors[Settings.Text.AbilityMiss2],
                            Server.Instance.Settings.Colors[Settings.Text.AbilityMiss3] });
                    if (this.ApplyStatCostStage == Stage.Miss)
                        this.ApplyStatCost(actor, actor, target, item);

                    return true;
                }
            }
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        object[] DoScript(ScriptType type, Object actor, ref Object target, Item item)
        {
            Guid scriptid;
            if (!this.scripts2_.TryGetValue(type, out scriptid) || scriptid == Guid.Empty)
                return null;

            Script script = Server.Instance.Get<Script>(scriptid);
            if (script == null)
            {
                Trace.WriteLine("Removing invalid script: " + type, "Ability");
                this.scripts2_[type] = Guid.Empty;
                return null;
            }

            try
            {
                lock (this.lua_)
                {
                    if (string.IsNullOrEmpty(script.Code))
                        return null;
                    string str = script.Code;
                    this.lua_["actor"] = actor;
                    this.lua_["target"] = target;
                    if (item != null) this.lua_["item"] = item;
                    else str = str.Insert(0, "item=nil\r\n");
                    this.lua_["ability"] = this;
                    return this.lua_.DoString(str);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR: Ability: " + type + " (" + this.Name + ")");
                Debug.WriteLine(script.Code, "Script");
                Debug.WriteLine(e.ToString(), "StackTrace");
#if DEBUG
                actor.Send(e.Message);
#endif
            }
            return null;
        }
        
        public bool CheckRange(IObject actor, IObject target)
        {
            if (actor == null || target == null)
                return true;

            int[] range = this.GetRange(actor);
            if (range[1] == -1)
                return true;
            if (actor.Parent != target.Parent)
                return false;
            int distance = actor.Distance(target);
            return (range[0] <= distance && distance <= range[1]);
        }        
        public int[] GetRange(IObject actor)
        {
            int maxrange = this.range_;
            uint minrange = this.minrange_;
            if (actor is Character)
            {
                Character ch = actor as Character;
                Item item = ch.GetEquippedItem(this.bodyPart_);
                if (item != null)
                {
                    maxrange = item.RangeMax;
                    minrange = item.RangeMin;
                }
            }

            int[] range = new int[2] { (int)minrange, maxrange };
            if (this.LimitRangeBySight)
            {
                Stat sight = actor.GetStat("sight");
                if (sight != null)
                    range[1] = System.Math.Min(maxrange, (int)sight.Current);
            }
            return range;
        }
        public int GetPrepareTime(Character ch)
        {
            return this.GetPrepareTime((ch == null ? null : ch.GetEquippedItem(this.bodyPart_)));
        }
        public int GetPrepareTime(Item item)
        {
            if (this.bodyPart_ != Character.BodyParts.None && item != null)
                return (this.prepare_ + item.PrepareTime);
            return this.prepare_;
        }
        public int GetRecoverTime(Character ch)
        {
            return this.GetRecoverTime((ch == null ? null : ch.GetEquippedItem(this.bodyPart_)));
        }
        public int GetRecoverTime(Item item)
        {
            if (this.bodyPart_ != Character.BodyParts.None && item != null)
                return (this.recover_ + item.RecoverTime);

            return this.recover_;
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        void OnDeserialized(StreamingContext context)
        {
            if (this.aggressive_)
            {
                this.Style = Styles.Aggressive;
                this.aggressive_ = false;
            }
            else if (this.defensive_)
            {
                this.Style = Styles.Defensive;
                this.defensive_ = false;
            }

            if (this.scripts2_ == null) this.scripts2_ = new Dictionary<ScriptType, Guid>();
            if (this.targettypes_ == null) this.targettypes_ = new BitArray(32);
            if (this.addTargetStatusEffects_ == null) this.addTargetStatusEffects_ = new List<StatusEffect>();
            if (this.removeTargetStatusEffects_ == null) this.removeTargetStatusEffects_ = new List<StatusEffect>();
            if (this.improvementRate_ > 0)
            {
                this.levelupformula_ = "return math.random(100) < " + this.improvementRate_;
                this.improvementRate_ = 0;
            }
            this.missFactors_ = this.successFactors_ = null;
            this.itemtypes_ = null;
            this.canfail_ = false;
            this.lua_ = new Lua();
            if (this.itemtags_ == null)
                this.itemtags_ = new List<Guid>();

            if (!string.IsNullOrEmpty(this.levelupformula_))
            {
                Server.Instance.Loaded += delegate()
                {
                    Guid scriptid = Guid.Empty;
                    foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                    {
                        if (s.Code == this.levelupformula_)
                        {
                            scriptid = s.Id;
                            break;
                        }
                    }

                    if (scriptid == Guid.Empty)
                    {
                        Script script = new Script(Script.Types.Ability, "LevelUpFormula_" + this.name_, this.levelupformula_);
                        Server.Instance.Add<Script>(script);
                        scriptid = script.Id;
                    }

                    if (this.scripts2_.ContainsKey(ScriptType.LevelUpFormula))
                        this.scripts2_[ScriptType.LevelUpFormula] = scriptid;
                    else
                        this.scripts2_.Add(ScriptType.LevelUpFormula, scriptid);
                };
            }
            if (!string.IsNullOrEmpty(this.missformula_))
            {
                Server.Instance.Loaded += delegate()
                {
                    Guid scriptid = Guid.Empty;
                    foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                    {
                        if (s.Code == this.missformula_)
                        {
                            scriptid = s.Id;
                            break;
                        }
                    }

                    if (scriptid == Guid.Empty)
                    {
                        Script script = new Script(Script.Types.Ability, "MissFormula_" + this.name_, this.missformula_);
                        Server.Instance.Add<Script>(script);
                        scriptid = script.Id;
                    }
                    if (this.scripts2_.ContainsKey(ScriptType.MissFormula))
                        this.scripts2_[ScriptType.MissFormula] = scriptid;
                    else
                        this.scripts2_.Add(ScriptType.MissFormula, scriptid);
                };
            }
            if (!string.IsNullOrEmpty(this.failformula_))
            {
                Server.Instance.Loaded += delegate()
                {
                    Guid scriptid = Guid.Empty;
                    foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                    {
                        if (s.Code == this.failformula_)
                        {
                            scriptid = s.Id;
                            break;
                        }
                    }

                    if (scriptid == Guid.Empty)
                    {
                        Script script = new Script(Script.Types.Ability, "FailFormula_" + this.name_, this.failformula_);
                        Server.Instance.Add<Script>(script);
                        scriptid = script.Id;
                    }
                    if (this.scripts2_.ContainsKey(ScriptType.FailFormula))
                        this.scripts2_[ScriptType.FailFormula] = scriptid;
                    else
                        this.scripts2_.Add(ScriptType.FailFormula, scriptid);
                };
            }

            this.levelupformula_ = this.missformula_ = this.failformula_ = null;

            if (this.scripts_ != null)
            {
                Server.Instance.Loaded += delegate()
                {
                    this.scripts_.OnDeserialization(null);
                    foreach (KeyValuePair<ScriptType, string> kvp in this.scripts_)
                    {
                        if (!string.IsNullOrEmpty(kvp.Value))
                        {
                            Guid scriptid = Guid.Empty;
                            foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                            {
                                if (s.Code == kvp.Value)
                                {
                                    scriptid = s.Id;
                                    break;
                                }
                            }

                            if (scriptid == Guid.Empty)
                            {
                                Script script = new Script(Script.Types.Ability, kvp.Key + "_" + this.name_, kvp.Value);
                                Server.Instance.Add<Script>(script);
                                scriptid = script.Id;
                            }
                            if (this.scripts2_.ContainsKey(kvp.Key))
                                this.scripts2_[kvp.Key] = scriptid;
                            else
                                this.scripts2_.Add(kvp.Key, scriptid);
                        }
                    }
                    this.scripts_ = null;
                };
            }
        }
        #endregion
    }
}
