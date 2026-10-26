using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Reflection;
using LuaInterface;
using Damdai.TypeConverters;

namespace Ultimud
{
    /// <summary>
    /// In-game items such as weapons, armor, and other inanimate objects.
    /// </summary>
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Item : Object
    {
        enum ScriptType
        {
            OnUse
        }

        Guid blueprintId_ = Guid.Empty;
        Character.BodyParts equipLocation_;
        List<StatModifier> statMods_ = new List<StatModifier>();
        List<StatModifier> targetStatMods_ = new List<StatModifier>();
        List<StatusEffect> addUserStatusEffects_ = new List<StatusEffect>();
        List<StatusEffect> addTargetStatusEffects_ = new List<StatusEffect>();
        List<StatusEffect> removeTargetStatusEffects_ = new List<StatusEffect>();
        int prepTime_;
        int recoverTime_;
        List<Guid> raceRestrictions_ = new List<Guid>();
        List<Guid> jobRestrictions_ = new List<Guid>();
        List<StatModifier> abilityStatMods_ = new List<StatModifier>();
        List<Guid> abilityRequirements_ = new List<Guid>();
        List<Stat> statRequirements_ = new List<Stat>();
        Dictionary<ScriptType, string> itemscripts_ = new Dictionary<ScriptType, string>();
        int maxrange_;
        uint minrange_;
#pragma warning disable 0169, 0414
        string type_; //Deprecated.
#pragma warning restore 0169, 0414

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_ = new Lua();

        #region Properties
        [Browsable(false)]
        public Guid BlueprintId
        {
            get { return this.blueprintId_; }
        }

        [Browsable(false)]
        public override int IconIndex
        {
            get
            {
                return 23;
            }
        }

        [Description("The location on the body that this item can be worn.")]
        [DefaultValue(Character.BodyParts.None)]
        public Character.BodyParts EquipLocation
        {
            get { return this.equipLocation_; }
            set { this.equipLocation_ = value; }
        }

        [Browsable(false)]
        new public Sex Gender
        {
            get { return base.Gender; }
        }

        [Browsable(false)]
        new public Location Location
        {
            get { return base.Location; }
            set { base.Location = value; }
        }

        new public Object Parent
        {
            get { return base.Parent; }
            set
            {
                if (this.Parent is Character)
                {
                    Character ch = this.Parent as Character;
                    ch.Remove(this, true);
                }

                if (this.ObjectFlags.Get((int)ObjFlags.Money) && value is Character && (value as Character).Position != Positions.Dead)
                {
                    value.Value += this.Value;
                    value.Send("You are $" + this.Value.ToString(Server.Instance.Settings.NumberFormat) + " richer!");
                    base.Parent = null;
                }
                else
                {
                    base.Parent = value;
                }
            }
        }

        [Browsable(false)]
        new public Positions Position
        {
            get { return base.Position; }
        }

        [Category("Stat Modifications")]
        [DisplayName("User")]
        [Description("The amount to modify user's stats when this item is used.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> UserStatMods
        {
            get { return this.statMods_; }
            set { this.statMods_ = value; }
        }

        [Category("Stat Modifications")]
        [DisplayName("Target")]
        [Description("The amount to modify target's stats when this item is used on them.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> TargetStatMods
        {
            get { return this.targetStatMods_; }
            set { this.targetStatMods_ = value; }
        }

        [Category("Stat Modifications")]
        [DisplayName("Ability")]
        [Description("The amount to modify user's stats when this item is used in conjunction with an ability. For example, attacking with a heavy sword would use more energy than attacking with a stick.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> AbilityStatMods
        {
            get { return this.abilityStatMods_; }
            set { this.abilityStatMods_ = value; }
        }

        [Description("The amount of time it takes in seconds to prepare this item for use.")]
        [DefaultValue(0)]
        public int PrepareTime
        {
            get { return this.prepTime_; }
            set { this.prepTime_ = value; }
        }

        [Description("The amount of time it takes in seconds to recover from using this item.")]
        [DefaultValue(0)]
        public int RecoverTime
        {
            get { return this.recoverTime_; }
            set { this.recoverTime_ = value; }
        }

        [Category("Status Effects")]
        [DisplayName("AddToUser")]
        [Description("The status effects that this item adds to user when used.")]
        [TypeConverter(typeof(ListConverter<StatusEffect>))]
        public List<StatusEffect> AddUserStatusEffects
        {
            get { return this.addUserStatusEffects_; }
            set { this.addUserStatusEffects_ = value; }
        }

        [Category("Status Effects")]
        [DisplayName("AddToTarget")]
        [Description("The status effects that this item will add to target when used.")]
        [TypeConverter(typeof(ListConverter<StatusEffect>))]
        public List<StatusEffect> AddTargetStatusEffects
        {
            get { return this.addTargetStatusEffects_; }
            set { this.addTargetStatusEffects_ = value; }
        }

        [Category("Status Effects")]
        [DisplayName("RemoveFromTarget")]
        [Description("The status effects that this item can remove from target.")]
        [TypeConverter(typeof(ListConverter<StatusEffect>))]
        public List<StatusEffect> RemoveTargetStatusEffects
        {
            get { return this.removeTargetStatusEffects_; }
            set { this.removeTargetStatusEffects_ = value; }
        }

        [Category("Requirements")]
        [DisplayName("Race")]
        [Description("Which races can use this item? If blank, all races will be able to use it.")]
        [Editor(typeof(Server.MultiObjectSelector<Race>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Race>))]
        public List<Guid> RaceRestrictions
        {
            get { return this.raceRestrictions_; }
            set { this.raceRestrictions_ = value; }
        }

        [Category("Requirements")]
        [DisplayName("Job")]
        [Description("Which jobs can use this item? If blank, all jobs will be able to use it.")]
        [Editor(typeof(Server.MultiObjectSelector<Job>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Job>))]
        public List<Guid> JobRestrictions
        {
            get { return this.jobRestrictions_; }
            set { this.jobRestrictions_ = value; }
        }

        [Category("Requirements")]
        [DisplayName("Abilities")]
        [Description("What abilities must the character know in order to use this item? If no requirement, leave blank.")]
        [Editor(typeof(Server.MultiObjectSelector<Ability>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public List<Guid> AbilityRequirements
        {
            get { return this.abilityRequirements_; }
            set { this.abilityRequirements_ = value; }
        }

        [Category("Requirements")]
        [DisplayName("Stats")]
        [Description("What stat values must the character have in order to use this item? If no requirement, leave blank.")]
        [TypeConverter(typeof(ListConverter<Stat>))]
        public List<Stat> StatRequirements
        {
            get { return this.statRequirements_; }
            set { this.statRequirements_ = value; }
        }

        [Editor(typeof(StringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Description("This script is executed when this item is used.\r\n\r\nObjects: item, actor\r\nReturns: nothing\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Category("Scripts")]
        public string OnUse
        {
            get
            {
                string script;
                if (!this.itemscripts_.TryGetValue(ScriptType.OnUse, out script))
                    this.itemscripts_.Add(ScriptType.OnUse, null);
                return script;
            }
            set
            {
                if (this.itemscripts_.ContainsKey(ScriptType.OnUse))
                    this.itemscripts_[ScriptType.OnUse] = value;
                else
                    this.itemscripts_.Add(ScriptType.OnUse, value);
            }
        }

        [Description("The maximum range of this item.")]
        [DefaultValue(0)]
        public int RangeMax
        {
            get { return this.maxrange_; }
            set { this.maxrange_ = value; }
        }

        [Description("The minimum range of this item.")]
        [DefaultValue(0)]
        public uint RangeMin
        {
            get { return this.minrange_; }
            set { this.minrange_ = value; }
        }
        #endregion

        public Item()
        {
            lock (this.Stats)
            {
                this.Stats.Clear();
            }
            this.DeathMessage = "{1} crumbles to dust!";
        }
        
        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Item)))
                Server.Instance.DataSets.Add(typeof(Item), new Dictionary<Guid, IIdentifiable>());
        }
        public void Init(Item other)
        {
            base.Init(other);
            this.UserStatMods = new List<StatModifier>();
            if (other.UserStatMods != null)
            {
                foreach (StatModifier mod in other.UserStatMods)
                    this.UserStatMods.Add(mod.Clone());
            }
            this.TargetStatMods = new List<StatModifier>();
            if (other.TargetStatMods != null)
            {
                foreach (StatModifier mod in other.TargetStatMods)
                    this.TargetStatMods.Add(mod.Clone());
            }
            this.AbilityStatMods = new List<StatModifier>();
            if (other.AbilityStatMods != null)
            {
                foreach (StatModifier mod in other.AbilityStatMods)
                    this.AbilityStatMods.Add(mod.Clone());
            }
            this.AddUserStatusEffects = new List<StatusEffect>();
            if (other.AddUserStatusEffects != null)
            {
                foreach (StatusEffect se in other.AddUserStatusEffects)
                    this.AddUserStatusEffects.Add(StatusEffect.Create(se));
            }
            this.AddTargetStatusEffects = new List<StatusEffect>();
            if (other.AddTargetStatusEffects != null)
            {
                foreach (StatusEffect se in other.AddTargetStatusEffects)
                    this.AddTargetStatusEffects.Add(StatusEffect.Create(se));
            }
            this.RemoveTargetStatusEffects = new List<StatusEffect>();
            if (other.RemoveTargetStatusEffects != null)
            {
                foreach (StatusEffect se in other.RemoveTargetStatusEffects)
                    this.RemoveTargetStatusEffects.Add(StatusEffect.Create(se));
            }
            this.RaceRestrictions = other.RaceRestrictions != null ? new List<Guid>(other.RaceRestrictions) : new List<Guid>();
            this.JobRestrictions = other.JobRestrictions != null ? new List<Guid>(other.JobRestrictions) : new List<Guid>();
            this.AbilityRequirements = other.AbilityRequirements != null ? new List<Guid>(other.AbilityRequirements) : new List<Guid>();
            this.StatRequirements = other.StatRequirements != null ? new List<Stat>(other.StatRequirements) : new List<Stat>();
        }
        public static Item Create(Item blueprint)
        {
            Item item = blueprint.MemberwiseClone() as Item;
            item.blueprintId_ = blueprint.Id;
            item.Init(blueprint);
            return item;
        }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                string str = this.Name;
                if (this.UserStatMods.Count > 0)
                {
                    str += " (";
                    foreach (StatModifier mod in this.UserStatMods)
                        str += mod.ToString() + ", ";
                    str = str.TrimEnd(',', ' ') + ")";
                }
                return str;
            }

            return base.ToString();
        }
        
        [Obfuscation(Exclude = false)]
        public void Apply(Object o)
        {
            StatModifier.Apply(this.UserStatMods, o, this, o, null, false);
        }
        
        [Obfuscation(Exclude = false)]
        public void ApplyTargetStatusEffects(Object target)
        {
            foreach (StatusEffect status in this.AddTargetStatusEffects)
                status.Apply(target, this);
        }
        
        [Obfuscation(Exclude = false)]
        void ApplyUserStatusEffects(Object user)
        {
            foreach (StatusEffect status in this.AddUserStatusEffects)
                status.Apply(user, this);
        }
        
        [Obfuscation(Exclude = false)]
        public void Undo(Object o)
        {
            StatModifier.Undo(this.UserStatMods, o, this, false);
            foreach (StatusEffect effect in this.AddUserStatusEffects)
            {
                StatusEffect se = o.GetStatusEffect(effect.BlueprintId);
                if (o.StatusEffects.Contains(se))
                    o.RemoveStatusEffect(se);
            }
        }
        
        public override void Die(Object killer)
        {
            base.Die(killer);
            this.DropItems();
            this.Parent = null;
        }
        void DropItems()
        {
            foreach (Item item in this.GetChildren<Item>())
                item.Location = this.Location;
        }
        public bool CanUse(Character ch)
        {
            if (this.RaceRestrictions != null && this.RaceRestrictions.Count > 0 && !this.RaceRestrictions.Contains(ch.RaceId))
                return false;
            if (this.JobRestrictions != null && this.JobRestrictions.Count > 0 && !this.JobRestrictions.Contains(ch.JobId))
                return false;
            if (this.AbilityRequirements != null && this.AbilityRequirements.Count > 0 && !this.AbilityRequirements.TrueForAll(delegate(Guid id) { return ch.Abilities.ContainsKey(id); }))
                return false;

            if (this.StatRequirements != null && this.StatRequirements.Count > 0 && !this.StatRequirements.TrueForAll(delegate(Stat statreq) 
            {
                Stat stat;
                if (ch.Stats.TryGetValue(statreq.Id, out stat) && stat.Current >= statreq.Current && stat.Max >= statreq.Max)
                    return true;
                else
                    return false;
            }))
            {
                return false;
            }

            return true;
        }
        
        [Obfuscation(Exclude = false)]
        public void OnUsed(Character ch)
        {
            this.ApplyUserStatusEffects(ch);
            this.DoScript(ScriptType.OnUse, ch);
        }

        [Obfuscation(Exclude = false)]
        void DoScript(ScriptType type, Character ch)
        {
            string script;
            if (!this.itemscripts_.TryGetValue(type, out script) || string.IsNullOrEmpty(script))
                return;

            try
            {
                lock (this.lua_)
                {
                    this.lua_["item"] = this;
                    this.lua_["actor"] = ch;
                    this.lua_.DoString(script);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR: Item: " + type + " (" + this.Name + ")");
                Debug.WriteLine(script, "Script");
                Debug.WriteLine(e.ToString(), "StackTrace");
#if DEBUG
                ch.Send(e.Message);
#endif
            }
        }
        
        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal new void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);
            if (this.StatRequirements == null) this.StatRequirements = new List<Stat>();
            if (this.AbilityRequirements == null) this.AbilityRequirements = new List<Guid>();
            if (this.itemscripts_ == null) this.itemscripts_ = new Dictionary<ScriptType, string>();
            if (this.statMods_ == null) this.statMods_ = new List<StatModifier>();
            if (this.targetStatMods_ == null) this.targetStatMods_ = new List<StatModifier>();
            if (this.abilityStatMods_ == null) this.abilityStatMods_ = new List<StatModifier>();
            this.lua_ = new Lua();
            this.type_ = null;
        }
        
        [Obfuscation(Exclude = false)]
        internal override void Draw(Graphics g, AreaCanvas canvas, AreaCanvas.View view)
        {
            if (canvas.Contains(this))
            {
                switch (view)
                {
                    case AreaCanvas.View.Design1:
                    case AreaCanvas.View.Design2:
                        g.DrawImage(Ultimud.Properties.Resources.item2,
                            (this.Location.Point.X - canvas.Offset.X) * canvas.Zoom,
                            (this.Location.Point.Y - canvas.Offset.Y) * canvas.Zoom,
                            canvas.Zoom,
                            canvas.Zoom);
                        break;
                    case AreaCanvas.View.Game:
                        using (Font font = new Font("fixedsys", Math.Max((int)(canvas.Zoom / 1.7), 1), FontStyle.Bold))
                        {
                            int xpos = (this.Location.Point.X - canvas.Offset.X) * canvas.Zoom;
                            int ypos = (this.Location.Point.Y - canvas.Offset.Y) * canvas.Zoom;
                            g.FillRectangle(Brushes.Black, xpos, ypos, canvas.Zoom, canvas.Zoom);
                            using (SolidBrush b = new SolidBrush(Telnet.ColorToSystemColor(this.MapColor)))
                                g.DrawString(this.MapSymbol, font, b, xpos, ypos);
                        }
                        break;
                }
            }
        }
    }
}
