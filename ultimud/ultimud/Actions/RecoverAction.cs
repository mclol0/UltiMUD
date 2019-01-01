using System;
using System.Reflection;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class RecoverAction : Action
    {
        int timer_;
        DateTime startTime_;
        bool silent_;

        [NonSerialized]
        Function_void executeOnRecover_;

        public RecoverAction(Object actor, int seconds, bool silent)
            : base(actor)
        {
            this.timer_ = seconds;
            this.silent_ = silent;
        }
        public RecoverAction(Object actor, int seconds, bool silent, Function_void executeOnRecover)
            : base(actor)
        {
            this.timer_ = seconds;
            this.silent_ = silent;
            this.executeOnRecover_ = executeOnRecover;
        }

        [Obfuscation(Exclude = false)]
        public override void Start()
        {
            this.object_.Position = Object.Positions.Recovering;
            this.startTime_ = DateTime.Now;
        }

        [Obfuscation(Exclude = false)]
        public override bool Stop(bool silent)
        {
            return false;
        }

        [Obfuscation(Exclude = false)]
        public override bool Wait()
        {
            return false;
        }

        [Obfuscation(Exclude = false)]
        public override void Go()
        {
        }

        [Obfuscation(Exclude = false)]
        public override bool Finish()
        {
            this.object_.RemoveAction(this);
            this.object_.Position = Object.Positions.Stopped;
            if (!this.silent_)
            {
                String str = new String("You have recovered.", "You have recovered.", "{1} has recovered.", "{1} has recovered.", "{1} has recovered.", null);
                this.object_.Area.Send(str, this.object_, null, 0);

                if (this.executeOnRecover_ != null)
                    this.executeOnRecover_();

                Server.Instance.FlushOutput();
            }
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if ((DateTime.Now - startTime_).TotalSeconds >= this.timer_)
                this.Finish();
        }
    }
}
