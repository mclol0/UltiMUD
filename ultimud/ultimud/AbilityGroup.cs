using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class AbilityGroup : IIdentifiable
    {
        Guid id_;
        string name_;

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Description("The name of this ability group.")]
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = Telnet.Decolorize(value); }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return base.ToString();
        }

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(AbilityGroup)))
                Server.Instance.DataSets.Add(typeof(AbilityGroup), new Dictionary<Guid, IIdentifiable>());
        }
    }
}
