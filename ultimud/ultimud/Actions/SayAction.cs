using System;
using System.Drawing;

namespace Ultimud
{
    [Serializable]
    public class SayAction : Action
    {
        string msg_;
        int delay_;
        DateTime startTime_;

        public SayAction(string msg, int delay, Character speaker)
            : base(speaker)
        {
            this.msg_ = msg;
            this.delay_ = delay;
        }

        public override void Start()
        {
            this.startTime_ = DateTime.Now;
        }

        public override bool Stop()
        {
            this.object_.RemoveAction(this);
            return true;
        }

        public override bool Wait()
        {
            this.paused_ = true;
            return true;
        }

        public override void Go()
        {
            this.paused_ = false;
        }

        public override bool Finish()
        {
            Character ch = this.object_ as Character;
            ch.Say(this.msg_);
            this.object_.RemoveAction(this);
            return true;
        }

        public override void Update()
        {
            if (this.paused_)
            {
                return;
            }

            if ((DateTime.Now - this.startTime_).TotalMilliseconds >= this.delay_)
            {
                this.Finish();
            }
        }
    }
}
