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

        internal static void PostInit()
        {
            if (Server.Instance == null || !Server.Instance.DataSets.ContainsKey(typeof(Social)))
                return;
            Dictionary<Guid, IIdentifiable> socials = Server.Instance.DataSets[typeof(Social)];
            if (socials.Count > 0)
                return;

            AddDefaultSocial("smile", "You smile happily.", "{1} smiles happily.", "You smile warmly at {2}.", "{1} smiles warmly at you.", "{1} smiles warmly at {2}.");
            AddDefaultSocial("laugh", "You laugh out loud!", "{1} laughs out loud!", "You laugh heartily at {2}.", "{1} laughs heartily at you.", "{1} laughs heartily at {2}.");
            AddDefaultSocial("wave", "You wave.", "{1} waves.", "You wave goodbye to {2}.", "{1} waves goodbye to you.", "{1} waves goodbye to {2}.");
            AddDefaultSocial("bow", "You bow deeply.", "{1} bows deeply.", "You bow respectfully to {2}.", "{1} bows respectfully to you.", "{1} bows respectfully to {2}.");
            AddDefaultSocial("cheer", "You cheer enthusiastically!", "{1} cheers enthusiastically!", "You cheer for {2}!", "{1} cheers for you!", "{1} cheers for {2}!");
            AddDefaultSocial("nod", "You nod solemnly.", "{1} nods solemnly.", "You nod in agreement with {2}.", "{1} nods in agreement with you.", "{1} nods in agreement with {2}.");
            AddDefaultSocial("shrug", "You shrug helplessly.", "{1} shrugs helplessly.", "You shrug at {2}.", "{1} shrugs at you.", "{1} shrugs at {2}.");
            AddDefaultSocial("dance", "You dance a lively jig!", "{1} dances a lively jig!", "You sweep {2} into a lively dance!", "{1} sweeps you into a lively dance!", "{1} sweeps {2} into a lively dance!");
            AddDefaultSocial("wink", "You wink suggestively.", "{1} winks suggestively.", "You wink at {2}.", "{1} winks at you.", "{1} winks at {2}.");
            AddDefaultSocial("grin", "You grin mischievously.", "{1} grins mischievously.", "You grin mischievously at {2}.", "{1} grins mischievously at you.", "{1} grins mischievously at {2}.");
        }

        static void AddDefaultSocial(string name, string selfNoArg, string roomNoArg, string selfTarget, string victimTarget, string roomTarget)
        {
            Social s = new Social();
            s.Id = Guid.NewGuid();
            s.Name = name;
            s.Output = new String(selfNoArg, selfNoArg, null, null, roomNoArg, null);
            s.TargetOutput = new String(selfTarget, selfTarget, victimTarget, victimTarget, roomTarget, null);
            Server.Instance.DataSets[typeof(Social)][s.Id] = s;
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
