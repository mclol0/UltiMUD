using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using Damdai;
using Damdai.TypeConverters;

namespace Ultimud
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("Customize character level-up settings.")]
    [Obfuscation(Exclude = true)]
    public class LevelUpSettings
    {
        String message_ = new String("You raise a level!", null, null, null, "{1} raises a level!", null, "{1} raises a level!");
        Telnet.Color color_ = Telnet.Color.BrightGreen;
        List<StatModifier> statMods_ = new List<StatModifier>();
        bool restore_;
        Area.Range broadcastRange_ = Area.Range.World;

        #region Properties
        [Description("The message that is displayed when a player levels up.")]
        public String Message
        {
            get { return this.message_; }
            set { this.message_ = value; }
        }

        [Description("The color of the level up message.")]
        [DefaultValue(Telnet.Color.BrightGreen)]
        public Telnet.Color Color
        {
            get { return this.color_; }
            set { this.color_ = value; }
        }

        [Description("The character's stats to modify when they level up. This will be applied to all characters, regardless of race or job.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> StatModifications
        {
            get { return this.statMods_; }
            set { this.statMods_ = value; }
        }

        [Description("Should characters be restored when they level up?")]
        [DefaultValue(false)]
        public bool Restore
        {
            get { return this.restore_; }
            set { this.restore_ = value; }
        }

        [Description("Should the level up message be broadcast to the world, area, room, etc.?")]
        [DefaultValue(Area.Range.World)]
        public Area.Range BroadcastRange
        {
            get { return this.broadcastRange_; }
            set { this.broadcastRange_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public void Apply(Object o)
        {
            if (!(o is Npc))
            {
                Telnet.Color[] colors = new Telnet.Color[3] { this.Color, this.Color, this.Color };
                switch (this.broadcastRange_)
                {
                    case Area.Range.Self:
                        o.Send(this.Message.ToFirstPerson(this.Color, o, null));
                        break;
                    case Area.Range.Room:
                        o.Area.Send(this.Message, o, null, 0, new Function<bool, IObject>[] { Object.CanHear }, colors);
                        break;
                    case Area.Range.Nearby:
                        o.Area.Send(this.Message, o, null, (int)Area.Range.Nearby, new Function<bool, IObject>[] { Object.CanHear }, colors);
                        break;
                    case Area.Range.Far:
                        o.Area.Send(this.Message, o, null, (int)Area.Range.Far, new Function<bool, IObject>[] { Object.CanHear }, colors);
                        break;
                    case Area.Range.Area:
                        o.Area.Send(this.Message, o, null, -1, new Function<bool, IObject>[] { Object.CanHear }, colors);
                        break;
                    case Area.Range.World:
                        o.Send(this.Message.ToFirstPerson(this.Color, o, null));
                        Server.Instance.Echo(this.Message.ToThirdPersonAtFirstPerson(this.Color, o, null), o);
                        break;
                }
            }

            StatModifier.Apply(this.StatModifications, o, o, o, null, false);
            if (this.Restore)
                o.Restore(false);
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return null;
        }
    }
}
