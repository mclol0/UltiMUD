using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Drawing.Design;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class TextTrigger
    {
        public enum BusyActions
        {
            Ignore,
            Enqueue,
            Interrupt
        }

        string trigger_;
        string response_;
        Regex regex_;
        int delay_; //Deprecated.
        List<Guid> raceRequirements_ = new List<Guid>();
        List<Guid> jobRequirements_ = new List<Guid>();
        List<Guid> abilityRequirements_ = new List<Guid>();
        BusyActions busyaction_ = BusyActions.Ignore;
        //List<Stat> statRequirements_ = new List<Stat>();

        #region Properties
        [Description("A regex string that the NPC must see before responding.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Trigger
        {
            get { return this.trigger_; }
            set
            {
                this.trigger_ = value;
                this.regex_ = new Regex(value, RegexOptions.IgnoreCase);
            }
        }

        [Description("The command that the NPC will perform when the trigger is matched. Commands can be separated by ';'.")]
        [Editor(typeof(StringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Response
        {
            get { return this.response_; }
            set { this.response_ = value; }
        }

        [Description("What race must the character be in order to elicit a response? If no requirement, select none.")]
        [Editor(typeof(Server.MultiObjectSelector<Race>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Race>))]
        public List<Guid> RaceRequirements
        {
            get { return this.raceRequirements_; }
            set { this.raceRequirements_ = value; }
        }

        [Description("What job must the character be in order to elicit a response? If no requirement, select none.")]
        [Editor(typeof(Server.MultiObjectSelector<Job>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Job>))]
        public List<Guid> JobRequirements
        {
            get { return this.jobRequirements_; }
            set { this.jobRequirements_ = value; }
        }

        [Description("What abilities must the character know in order to elicit a response? If no requirement, select none.")]
        [Editor(typeof(Server.MultiObjectSelector<Ability>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public List<Guid> AbilityRequirements
        {
            get { return this.abilityRequirements_; }
            set { this.abilityRequirements_ = value; }
        }

        [DefaultValue(BusyActions.Ignore)]
        [Description("What to do if the npc is currently busy performing another action.")]
        public BusyActions BusyAction
        {
            get { return this.busyaction_; }
            set { this.busyaction_ = value; }
        }
        #endregion

        public TextTrigger() { }
        public TextTrigger(string trigger, string response)
        {
            this.Trigger = trigger;
            this.Response = response;
        }

        [Obfuscation(Exclude = false)]
        public bool IsMatch(string str, IObject actor)
        {
            Character ch = actor as Character;
            if (ch != null)
            {
                if ((this.RaceRequirements != null && this.RaceRequirements.Count > 0 && !this.RaceRequirements.Contains(ch.RaceId)) || (this.JobRequirements != null && this.JobRequirements.Count > 0 && !this.JobRequirements.Contains(ch.JobId)))
                    return false;

                if (this.AbilityRequirements != null)
                {
                    foreach (Guid abilityId in this.AbilityRequirements)
                        if (!ch.Abilities.ContainsKey(abilityId))
                            return false;
                }
            }

            return (!string.IsNullOrEmpty(str) && this.regex_ != null && this.regex_.IsMatch(str));
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            this.delay_ = 0;
        }
    }
}
