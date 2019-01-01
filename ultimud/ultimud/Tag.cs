using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Drawing.Design;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Tag : IIdentifiable
    {
        Guid id_;
        string name_;

        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = value; }
        }

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Tag)))
                Server.Instance.DataSets.Add(typeof(Tag), new Dictionary<Guid, IIdentifiable>());
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return (string.IsNullOrEmpty(this.name_) ? base.ToString() : this.name_);
        }
    }
}
