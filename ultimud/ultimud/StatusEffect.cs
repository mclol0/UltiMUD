using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization;
using System.Diagnostics;
using LuaInterface;
using Damdai;
using Damdai.TypeConverters;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class StatusEffect
    {
        public enum ScriptType
        {
            OnStart,
            OnUpdate,
            OnEnd,
        }

        #region Members
        Guid blueprintId_;
        int chance_ = 100;
#pragma warning disable 0169, 0414
        int duration_; //Deprecated.
        int updateFrequency_; //Deprecated.
#pragma warning restore 0169, 0414
        List<StatModifier> statMods_ = new List<StatModifier>();
        List<StatFactor> offensiveDurationFactors_ = new List<StatFactor>();
        List<StatFactor> defensiveDurationFactors_ = new List<StatFactor>();
        bool permanent_;
#pragma warning disable 0169, 0414
        DateTime lastUpdate_; //Deprecated.
        DateTime created_; //Deprecated.
#pragma warning restore 0169, 0414
        BitArray objectFlags_ = new BitArray(16);
        Object.Positions position_ = Object.Positions.None;
        BitArray removeOnPosition_ = new BitArray(16);
        Timer updateTimer_;
        Timer broadcastTimer_;
        Timer durationTimer_;
        List<Guid> removeOnStatMax_ = new List<Guid>();
        List<Guid> removeOnStatMin_ = new List<Guid>();
        Dictionary<ScriptType, string> scripts_ = new Dictionary<ScriptType, string>();

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_ = new Lua();
        #endregion

        #region Properties
        [DisplayName("Status")]
        [Description("The type of this status effect.")]
        [TypeConverter(typeof(StatusEffect.IdToNameConverter))]
        [Editor(typeof(StatusEffectSelector), typeof(UITypeEditor))]
        public Guid BlueprintId
        {
            get { return this.blueprintId_; }
            set { this.blueprintId_ = value; }
        }

        [Browsable(false)]
        public StatusEffectBlueprint Blueprint
        {
            get { return Server.Instance.Get<StatusEffectBlueprint>(this.blueprintId_); }
        }

        [Description("The percent chance that this status will take effect.")]
        [DefaultValue(100)]
        public int Chance
        {
            get { return this.chance_; }
            set { this.chance_ = value; }
        }

        [Category("Duration")]
        [DisplayName("Base")]
        [Description("The length of time that this effect will last.")]
        public Timer Duration
        {
            get { return this.durationTimer_; }
            set { this.durationTimer_ = value; }
        }

        [Category("Timers")]
        [DisplayName("Update")]
        [Description("The length of time in seconds between each update.")]
        public Timer UpdateTimer
        {
            get { return this.updateTimer_; }
            set { this.updateTimer_ = value; }
        }

        [Category("Timers")]
        [DisplayName("Broadcast")]
        [Description("The length of time in seconds between each message broadcast.")]
        public Timer BroadcastTimer
        {
            get { return this.broadcastTimer_; }
            set { this.broadcastTimer_ = value; }
        }

        [Description("The amount to modify effected object's stats. If UpdateFrequency is 0, modifications will be applied immediately. Otherwise, they will be applied every update.")]
        [DisplayName("StatMods")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> StatModifications
        {
            get { return this.statMods_; }
            set { this.statMods_ = value; }
        }

        [Category("Duration")]
        [DisplayName("OffensiveFactors")]
        [Description("The actor's stats that affect the duration of this status effect.")]
        [TypeConverter(typeof(StatFactor.CollectionConverter))]
        public List<StatFactor> OffensiveDurationFactors
        {
            get
            {
                if (this.offensiveDurationFactors_ == null)
                {
                    this.offensiveDurationFactors_ = new List<StatFactor>();
                }
                return this.offensiveDurationFactors_;
            }
            set { this.offensiveDurationFactors_ = value; }
        }

        [Category("Duration")]
        [DisplayName("DefensiveFactors")]
        [Description("The target's stats that affect the duration of this status effect.")]
        [TypeConverter(typeof(StatFactor.CollectionConverter))]
        public List<StatFactor> DefensiveDurationFactors
        {
            get
            {
                if (this.defensiveDurationFactors_ == null)
                {
                    this.defensiveDurationFactors_ = new List<StatFactor>();
                }
                return this.defensiveDurationFactors_;
            }
            set { this.defensiveDurationFactors_ = value; }
        }

        [Browsable(false)]
        public int SecondsRemaining
        {
            get
            {
                return (this.Duration != null ? (int)this.Duration.Length.TotalSeconds : 0);
            }
        }

        [Description("A permanent status effect does not go away upon death. Note: If an item adds a permanent status effect when worn, it will be removed when the item is removed.")]
        [DefaultValue(false)]
        public bool IsPermanent
        {
            get { return this.permanent_; }
            set { this.permanent_ = value; }
        }

        [DisplayName("SetObjectFlags")]
        [Description("The object flags that this status effect adds.")]
        [Editor(typeof(FlagEditor<Object.ObjFlags>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<Object.ObjFlags>))]
        public BitArray ObjectFlags
        {
            get { return this.objectFlags_; }
            set { this.objectFlags_ = value; }
        }

        [DisplayName("SetPosition")]
        [Description("Set the affected object's position to this value.")]
        [DefaultValue(Object.Positions.None)]
        public Object.Positions Position
        {
            get { return this.position_; }
            set { this.position_ = value; }
        }

        [Description("Should this status effect be removed if the character takes one of these positions?")]
        [Editor(typeof(FlagEditor<Object.Positions>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<Object.Positions>))]
        public BitArray RemoveOnPosition
        {
            get
            {
                if (this.removeOnPosition_ == null)
                {
                    this.removeOnPosition_ = new BitArray(16);
                }
                return this.removeOnPosition_;
            }
            set { this.removeOnPosition_ = value; }
        }

        [Description("Whether or not the object's stats that this status effect modifies are to be undone when this status effect is removed.")]
        bool UndoStatModsOnRemove
        {
            get { return (this.UpdateTimer == null); }
        }

        [Description("This status will be removed when all selected stats reach their maximum.")]
        [Editor(typeof(Server.MultiObjectSelector<StatBlueprint>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<StatBlueprint>))]
        public List<Guid> RemoveOnStatMax
        {
            get { return this.removeOnStatMax_; }
            set { this.removeOnStatMax_ = value; }
        }

        [Description("This status will be removed when all selected stats reach their minimum.")]
        [Editor(typeof(Server.MultiObjectSelector<StatBlueprint>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<StatBlueprint>))]
        public List<Guid> RemoveOnStatMin
        {
            get { return this.removeOnStatMin_; }
            set { this.removeOnStatMin_ = value; }
        }

        [Description("This script is executed when this status is applied.\r\n\r\nObjects: actor, target, status\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        [Category("Scripts")]
        public string OnStart
        {
            get
            {
                ScriptType scripttype = ScriptType.OnStart;
                string script;
                if (!this.scripts_.TryGetValue(scripttype, out script))
                    this.scripts_.Add(scripttype, null);
                return script;
            }
            set
            {
                ScriptType scripttype = ScriptType.OnStart;
                if (this.scripts_.ContainsKey(scripttype))
                    this.scripts_[scripttype] = value;
                else
                    this.scripts_.Add(scripttype, value);
            }
        }

        [Description("This script is executed when this status is updated.\r\n\r\nObjects: actor, status\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        [Category("Scripts")]
        public string OnUpdate
        {
            get
            {
                ScriptType scripttype = ScriptType.OnUpdate;
                string script;
                if (!this.scripts_.TryGetValue(scripttype, out script))
                    this.scripts_.Add(scripttype, null);
                return script;
            }
            set
            {
                ScriptType scripttype = ScriptType.OnUpdate;
                if (this.scripts_.ContainsKey(scripttype))
                    this.scripts_[scripttype] = value;
                else
                    this.scripts_.Add(scripttype, value);
            }
        }

        [Description("This script is executed when this status is removed.\r\n\r\nObjects: actor, target, status\r\nReturns: nothing\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(StringEditor), typeof(UITypeEditor))]
        [Category("Scripts")]
        public string OnEnd
        {
            get
            {
                ScriptType scripttype = ScriptType.OnEnd;
                string script;
                if (!this.scripts_.TryGetValue(scripttype, out script))
                    this.scripts_.Add(scripttype, null);
                return script;
            }
            set
            {
                ScriptType scripttype = ScriptType.OnEnd;
                if (this.scripts_.ContainsKey(scripttype))
                    this.scripts_[scripttype] = value;
                else
                    this.scripts_.Add(scripttype, value);
            }
        }
        #endregion

        public StatusEffect()
        {
        }

        public static StatusEffect Create(StatusEffect other)
        {
            StatusEffect effect = other.MemberwiseClone() as StatusEffect;
            effect.StatModifications = new List<StatModifier>();
            if (other.StatModifications != null)
            {
                foreach (StatModifier mod in other.StatModifications)
                    effect.StatModifications.Add(mod.Clone());
            }
            effect.ObjectFlags = new BitArray(16);
            effect.ObjectFlags = (BitArray)other.ObjectFlags.Clone();
            effect.RemoveOnPosition = new BitArray(16);
            effect.RemoveOnPosition = (BitArray)other.RemoveOnPosition.Clone();
            if (other.BroadcastTimer != null)
                effect.BroadcastTimer = new Timer(other.BroadcastTimer.Length);
            if (other.UpdateTimer != null)
                effect.UpdateTimer = new Timer(other.UpdateTimer.Length);
            if (other.Duration != null)
                effect.Duration = new Timer(other.Duration.Length);
            effect.lua_ = new Lua();

            return effect;
        }
        
        [Obfuscation(Exclude = false)]
        public void Update(Object o)
        {
            if (this.RemoveOnPosition.Get((int)o.Position))
            {
                o.RemoveStatusEffect(this);
                return;
            }

            if (this.removeOnStatMax_ != null && this.removeOnStatMax_.Count > 0 && this.removeOnStatMax_.TrueForAll(delegate(Guid statid)
            {
                Stat stat = o.GetStat(statid);
                if (stat == null || stat.Current >= stat.Max)
                    return true;
                return false;
            }))
            {
                o.RemoveStatusEffect(this);
                return;
            }

            if (this.removeOnStatMin_ != null && this.removeOnStatMin_.Count > 0 && this.removeOnStatMin_.TrueForAll(delegate(Guid statid)
            {
                StatBlueprint statbp = Server.Instance.Get<StatBlueprint>(statid);
                Stat stat = o.GetStat(statid);
                if (stat == null || (statbp != null && stat.Current <= statbp.Min))
                    return true;
                return false;
            }))
            {
                o.RemoveStatusEffect(this);
                return;
            }

            if (this.BroadcastTimer != null && !this.BroadcastTimer.Update())
            {
                StatusEffectBlueprint bp = this.Blueprint;
                if (bp != null)
                    o.Area.Send(bp.UpdateMessage, o, null, 0);
                this.BroadcastTimer.Reset();
            }

            if(this.UpdateTimer != null && !this.UpdateTimer.Update())
            {
                object[] rvals = this.DoScript(ScriptType.OnUpdate, o, null);
                if (rvals != null && rvals[0].ToString() == "cancel")
                {
                    o.RemoveStatusEffect(this);
                    return;
                }
                else
                {
                    StatModifier.Apply(this.StatModifications, o, o, o, null, false);
                    this.UpdateTimer.Reset();
                }
            }

            if (!this.IsPermanent && this.Duration != null && !this.Duration.Update())
            {
                o.RemoveStatusEffect(this);
                this.Duration = null;
                return;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (this.BlueprintId != Guid.Empty)
            {
                StatusEffectBlueprint bp = this.Blueprint;
                if (bp != null)
                    return bp.Name;
                else
                    return "DELETED";
            }

            return base.ToString();
        }
        
        public void Apply(Object target, Object actor)
        {
            StatusEffectBlueprint bp = this.Blueprint;
            if (bp != null)
            {
                if (Server.RandomNumber.Next(100) <= this.Chance)
                {
                    lock (target.StatusEffects)
                    {
                        StatusEffect effect = target.GetStatusEffect(this.BlueprintId);
                        if (effect == null)
                        {
                            effect = StatusEffect.Create(this);
                            if (effect.Duration != null)
                            {
                                long factorAdjustment = 0;
                                if (actor != null && effect.OffensiveDurationFactors != null)
                                {
                                    foreach (StatFactor factor in effect.OffensiveDurationFactors)
                                    {
                                        Stat stat = actor.GetStat(factor.StatId);
                                        if (stat != null)
                                            factorAdjustment += factor.Calculate(stat);
                                    }
                                }
                                if (target != null && effect.DefensiveDurationFactors != null)
                                {
                                    foreach (StatFactor factor in effect.DefensiveDurationFactors)
                                    {
                                        Stat stat = target.GetStat(factor.StatId);
                                        if (stat != null)
                                            factorAdjustment -= factor.Calculate(stat);
                                    }
                                }
                                if (factorAdjustment != 0)
                                {
                                    double totalSecs = System.Math.Max(1.0, effect.Duration.Length.TotalSeconds + factorAdjustment);
                                    effect.Duration = new Timer(TimeSpan.FromSeconds(totalSecs));
                                }
                            }
                            target.StatusEffects.Add(effect);
                            if (!bp.StartMessage.Empty)
                                target.Area.Send(bp.StartMessage, target, actor, 0);
                            else if (bp.Visible)
                            {
                                target.Send("You have been affected by " + bp.Name + "!");
                                if (actor != null && actor != target)
                                    actor.Send(target.Name + " has been affected by " + bp.Name + "!");
                            }

                            effect.SetObjectFlags(target);
                            effect.SetPosition(target);
                            if (effect.UndoStatModsOnRemove)
                                StatModifier.Apply(effect.StatModifications, target, actor, target, null, false);
                        }
                        else
                        {
                            if (effect.Duration != null)
                                effect.Duration.Reset();
                        }
                    }
                    this.DoScript(ScriptType.OnStart, actor, target);
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void Undo(Object o)
        {
            if (this.Position != Object.Positions.None)
                o.Position = Object.Positions.Stopped;

            for (int x = 0; x < this.ObjectFlags.Length; ++x)
                if (this.ObjectFlags[x])
                    o.ObjectFlags[x] = false;

            StatusEffectBlueprint bp = this.Blueprint;
            if (bp != null)
            {
                if (bp.EndMessage.Empty)
                {
                    if (bp.Visible)
                        o.Send("You are no longer affected by " + bp.Name + ".");
                }
                else
                    o.Area.Send(bp.EndMessage, o, null, 0);
            }
            if (this.UndoStatModsOnRemove)
                StatModifier.Undo(this.StatModifications, o, null, false);

            this.DoScript(ScriptType.OnEnd, o, null);
        }
        
        [Obfuscation(Exclude = false)]
        void SetObjectFlags(Object o)
        {
            for (int x = 0; x < this.ObjectFlags.Length; ++x)
                if (this.ObjectFlags[x])
                    o.ObjectFlags[x] = true;
        }
        
        [Obfuscation(Exclude = false)]
        void SetPosition(Object o)
        {
            if (this.Position != Object.Positions.None)
                o.Position = this.Position;
        }
        
        [Obfuscation(Exclude = false)]
        object[] DoScript(ScriptType type, Object actor, Object target)
        {
            string script;
            if (!this.scripts_.TryGetValue(type, out script) || string.IsNullOrEmpty(script))
                return null;

            try
            {
                lock (this.lua_)
                {
                    string str = script;
                    if (actor != null) this.lua_["actor"] = actor;
                    else str = str.Insert(0, "actor=nil\r\n");
                    if (target != null) this.lua_["target"] = target;
                    else str = str.Insert(0, "target=nil\r\n");
                    this.lua_["status"] = this;
                    return this.lua_.DoString(str);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR: StatusEffect: " + type + " (" + this.Blueprint.Name + ")");
                Debug.WriteLine(script, "Script");
                Debug.WriteLine(e.ToString(), "StackTrace");
#if DEBUG
                actor.Send(e.Message);
#endif
            }
            return null;
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            if (this.duration_ > 0)
            {
                this.Duration = new Timer(new TimeSpan(0, 0, this.duration_));
                this.duration_ = 0;
            }
            if (this.updateFrequency_ > 0)
            {
                this.UpdateTimer = new Timer(new TimeSpan(0, 0, this.updateFrequency_));
                this.updateFrequency_ = 0;
            }
            if (this.scripts_ == null)
                this.scripts_ = new Dictionary<ScriptType, string>();

            this.lastUpdate_ = default(DateTime);
            this.created_ = default(DateTime);
            this.lua_ = new Lua();
        }

        [Obfuscation(Exclude = false)]
        public class IdToNameConverter : StringConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is Guid)
                {
                    StatusEffectBlueprint stat = Server.Instance.Get<StatusEffectBlueprint>((Guid)value);
                    if (stat != null)
                        return stat.Name;
                }

                return string.Empty;
            }
        }
    }
}
