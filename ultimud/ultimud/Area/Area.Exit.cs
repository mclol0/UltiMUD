using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Reflection;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Exit : Door, IPortal
        {
            Location destination_ = new Location();
            bool autoEnter_;
            String enterMessage_ = new String("You enter {2}.", "You enter {2}.", null, null, "{1} enters {2}.", null);
            String exitMessage_ = new String(null, null, null, null, "{1} has arrived from {2}.", null);

            #region Properties
            [Description("The destination of this exit.")]
            public Location Destination
            {
                get { return this.destination_; }
                set { this.destination_ = value; }
            }

            [Description("If true, any character that travels over this exit will be automatically transported to the destination. Otherwise, they must enter manually.")]
            [DefaultValue(false)]
            public bool AutoEnter
            {
                get { return this.autoEnter_; }
                set { this.autoEnter_ = value; }
            }

            public String EnterMessage
            {
                get { return this.enterMessage_; }
                set { this.enterMessage_ = value; }
            }
            public String ExitMessage
            {
                get { return this.exitMessage_; }
                set { this.exitMessage_ = value; }
            }

            [Description("Can this area be closed?")]
            [Browsable(true)]
            public new bool Closeable
            {
                get { return base.Closeable; }
                set { base.Closeable = value; }
            }

            [Description("Is the entrance to this area closed?")]
            [Browsable(true)]
            public new bool Closed
            {
                get { return base.Closed; }
                set { base.Closed = value; }
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

            public override int IconIndex
            {
                get { return 4; }
            }
            #endregion

            public Exit()
            {
                this.FillChar = 'X';
                this.FillColor = Telnet.Color.Gray;
            }
            public Exit(Area area)
                : this()
            {
                this.Destination = new Location(area, area.Start);
            }

            [OnDeserialized]
            [Obfuscation(Exclude = false)]
            new internal void OnDeserialized(StreamingContext context)
            {
                base.OnDeserialized(context);
                if (this.enterMessage_ == null)
                    this.enterMessage_ = new String("You enter {2}.", "You enter {2}.", null, null, "{1} enters {2}.", null);
                if (this.exitMessage_ == null)
                    this.exitMessage_ = new String(null, null, null, null, "{1} has arrived from {2}.", null);
            }
        }
    }
}
