using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Player : Character
    {
        public new string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                this.MapSymbol = value.Substring(0, 2).ToUpper();
            }
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal new void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);
            this.MapSymbol = this.Name.Substring(0, 2).ToUpper();
            this.MapColor = Telnet.Color.BrightRed;
        }
    }
}
