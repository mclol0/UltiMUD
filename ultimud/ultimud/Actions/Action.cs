using System;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public abstract class Action
    {
        protected Object object_;
        protected DateTime lastUpdate_ = DateTime.Now;
        protected bool paused_;

        protected Action(Object o)
        {
            this.object_ = o;
        }

        [Obfuscation(Exclude = false)]
        public abstract void Start();

        [Obfuscation(Exclude = false)]
        public abstract bool Stop(bool silent);

        [Obfuscation(Exclude = false)]
        public abstract bool Wait();
        
        [Obfuscation(Exclude = false)]
        public abstract void Go();

        [Obfuscation(Exclude = false)]
        public abstract bool Finish();

        [Obfuscation(Exclude = false)]
        public abstract void Update();
    }
}
