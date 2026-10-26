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
                this.MapSymbol = !string.IsNullOrEmpty(value) && value.Length >= 2 ? value.Substring(0, 2).ToUpper() : (value != null ? value.ToUpper() : string.Empty);
            }
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal new void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);
            this.MapSymbol = !string.IsNullOrEmpty(this.Name) && this.Name.Length >= 2 ? this.Name.Substring(0, 2).ToUpper() : (this.Name != null ? this.Name.ToUpper() : string.Empty);
            this.MapColor = Telnet.Color.BrightRed;
        }
    }
}
