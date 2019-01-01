using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class StatusEffectBlueprint : IIdentifiable
    {
        Guid id_;
        string name_;
        String updateMessage_ = new String();
        bool visible_ = true;
        String startMessage_ = new String();
        String endMessage_ = new String();

        #region Properties
        [Browsable(false)]
        [Obfuscation(Exclude = false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Description("The name of this status effect.")]
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = Telnet.Decolorize(value); }
        }

        [Description("The message that is periodically broadcast to the room when something is affected by this status effect.")]
        public String UpdateMessage
        {
            get { return this.updateMessage_; }
            set { this.updateMessage_ = value; }
        }

        [Description("Is this status effect known to players?")]
        [DefaultValue(true)]
        public bool Visible
        {
            get { return this.visible_; }
            set { this.visible_ = value; }
        }

        [Description("The message that is displayed to the room when this status effect starts.")]
        public String StartMessage
        {
            get { return this.startMessage_; }
            set { this.startMessage_ = value; }
        }

        [Description("The message that is displayed to the room when this status effect ends.")]
        public String EndMessage
        {
            get { return this.endMessage_; }
            set { this.endMessage_ = value; }
        }
        #endregion

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(StatusEffectBlueprint)))
                Server.Instance.DataSets.Add(typeof(StatusEffectBlueprint), new Dictionary<Guid, IIdentifiable>());
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return base.ToString();
        }
    }
}
