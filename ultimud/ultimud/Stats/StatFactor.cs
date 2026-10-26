using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public struct StatFactor
    {
        Guid statId_;
        float factor_;
        StatModifier.ApplyTarget target_;

        #region Properties
        [DisplayName("Stat")]
        [Editor(typeof(StatSelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Stat.IdToNameConverter))]
        public Guid StatId
        {
            get { return this.statId_; }
            set { this.statId_ = value; }
        }

        [DefaultValue(0)]
        public float Factor
        {
            get { return this.factor_; }
            set { this.factor_ = value; }
        }

        [DefaultValue(StatModifier.ApplyTarget.Current)]
        public StatModifier.ApplyTarget Target
        {
            get { return this.target_; }
            set { this.target_ = value; }
        }
        #endregion

        public StatFactor(Guid statId, float factor, StatModifier.ApplyTarget target)
        {
            this.statId_ = statId;
            this.factor_ = factor;
            this.target_ = target;
        }

        public StatFactor(StatFactor other)
        {
            this.statId_ = other.statId_;
            this.factor_ = other.factor_;
            this.target_ = other.target_;
        }

        public StatFactor Clone()
        {
            return new StatFactor(this.statId_, this.factor_, this.target_);
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (this.StatId == Guid.Empty)
            {
                return base.ToString();
            }
            else
            {
                StatBlueprint bp = Server.Instance.Get<StatBlueprint>(this.StatId);
                if (bp == null)
                {
                    return this.StatId.ToString();
                }
                else
                {
                    return bp.Name;
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public long Calculate(Stat stat)
        {
            switch (this.Target)
            {
                case StatModifier.ApplyTarget.Current:
                    return (long)(stat.Current * this.Factor);
                case StatModifier.ApplyTarget.Max:
                    return (long)(stat.Max * this.Factor);
                case StatModifier.ApplyTarget.CurrentAndMax:
                    return (long)((stat.Current * this.Factor) + (stat.Max * this.Factor));
            }
            return 0;
        }
        
        [Obfuscation(Exclude = false)]
        public class CollectionConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                string str = null;

                if (value != null)
                {
                    if (value is List<StatFactor>)
                    {
                        List<StatFactor> list = value as List<StatFactor>;
                        foreach (StatFactor statFactor in list)
                        {
                            StatBlueprint statBlueprint = Server.Instance.Get<StatBlueprint>(statFactor.StatId);
                            if (statBlueprint != null)
                            {
                                str += statBlueprint.Name + " (" + statFactor.Factor + /*(Symbol == null ? "" : Symbol.ToString()) +*/ "), ";
                            }
                        }

                        if (str != null)
                        {
                            str = str.Trim(new char[] { ' ', ',' });
                        }
                    }
                }

                return str;
            }
        }
    }
}
