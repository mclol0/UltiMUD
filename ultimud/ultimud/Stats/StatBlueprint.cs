using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class StatBlueprint : ICloneable, IIdentifiable
    {
        Guid id_;
        string name_;
        long min_;
        long max_;
#pragma warning disable 0169, 0414
        long default_; //Deprecated.
#pragma warning restore 0169, 0414
        int regenFrequency_;
        int regenAmount_;
        List<StatFactor> regenFactors_ = new List<StatFactor>();
        Object.TriggerAction onMin_;
        Object.TriggerAction onMax_;
        bool visible_ = true;
        String warningMessage_ = new String();
        double warningThreshold_ = -25;
        int warningFrequency_ = 20;
        long startMax_;
        long startCurrent_;
        bool canRestore_ = true;
        bool permanent_;

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Category("Appearance")]
        [Description("The name of this stat.")]
        public string Name
        {
            get { return this.name_; }
            set
            {
                if (this.IsPermanent && this.name_.ToLower() != value.ToLower())
                {
                    throw new Exception("You can't change the name of a permanent system stat.");
                }
                this.name_ = Telnet.Decolorize(value);
            }
        }

        [Category("Appearance")]
        [Description("Whether or not this stat is visible by the players.")]
        [DefaultValue(true)]
        public bool Visible
        {
            get { return this.visible_; }
            set { this.visible_ = value; }
        }

        [DefaultValue(0),
        Description("The minimum value of this stat.")]
        public long Min
        {
            get { return min_; }
            set { this.min_ = value; }
        }

        [DefaultValue(0),
        Description("The maximum possible value of this stat.")]
        public long Max
        {
            get { return this.max_; }
            set { this.max_ = value; }
        }

        [DefaultValue(0),
        Description("The amount of time, in seconds, between each regen update.")]
        public int RegenFrequency
        {
            get { return this.regenFrequency_; }
            set { this.regenFrequency_ = value; }
        }

        [DefaultValue(0),
        Description("The amount to modify this stat each regen update.")]
        public int RegenAmount
        {
            get { return this.regenAmount_; }
            set { this.regenAmount_ = value; }
        }

        [Description("The stats that effect the regen amount."),
        TypeConverter(typeof(StatFactor.CollectionConverter))]
        public List<StatFactor> RegenFactors
        {
            get { return this.regenFactors_; }
            set { this.regenFactors_ = value; }
        }

        [Category("Triggers")]
        [Description("The action that will occur when the current stat value reaches the minimum.")]
        [TypeConverter(typeof(Object.TriggerActionConverter))]
        public Object.TriggerAction OnMin
        {
            get { return this.onMin_; }
            set { this.onMin_ = value; }
        }

        [Category("Triggers")]
        [Description("The action that will occur when the current stat value reaches the maximum.")]
        [TypeConverter(typeof(Object.TriggerActionConverter))]
        public Object.TriggerAction OnMax
        {
            get { return this.onMax_; }
            set { this.onMax_ = value; }
        }

        [Description("The message that is periodically broadcast when this stat crosses the warning threshold.")]
        public String WarningMessage
        {
            get
            {
                if (this.warningMessage_ == null)
                    this.warningMessage_ = new String();
                return this.warningMessage_;
            }
            set { this.warningMessage_ = value; }
        }

        [Description("When this stat crosses the warning threshold percentage, WarningMessage will periodically be broadcast. If this value is negative, the threshold is below this percentage. Otherwise, the threshold is above.")]
        [DefaultValue(-25)]
        public double WarningThreshold
        {
            get { return this.warningThreshold_; }
            set { this.warningThreshold_ = value; }
        }

        [Description("The length of time in seconds between each warning message.")]
        [DefaultValue(20)]
        public int WarningFrequency
        {
            get { return this.warningFrequency_; }
            set { this.warningFrequency_ = value; }
        }

        [DefaultValue(0)]
        [Description("The starting value of this stat.")]
        public long StartCurrent
        {
            get { return this.startCurrent_; }
            set { this.startCurrent_ = value; }
        }

        [DefaultValue(0)]
        [Description("The starting max value of this stat.")]
        public long StartMax
        {
            get { return this.startMax_; }
            set { this.startMax_ = value; }
        }

        [DefaultValue(true)]
        [Description("Can this stat be restored to it's maximum value by an automated restore or an ability?")]
        public bool CanRestore
        {
            get { return this.canRestore_; }
            set { this.canRestore_ = value; }
        }

        [Browsable(false)]
        public bool IsPermanent
        {
            get { return this.permanent_; }
            set { this.permanent_ = value; }
        }
        #endregion

        #region ICloneable Members
        [Obfuscation(Exclude = false)]
        public object Clone()
        {
            StatBlueprint clone = new StatBlueprint();
            clone.Name = this.Name;
            clone.Min = this.Min;
            clone.Max = this.Max;
            //clone.Default = this.Default;
            clone.StartMax = this.StartMax;
            clone.StartCurrent = this.StartCurrent;
            clone.RegenFrequency = this.RegenFrequency;
            clone.RegenAmount = this.RegenAmount;
            //clone.RegenFactors = (StatPercentage.Collection)this.RegenFactors.Clone();
            clone.RegenFactors = this.RegenFactors;
            clone.OnMin = this.OnMin;
            clone.OnMax = this.OnMax;
            clone.Visible = this.Visible;
            return clone;
        }
        #endregion

        public StatBlueprint()
        {
        }
        
        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(StatBlueprint)))
                Server.Instance.DataSets.Add(typeof(StatBlueprint), new Dictionary<Guid, IIdentifiable>());

            Object.NewObjectEvent += delegate(Object o) { InitStats(o); };
            Server.Instance.UserLoggedIn += delegate(User user) { InitStats(user.Character); };
        }
        internal static void PostInit()
        {
            StatBlueprint hp = Server.Instance.Get<StatBlueprint>("hp");
            if (hp == null)
            {
                hp = new StatBlueprint();
                hp.Name = "Hp";
                hp.IsPermanent = true;
                hp.Max = 9999;
                hp.StartMax = 100;
                hp.StartCurrent = 100;
                Server.Instance.Add<StatBlueprint>(hp);
            }
            else
            {
                hp.IsPermanent = true;
            }

            StatBlueprint mp = Server.Instance.Get<StatBlueprint>("mp");
            if (mp == null)
            {
                mp = new StatBlueprint();
                mp.Name = "Mp";
                mp.IsPermanent = true;
                mp.Max = 9999;
                mp.StartMax = 100;
                mp.StartCurrent = 100;
                Server.Instance.Add<StatBlueprint>(mp);
            }
            else
            {
                mp.IsPermanent = true;
            }

            StatBlueprint speed = Server.Instance.Get<StatBlueprint>("speed");
            if (speed == null)
            {
                speed = new StatBlueprint();
                speed.Name = "Speed";
                speed.IsPermanent = true;
                speed.Max = 99;
                speed.StartMax = 5;
                speed.StartCurrent = 5;
                Server.Instance.Add<StatBlueprint>(speed);
            }
            else
            {
                speed.IsPermanent = true;
            }

            if (Server.Instance.Settings != null && Server.Instance.Settings.SpeedStat == Guid.Empty)
            {
                Server.Instance.Settings.SpeedStat = speed.Id;
            }

            StatBlueprint sight = Server.Instance.Get<StatBlueprint>("sight");
            if (sight == null)
            {
                sight = new StatBlueprint();
                sight.Name = "Sight";
                sight.IsPermanent = true;
                sight.Max = 99;
                sight.StartMax = 5;
                sight.StartCurrent = 5;
                Server.Instance.Add<StatBlueprint>(sight);
            }
            else
            {
                sight.IsPermanent = true;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public static void InitStats(Object o)
        {
            if (Server.Instance != null)
            {
                Dictionary<Guid, IIdentifiable> dict;
                if (Server.Instance.DataSets.TryGetValue(typeof(StatBlueprint), out dict))
                {
                    lock (o.Stats)
                    {
                        foreach (StatBlueprint stat in dict.Values)
                        {
                            if (!o.Stats.ContainsKey(stat.Id))
                            {
                                o.SetStat(stat.Id, stat.StartCurrent, stat.StartMax);
                            }
                        }
                    }
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return this.Name;
            }
            return base.ToString();
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            if (this.default_ > 0 && this.startCurrent_ == 0 && this.startMax_ == 0)
            {
                this.startCurrent_ = this.default_;
                this.startMax_ = this.default_;
            }
            this.default_ = 0;
        }
    }
}
