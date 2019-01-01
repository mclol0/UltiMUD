using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Ultimud.Actions
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class DelayAction : Action
    {
        Damdai.Timer delay_ = new Damdai.Timer();

        public DelayAction(Object parent, TimeSpan delay)
            : base(parent)
        {
            this.delay_ = new Damdai.Timer(delay);
        }

        [Obfuscation(Exclude = false)]
        public override void Start()
        {
            this.delay_.Reset();
        }

        [Obfuscation(Exclude = false)]
        public override bool Stop(bool silent)
        {
            this.object_.RemoveAction(this);
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override bool Wait()
        {
            this.paused_ = true;
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Go()
        {
            this.paused_ = false;
        }

        [Obfuscation(Exclude = false)]
        public override bool Finish()
        {
            this.object_.RemoveAction(this);
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if (this.paused_)
                return;
            if (!this.delay_.Update())
                this.Finish();
        }
    }
}
