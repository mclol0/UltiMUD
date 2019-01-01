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
    public class TextTrigger : MarshalByRefObject
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
        List<IRace> racerequirements_ = new List<IRace>();
        List<IJob> jobrequirements_ = new List<IJob>();
        List<IAbility> abilityrequirements_ = new List<IAbility>();
        BusyActions busyaction_ = BusyActions.Ignore;

        #region Properties
        [Description("A regex string that the NPC must see before responding.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(UITypeEditor))]
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
        //[Editor(typeof(StringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Response
        {
            get { return this.response_; }
            set { this.response_ = value; }
        }

        [Description("What race must the character be in order to elicit a response? If no requirement, select none.")]
        //[Editor(typeof(BlueprintsSelector<IRace>), typeof(UITypeEditor))]
        //[TypeConverter(typeof(BlueprintsToNameConverter<IRace>))]
        public List<IRace> RaceRequirements
        {
            get { return this.racerequirements_; }
            set { this.racerequirements_ = value; }
        }

        [Description("What job must the character be in order to elicit a response? If no requirement, select none.")]
        //[Editor(typeof(BlueprintsSelector<IJob>), typeof(UITypeEditor))]
        //[TypeConverter(typeof(BlueprintsToNameConverter<IJob>))]
        public List<IJob> JobRequirements
        {
            get { return this.jobrequirements_; }
            set { this.jobrequirements_ = value; }
        }

        [Description("What abilities must the character know in order to elicit a response? If no requirement, select none.")]
        //[Editor(typeof(BlueprintsSelector<IAbility>), typeof(UITypeEditor))]
        //[TypeConverter(typeof(BlueprintsToNameConverter<IAbility>))]
        public List<IAbility> AbilityRequirements
        {
            get { return this.abilityrequirements_; }
            set { this.abilityrequirements_ = value; }
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
            ICharacter ch = actor as ICharacter;
            if (ch != null)
            {
                if (!this.RaceRequirements.Contains(ch.Race) || !this.JobRequirements.Contains(ch.Job))
                    return false;

                foreach (IAbility a in this.AbilityRequirements)
                    if (!ch.HasAbility(a))
                        return false;
            }

            return (!string.IsNullOrEmpty(str) && this.regex_ != null && this.regex_.IsMatch(str));
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
