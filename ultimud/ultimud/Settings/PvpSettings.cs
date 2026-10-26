using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("Customize player vs. player settings.")]
    [Obfuscation(Exclude = true)]
    public class PvpSettings
    {
        bool allowed_ = true;
        TimeSpan safeTimer_;

        #region Properties
        [Description("Are players allowed to attack other players?")]
        [DefaultValue(true)]
        public bool Allowed
        {
            get { return this.allowed_; }
            set { this.allowed_ = value; }
        }

        [Description("The amount of time a player cannot be attacked after they are killed.")]
        public TimeSpan SafeTimer
        {
            get { return this.safeTimer_; }
            set { this.safeTimer_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }
    }
}
