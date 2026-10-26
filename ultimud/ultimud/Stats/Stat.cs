using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Stat : ICloneable
    {
        Guid id_;
        long current_;
        long max_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        DateTime lastUpdate_;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized]
        DateTime lastBroadcast_;

        #region Properties
        [DisplayName("Stat")]
        [Editor(typeof(StatSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Stat.IdToNameConverter))]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                StatBlueprint bp = this.Blueprint;
                if (bp != null)
                {
                    return bp.Name;
                }
                else
                {
                    return null;
                }
            }
        }

        public long Current
        {
            get { return this.current_; }
            set
            {
                StatBlueprint bp = this.Blueprint;
                if (bp != null)
                {
                    this.current_ = value;
                    this.current_ = Math.Min(this.current_, this.Max);
                    this.current_ = Math.Max(this.current_, bp.Min);
                }
            }
        }
        public long Max
        {
            get { return this.max_; }
            set
            {
                StatBlueprint bp = this.Blueprint;
                if (bp != null)
                {
                    this.max_ = value;
                    this.max_ = Math.Min(this.max_, bp.Max);
                    this.max_ = Math.Max(this.max_, bp.Min);
                }
            }
        }

        [Browsable(false)]
        public StatBlueprint Blueprint
        {
            get { return Server.Instance.Get<StatBlueprint>(this.Id); }
        }

        [Browsable(false)]
        public double Percent
        {
            get {
                if (this.Max < 1)
                {
                    return 0;
                }
                return (this.Current * 100 / this.Max);
            }
        }
        #endregion

        public Stat()
        {
        }
        public Stat(Guid id)
        {
            this.id_ = id;
        }
        
        [Obfuscation(Exclude = false)]
        public void Update(Object o)
        {
            StatBlueprint blueprint = this.Blueprint;
            if (blueprint != null)
            {
                DateTime now = DateTime.Now;
                if (o.CanRegen && (now - this.lastUpdate_).TotalSeconds >= blueprint.RegenFrequency)
                {
                    if (this.Current < this.Max)
                    {
                        int amount = blueprint.RegenAmount;
                        if (blueprint.RegenFactors != null && blueprint.RegenFactors.Count > 0)
                        {
                            foreach (StatFactor statFactor in blueprint.RegenFactors)
                            {
                                Stat stat = o.GetStat(statFactor.StatId);
                                if (stat != null)
                                    amount += (int)statFactor.Calculate(stat);
                            }
                        }

                        this.Current += amount;
                    }

                    this.lastUpdate_ = now;
                }

                if (o is Character && (DateTime.Now - this.lastBroadcast_).TotalSeconds > blueprint.WarningFrequency)
                    this.BroadcastWarningMessage(o);

                this.CheckTriggers(o, null);
            }
        }
        
        [Obfuscation(Exclude = false)]
        public void CheckTriggers(Object self, Object other)
        {
            //this.Normalize();
            StatBlueprint blueprint = this.Blueprint;
            if (blueprint != null && this.max_ != blueprint.Min)
            {
                if (this.Current <= blueprint.Min && blueprint.OnMin != null)
                {
                    blueprint.OnMin(self, other);
                    Server.Instance.FlushOutput();
                }
                else if (this.Current >= this.Max && blueprint.OnMax != null)
                    blueprint.OnMax(self, other);
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
        
        [Obfuscation(Exclude = false)]
        public class IdToNameConverter : StringConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (value is Guid)
                {
                    StatBlueprint stat = Server.Instance.Get<StatBlueprint>((Guid)value);
                    if (stat != null)
                    {
                        return stat.Name;
                    }
                }

                return string.Empty;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
        [Obfuscation(Exclude = false)]
        public static string ResolveVars(string str, ICollection<Stat> stats)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            Dictionary<string, Stat> dict = new Dictionary<string, Stat>();
            foreach (Stat stat in stats)
                if (!string.IsNullOrEmpty(stat.Name))
                    dict.Add(stat.Name, stat);

            MatchCollection matches = Regex.Matches(str, @"\{stat:(?<stat>[a-zA-Z ]+)(\.(?<value>[a-z]+))?(,(?<width>([\-0-9]+)))?\}");
            foreach (Match match in matches)
            {
                string statName = match.Groups["stat"].Value;
                string value = match.Groups["value"].Value;
                string width = match.Groups["width"].Value;
                Stat stat = null;
                if (dict.TryGetValue(statName, out stat))
                {
                    if (string.IsNullOrEmpty(value))
                        value = stat.Name + ": " + stat.Current.ToString(Server.Instance.Settings.NumberFormat) + "/" + stat.Max.ToString(Server.Instance.Settings.NumberFormat);
                    else if (value == "current")
                        value = stat.Current.ToString(Server.Instance.Settings.NumberFormat);
                    else if (value == "max")
                        value = stat.Max.ToString(Server.Instance.Settings.NumberFormat);
                    else if (value == "percent")
                        value = stat.Percent + "%";
                    else if (value == "meter")
                        value = stat.Meter('*');

                    str = str.Replace(match.Value, string.Format("{0" + (string.IsNullOrEmpty(width) ? "" : ("," + width)) + "}", value));
                }
                else
                    str = str.Replace(match.Value, null);
            }

            return str;
        }
        
        [Obfuscation(Exclude = false)]
        void BroadcastWarningMessage(Object o)
        {
            if (o.Area == null)
                return;

            StatBlueprint bp = this.Blueprint;
            if (bp != null && bp.WarningMessage != null && !bp.WarningMessage.Empty)
            {
                double percent = this.Percent;
                if ((bp.WarningThreshold < 0 && percent < Math.Abs(bp.WarningThreshold)) ||
                    (bp.WarningThreshold > 0 && percent > bp.WarningThreshold))
                {
                    o.Area.Send(bp.WarningMessage, o, null, 0);
                }
            }

            this.lastBroadcast_ = DateTime.Now;
        }
        
        public string Meter(char c)
        {
            int length = (int)(this.Percent / 10);
            return String.PrintLine(c, length) + String.PrintLine(' ', 10 - length);
        }        

        public string Meter(string str)
        {
            int length = (int)(this.Percent / 10);
            return String.PrintLine(str, length) + String.PrintLine(' ', 10 - length);
        }
        
        [Obfuscation(Exclude = false)]
        public static Stat operator /(Stat o, int x)
        {
            Stat stat = o.Clone() as Stat;
            stat.max_ /= x;
            stat.current_ /= x;
            return stat;
        }
    }
}
