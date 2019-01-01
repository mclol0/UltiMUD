using System;
using System.ComponentModel;
using System.Reflection;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Door : Wall, ICloseable
        {
            #region Properties
            [Description("Is the entrance to this area closed?")]
            [Browsable(true)]
            public new bool Closed
            {
                get { return base.Closed; }
                set
                {
                    base.Closed = value;
                    this.LayerFlags.Set((int)LayerFlag.NoWalk, value);
                    this.LayerFlags.Set((int)LayerFlag.NoFly, value);
                }
            }

            [Description("Is the entrance to this area locked?")]
            [Browsable(true)]
            public new bool Locked
            {
                get { return base.Locked; }
                set { base.Locked = value; }
            }

            [Description("The item that is required to lock and unlock the entrance to this area.")]
            [Browsable(true)]
            public new Item Key
            {
                get { return base.Key; }
                set { base.Key = value; }
            }

            public override int Height
            {
                get { return (!this.Closed ? 0 : base.Height); }
                set { base.Height = value; }
            }

            [Browsable(false)]
            public override int IconIndex
            {
                get { return 9; }
            }
            #endregion

            public Door()
            {
                this.Closeable = true;
                this.Closed = false;
                this.FillChar = '=';
                this.FillColor = Telnet.Color.Gray;
            }
        }
    }
}
