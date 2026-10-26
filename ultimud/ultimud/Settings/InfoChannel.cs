using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public class InfoChannel
    {
        public enum Event
        {
            Login,
            Logout,
            NewCharacter,
            Deleted,
            MessagePosted
            //Death,
            //LevelUp,
        }

        BitArray events_ = new BitArray(32);
        string output_ = "{color:brightblue}[{color:brightgreen}INFO{color:brightblue}] {color:reset}{msg}";

        #region Properties
        [Description("The events to register with the info channel.")]
        [Editor(typeof(FlagEditor<Event>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<Event>))]
        public BitArray Events
        {
            get { return this.events_; }
            set { this.events_ = value; }
        }

        [Description("The look of the info messages.")]
        public string Output
        {
            get { return this.output_; }
            set { this.output_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }

        public void Broadcast(Event ev, string str)
        {
            if (this.Events.Get((int)ev) && Server.Instance != null && Server.Instance.Users != null)
            {
                string msg = this.Output.Replace("{msg}", str);
                foreach (User user in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
                {
                    if (user != null && user.Flags.Get((int)User.UserFlags.LoggedIn))
                    {
                        user.Send(msg);
                    }
                }
            }
        }
    }
}
