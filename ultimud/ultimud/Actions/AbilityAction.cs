using System;
using System.Reflection;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class AbilityAction : Action
    {
        Ability ability_;
        Object target_;
        DateTime startTime_;
        Item item_;
        bool executing_;

        public bool Executing
        {
            get { return this.executing_; }
        }
        public Ability Ability
        {
            get { return this.ability_; }
        }
        public Object Target
        {
            get { return this.target_; }
        }
        public Item Item
        {
            get { return this.item_; }
        }

        public AbilityAction(Ability ability, Object actor, Object target, Item item)
            : base(actor)
        {
            this.ability_ = ability;
            this.target_ = target;
            this.item_ = item;
        }

        [Obfuscation(Exclude = false)]
        public override void Start()
        {
            this.object_.Position = Object.Positions.Concentrating;
            this.startTime_ = DateTime.Now;
            if (this.ability_.PrepareMessage != null)
            {
                String prepmsg = this.ability_.PrepareMessage;
                if (this.item_ != null)
                    prepmsg = prepmsg.Replace("{item}", item_.Name);
                this.object_.Area.Send(
                    prepmsg,
                    this.object_,
                    this.target_,
                    0,
                    new Function<bool, IObject>[] { Object.CanHear },
                    new Telnet.Color[] { 
                        Server.Instance.Settings.Colors[Settings.Text.AbilityPrepare1],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityPrepare2],
                        Server.Instance.Settings.Colors[Settings.Text.AbilityPrepare3] });
            }
        }

        [Obfuscation(Exclude = false)]
        public override bool Stop(bool silent)
        {
            if (this.Ability.CanInterrupt)
            {
                this.object_.Position = Object.Positions.Stopped;
                if(!silent && this.object_.Area != null)
                    this.object_.Area.Send(new String("You stop.", "You stop.", null, "{1} stops.", "{1} stops.", null), this.object_, this.target_, 0);
                this.object_.RemoveAction(this);
                return true;
            }
            else
                return false;
        }

        [Obfuscation(Exclude = false)]
        public override bool Wait()
        {
            if (this.Ability.CanInterrupt)
            {
                this.paused_ = true;
                this.object_.Position = Object.Positions.Stopped;
                this.object_.Area.Send(new String("You wait.", "You wait.", "{1} waits.", "{1} waits.", "{1} waits.", null), this.object_, this.target_, 0);
                return true;
            }
            else
                return false;
        }

        [Obfuscation(Exclude = false)]
        public override void Go()
        {
            this.paused_ = false;
            this.object_.Position = Object.Positions.Moving;
            String str = new String(
                "You resume your previous action.",
                "You resume your previous action.",
                "{1} resumes {1:sex(it's,his,her)} previous action.",
                "{1} resumes {1:sex(it's,his,her)} previous action.",
                "{1} resumes {1:sex(it's,his,her)} previous action.",
                null);
            this.object_.Area.Send(str, this.object_, this.target_, 0);
        }

        [Obfuscation(Exclude = false)]
        public override bool Finish()
        {
            this.executing_ = true;
            this.object_.Position = Object.Positions.Stopped;
            bool hit = this.ability_.OnPerform(this.object_, this.target_, false, this.item_);
            this.object_.RemoveAction(this);
            return hit;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if (this.paused_)
                return;

            if ((DateTime.Now - this.startTime_).TotalSeconds >= this.ability_.PrepareTime + (this.item_ == null ? 0 : this.item_.PrepareTime))
                this.Finish();
        }
    }
}
