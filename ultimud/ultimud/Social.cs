using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Social : IIdentifiable, ICommand
    {
        Guid id_;
        string name_;
        String output_ = new String();
        String targetOutput_ = new String();

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Description("This is the command that a character must enter to perform this social.")]
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = Telnet.Decolorize(value); }
        }

        [Description("This is the text that people will see when this social is performed without a target.")]
        public String Output
        {
            get { return this.output_; }
            set { this.output_ = value; }
        }

        [Description("This is the text that people will see when this social is performed with a target.")]
        public String TargetOutput
        {
            get { return this.targetOutput_; }
            set { this.targetOutput_ = value; }
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
            if (!Server.Instance.DataSets.ContainsKey(typeof(Social)))
                Server.Instance.DataSets.Add(typeof(Social), new Dictionary<Guid, IIdentifiable>());
        }
        
        [Obfuscation(Exclude = false)]
        public void Perform(Character ch, Object target)
        {
            ch.Area.Send((target == null || this.TargetOutput.Empty ? this.Output : this.TargetOutput), ch, target, 0);
        }
        
        [Obfuscation(Exclude = false)]
        public bool ProcessCommand(User user)
        {
            string arg = user.GetArgument();
            this.Perform(user.Character, (string.IsNullOrEmpty(arg) ? null : user.Character.FindNearby<Object>(arg, 0, true)));
            return true;
        }
        
        [Obfuscation(Exclude = false)]
        public ICommand Get(string name)
        {
            return Server.Instance.Get<Social>(name) as ICommand;
        }
    }
}
