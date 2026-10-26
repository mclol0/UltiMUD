using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using LuaInterface;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class StatModifier : IDisposable
    {
        public enum ApplyTarget
        {
            Current,
            Max,
            CurrentAndMax
        }

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Lua lua_ = new Lua();

        Guid statId_;
#pragma warning disable 0169, 0414
        int amount_; //Deprecated.
        int random_; //Deprecated.
        List<StatFactor> offensiveFactors_ = new List<StatFactor>(); //Deprecated.
        List<StatFactor> defensiveFactors_ = new List<StatFactor>(); //Deprecated.
#pragma warning restore 0169, 0414
        ApplyTarget applyTo_ = ApplyTarget.Current;
        int chance_ = 100;
        bool restore_;
        bool silent_;
        long mod_;
#pragma warning disable 0169, 0414
        string formula_; //Deprecated.
#pragma warning restore 0169, 0414
        Guid formula_script_id_;

        #region Properties
        [DisplayName("Stat")]
        [Description("The stat to modify.")]
        [TypeConverter(typeof(Stat.IdToNameConverter))]
        [Editor(typeof(StatSelector), typeof(UITypeEditor))]
        public Guid StatId
        {
            get { return this.statId_; }
            set { this.statId_ = value; }
        }

        [Browsable(false)]
        public string StatName
        {
            get
            {
                StatBlueprint bp = Server.Instance.Get<StatBlueprint>(this.StatId);
                return (bp != null ? bp.Name : null);
            }
        }

        [Description("Whether to apply this stat modification to target's current or max stat.")]
        [DefaultValue(ApplyTarget.Current)]
        public ApplyTarget ApplyTo
        {
            get { return this.applyTo_; }
            set { this.applyTo_ = value; }
        }

        [Description("The percent chance that this modifier will take effect.")]
        [DefaultValue(100)]
        public int Chance
        {
            get { return this.chance_; }
            set { this.chance_ = value; }
        }

        [Description("Should current stat be restored to its max?")]
        [DefaultValue(false)]
        public bool Restore
        {
            get { return this.restore_; }
            set { this.restore_ = value; }
        }

        [Description("Should the player be aware of this stat modification?")]
        [DefaultValue(false)]
        public bool Silent
        {
            get { return this.silent_; }
            set { this.silent_ = value; }
        }

        [Description("The formula to use to calculate this stat modification.\r\n\r\nObjects: actor, target, ability\r\nReturns: int\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
        [Editor(typeof(Script.StatModifierScriptSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Script>))]
        public Guid Formula
        {
            get { return this.formula_script_id_; }
            set { this.formula_script_id_ = value; }
        }
        #endregion

        public long Calculate(Object actor, Object target, Ability ability, string script)
        {
            if (string.IsNullOrEmpty(script))
                return 0;

            try
            {
                lock (this.lua_)
                {
                    if (string.IsNullOrEmpty(script))
                    {
                        Trace.WriteLine("Script is null or empty.", "WARNING");
                        return 0;
                    }

                    string str = string.Empty;
                    if (actor == null) str += "actor=nil\r\n";
                    else this.lua_["actor"] = actor;
                    if (target == null) str += "target=nil\r\n";
                    else this.lua_["target"] = target;
                    if (ability == null) str += "ability=nil\r\n";
                    else this.lua_["ability"] = ability;
                    str += script;
                    return Convert.ToInt64(this.lua_.DoString(str)[0]);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR: StatModifier");
                Debug.WriteLine(script, "Script");
                Debug.WriteLine(e.ToString(), "StackTrace");
                return 0;
            }
        }
        public long Calculate(Object actor, Object target, Ability ability)
        {
            if (this.formula_script_id_ == Guid.Empty)
                return 0;
            Script script = Server.Instance.Get<Script>(this.formula_script_id_);
            if (script == null)
            {
                Debug.WriteLine("Invalid script.", "StatModifier");
                this.formula_script_id_ = Guid.Empty;
                return 0;
            }

            if (string.IsNullOrEmpty(script.Code))
            {
                Trace.WriteLine("Script is null or empty: " + script.Name, "WARNING");
                return 0;
            }
            return this.Calculate(actor, target, ability, script.Code);
        }
        public long Apply(Object applyto, Object actor, Object target, Ability ability, string script, bool silent)
        {
            Stat stat = applyto.GetStat(this.StatId);
            if (stat == null)
            {
                stat = new Stat(this.StatId);
                lock (applyto.Stats)
                    applyto.Stats.Add(this.StatId, stat);
            }

            StatBlueprint bp = stat.Blueprint;
            if (bp == null)
                return 0;

            long change = 0;
            if (this.Restore)
            {
                if (bp.CanRestore)
                {
                    change = stat.Max - stat.Current;
                    stat.Current = stat.Max;
                    if (!silent && !this.Silent)
                        applyto.Send(Server.Instance.Settings.GetColorCode(Settings.Text.StatIncrease) + "Your " + bp.Name + " has been restored!");
                }
            }
            else
            {
                change = this.Calculate(actor, target, ability, script);
                change = this.Normalize(stat, bp, change);

                switch (this.ApplyTo)
                {
                    case StatModifier.ApplyTarget.Current:
                        if (stat.Current + change >= stat.Max)
                            change = stat.Max - stat.Current;
                        stat.Current += change;
                        break;
                    case StatModifier.ApplyTarget.Max:
                        if (stat.Max + change >= bp.Max)
                            change = bp.Max - stat.Max;
                        stat.Max += change;
                        break;
                    case StatModifier.ApplyTarget.CurrentAndMax:
                        if (change > 0)
                        {
                            if (stat.Max + change >= bp.Max)
                                change = bp.Max - stat.Max;
                            stat.Max += change;
                            stat.Current += change;
                        }
                        else
                        {
                            if (stat.Max + change <= bp.Min)
                                change = bp.Min - stat.Max;
                            stat.Current += change;
                            stat.Max += change;
                        }
                        break;
                }

                if (!silent && !this.Silent && change != 0)
                {
                    string str = "Your " + this.StatName + " has ";
                    if (change > 0)
                    {
                        str += "increased! (" + Server.Instance.Settings.GetColorCode(Settings.Text.StatIncrease) + "+" + change.ToString(Server.Instance.Settings.NumberFormat) + "{color:reset})";
                        applyto.Send(str);
                    }
                    else
                    {
                        str += "decreased! (" + Server.Instance.Settings.GetColorCode(Settings.Text.StatDecrease) + change.ToString(Server.Instance.Settings.NumberFormat) + "{color:reset})";
                        applyto.Send(str);
                    }
                }
            }

            this.mod_ = change;
            return change;
        }
        public long Apply(Object applyto, Object actor, Object target, Ability ability, bool silent)
        {
            if (this.Chance >= 100 || Server.RandomNumber.Next(100) <= this.Chance)
            {
                if (this.formula_script_id_ == Guid.Empty)
                    return 0;
                Script script = Server.Instance.Get<Script>(this.formula_script_id_);
                if (script == null)
                {
                    Debug.WriteLine("Invalid script.", "StatModifier");
                    this.formula_script_id_ = Guid.Empty;
                    return 0;
                }

                if (string.IsNullOrEmpty(script.Code))
                {
                    Trace.WriteLine("Script is null or empty: " + script.Name, "WARNING");
                    return 0;
                }
                return this.Apply(applyto, actor, target, ability, script.Code, silent);
            }
            return 0;
        }
        public long Undo(Object undofrom, Object influence, bool silent)
        {
            long change = -this.mod_;
            Stat stat = undofrom.GetStat(this.StatId);
            Debug.Assert(stat != null, "Stat is null.");
            if (stat == null)
            {
                Trace.WriteLine("[undofrom] has no " + (this.StatName ?? this.StatId.ToString()) + " stat.", "ERROR");
                return 0;
            }

            switch (this.ApplyTo)
            {
                case StatModifier.ApplyTarget.Current:
                    stat.Current += change;
                    break;
                case StatModifier.ApplyTarget.Max:
                    stat.Max += change;
                    break;
                case StatModifier.ApplyTarget.CurrentAndMax:
                    if (change > 0)
                    {
                        stat.Max += change;
                        stat.Current += change;
                    }
                    else
                    {
                        stat.Current += change;
                        stat.Max += change;
                    }
                    break;
            }

            if (!silent && !this.Silent && change != 0)
            {
                StatBlueprint bp = Server.Instance.Get<StatBlueprint>(this.StatId);
                if (bp != null)
                {
                    string str = "Your " + this.StatName + " has ";
                    if (change > 0)
                        str += "increased! (" + Server.Instance.Settings.GetColorCode(Settings.Text.StatIncrease) + "+" + change.ToString(Server.Instance.Settings.NumberFormat) + "{color:reset})";
                    else
                        str += "decreased! (" + Server.Instance.Settings.GetColorCode(Settings.Text.StatDecrease) + change.ToString(Server.Instance.Settings.NumberFormat) + "{color:reset})";
                    undofrom.Send(str);
                }
            }

            return change;
        }
        public static void Apply(List<StatModifier> mods, Object applyto, Object actor, Object target, Ability ability, bool silent)
        {
            foreach (StatModifier mod in mods)
                mod.Apply(applyto, actor, target, ability, silent);
        }
        public static void Undo(List<StatModifier> mods, Object undofrom, Object influence, bool silent)
        {
            foreach (StatModifier mod in mods)
                mod.Undo(undofrom, influence, silent);
        }
        public StatModifier Clone()
        {
            StatModifier clone = new StatModifier();
            clone.statId_ = this.statId_;
            clone.applyTo_ = this.applyTo_;
            clone.chance_ = this.chance_;
            clone.restore_ = this.restore_;
            clone.silent_ = this.silent_;
            clone.mod_ = 0;
            clone.formula_script_id_ = this.formula_script_id_;
            return clone;
        }
        public override string ToString()
        {
            return (this.StatId != Guid.Empty ? this.StatName : base.ToString());
        }
        long Normalize(Stat stat, StatBlueprint bp, long change)
        {
            long total = 0;
            switch (this.ApplyTo)
            {
                case StatModifier.ApplyTarget.Current:
                    total = stat.Current + change;
                    if (total >= stat.Max) change = stat.Max - stat.Current;
                    else if (total <= bp.Min) change = bp.Min - stat.Current;
                    break;
                case StatModifier.ApplyTarget.Max:
                case StatModifier.ApplyTarget.CurrentAndMax:
                    total = stat.Max + change;
                    if (total >= bp.Max) change = bp.Max - stat.Max;
                    else if (total <= bp.Min) change = bp.Min - stat.Max;
                    break;
            }
            return change;
        }
        public void Dispose()
        {
            if (this.lua_ != null)
            {
                this.lua_.Dispose();
                this.lua_ = null;
            }
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        void OnDeserialized(StreamingContext context)
        {
            this.lua_ = new Lua();

            if (this.amount_ != 0 || this.random_ != 0 || (this.offensiveFactors_ != null && this.offensiveFactors_.Count > 0) || (this.defensiveFactors_ != null && this.defensiveFactors_.Count > 0))
            {
                Server.Instance.Loaded += delegate()
                {
                    this.formula_ = "return ";
                    if (this.amount_ != 0)
                    {
                        if (this.amount_ < 0) this.formula_ += "-(";
                        this.formula_ += this.amount_ + " + ";
                    }
                    if (this.random_ != 0)
                        this.formula_ += "math.random(-" + this.random_ + ", " + this.random_ + ") + ";
                    bool found = false;
                    foreach (StatFactor factor in this.offensiveFactors_)
                    {
                        StatBlueprint statbp = Server.Instance.Get<StatBlueprint>(factor.StatId);
                        if (statbp != null)
                        {
                            if (found) this.formula_ += " + "; else found = true;
                            this.formula_ += "(" + factor.Factor + " * actor:GetStat" + factor.Target + "('" + statbp.Name + "'))";
                        }
                    }
                    foreach (StatFactor factor in this.defensiveFactors_)
                    {
                        StatBlueprint statbp = Server.Instance.Get<StatBlueprint>(factor.StatId);
                        if (statbp != null)
                        {
                            if (found) this.formula_ += " + "; else found = true;
                            this.formula_ += "(" + factor.Factor + " * target:GetStat" + factor.Target + "('" + statbp.Name + "'))";
                        }
                    }
                    this.formula_ = this.formula_.TrimEnd(' ', '+');
                    if (this.amount_ < 0) this.formula_ += ")";
                    this.amount_ = 0;
                    this.random_ = 0;
                    this.offensiveFactors_ = null;
                    this.defensiveFactors_ = null;
                };
            }

            if (this.formula_ != null)
            {
                Server.Instance.Loaded += delegate()
                {
                    string str = "return " + this.formula_;
                    this.formula_ = null;
                    foreach (Script s in Server.Instance.DataSets[typeof(Script)].Values)
                    {
                        if (s.Code == str)
                        {
                            this.formula_script_id_ = s.Id;
                            return;
                        }
                    }

                    Script script = new Script(Script.Types.StatModifier, "StatMod_" + Guid.NewGuid(), str);
                    Server.Instance.Add<Script>(script);
                    this.formula_script_id_ = script.Id;
                };
            }
        }
    }
}
