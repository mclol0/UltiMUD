using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Channel : IIdentifiable, ICommand
    {
        public enum Targets
        {
            None,
            Player,
            Npc,
            Both
        }

        Guid id_;
        string name_;
        String output_ = new String();
        List<Guid> raceRequirements_ = new List<Guid>();
        List<Guid> jobRequirements_ = new List<Guid>();
        List<Guid> abilityRequirements_ = new List<Guid>();
        Targets target_ = Targets.None;
        Area.Range range_ = Area.Range.World;
        List<Guid> itemrequirements_= new List<Guid>();
        bool canTurnOff_ = true;

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Description("The name of this channel.")]
        public string Name
        {
            get { return this.name_; }
            set
            {
                this.name_ = Telnet.Decolorize(value);
                if (this.Output.Empty)
                    this.Output = new String("You [" + this.name_ + "]: {msg}", "You [" + this.name_ + "]: {msg}", null, null, null, null);
            }
        }

        [Description(@"The output of this channel.
  {1} - The name of the speaker.
  {2} - The name of the target.
  {msg} - The message.")]
        public String Output
        {
            get { return this.output_; }
            set { this.output_ = value; }
        }

        [Category("Requirements")]
        [Description("The races that can hear and use this channel.")]
        [Editor(typeof(Server.MultiObjectSelector<Race>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Race>))]
        public List<Guid> RaceRequirements
        {
            get { return this.raceRequirements_; }
            set { this.raceRequirements_ = value; }
        }

        [Category("Requirements")]
        [Description("The jobs that can hear and use this channel.")]
        [Editor(typeof(Server.MultiObjectSelector<Job>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Job>))]
        public List<Guid> JobRequirements
        {
            get { return this.jobRequirements_; }
            set { this.jobRequirements_ = value; }
        }

        [Category("Requirements")]
        [Description("The abilities that must be known in order to use and hear this channel.")]
        [Editor(typeof(Server.MultiObjectSelector<Ability>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public List<Guid> AbilityRequirements
        {
            get { return this.abilityRequirements_; }
            set { this.abilityRequirements_ = value; }
        }

        [Category("Requirements")]
        [Description("The items that must be equipped in order to use and hear this channel.")]
        [Editor(typeof(Server.MultiObjectSelector<Item>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Item>))]
        public List<Guid> ItemRequirements
        {
            get { return this.itemrequirements_; }
            set { this.itemrequirements_ = value; }
        }

        [DefaultValue(Targets.None)]
        public Targets Target
        {
            get { return this.target_; }
            set { this.target_ = value; }
        }

        [DefaultValue(Area.Range.World)]
        public Area.Range Range
        {
            get { return this.range_; }
            set { this.range_ = value; }
        }

        [DefaultValue(true)]
        [Description("Can this channel be turned off/on by players?")]
        public bool CanTurnOff
        {
            get { return this.canTurnOff_; }
            set { this.canTurnOff_ = value; }
        }
        #endregion

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return (!string.IsNullOrEmpty(this.Name) ? this.Name : base.ToString());
        }

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Channel)))
                Server.Instance.DataSets.Add(typeof(Channel), new Dictionary<Guid, IIdentifiable>());
        }
        
        bool CheckRequirements(IObject o)
        {
            Character ch = o as Character;
            if (ch != null)
            {
                if ((this.RaceRequirements != null && this.RaceRequirements.Count > 0 && !this.RaceRequirements.Contains(ch.RaceId))
                    || (this.JobRequirements != null && this.JobRequirements.Count > 0 && !this.JobRequirements.Contains(ch.JobId)))
                    return false;

                if (this.AbilityRequirements != null)
                    foreach (Guid abilityId in this.AbilityRequirements)
                        if (!ch.Abilities.ContainsKey(abilityId))
                            return false;

                if (!this.CheckItemRequirements(ch, this.itemrequirements_))
                    return false;

                if (ch.User != null && !ch.User.GetChannelSetting(this))
                    return false;

                return true;
            }
            return false;
        }
        
        [Obfuscation(Exclude = false)]
        public bool ProcessCommand(User user)
        {
            if (!this.CheckRequirements(user.Character))
            {
                user.Send("You can't use that channel.");
                return true;
            }
            if (user.Flags.Get((int)User.UserFlags.Silenced))
            {
                user.Send("You have been silenced.");
                return true;
            }

            user.InputHandler = this.ProcessCommand;

            if (this.target_ == Targets.None)
            {
                if (Command.GetArguments(user, User.EnterMessage))
                {
                    if (!user.GetChannelSetting(this))
                        user.SetChannel(this, true, false);
                    this.Send(this.output_.Replace("{msg}", user.GetCommandArg<string>(0)), this.range_, user.Character, null, user.Character.Area, user);
                    return true;
                }
            }
            else
            {
                Server.InputHandler targetselector = null;
                switch (this.target_)
                {
                    case Targets.Both:
                        switch (this.range_)
                        {
                            case Area.Range.Area: targetselector = User.SelectObjectArea<Character>; break;
                            case Area.Range.Far: targetselector = User.SelectObjectFar<Character>; break;
                            case Area.Range.Nearby: targetselector = User.SelectObjectNearby<Character>; break;
                            case Area.Range.Room: targetselector = User.SelectObjectRoom<Character>; break;
                            case Area.Range.World: targetselector = User.SelectObjectWorld<Character>; break;
                        }
                        break;
                    case Targets.Npc:
                        switch (this.range_)
                        {
                            case Area.Range.Area: targetselector = User.SelectObjectArea<Npc>; break;
                            case Area.Range.Far: targetselector = User.SelectObjectFar<Npc>; break;
                            case Area.Range.Nearby: targetselector = User.SelectObjectNearby<Npc>; break;
                            case Area.Range.Room: targetselector = User.SelectObjectRoom<Npc>; break;
                            case Area.Range.World: targetselector = User.SelectObjectWorld<Npc>; break;
                        }
                        break;
                    case Targets.Player:
                        switch (this.range_)
                        {
                            case Area.Range.Area: targetselector = User.SelectObjectArea<Player>; break;
                            case Area.Range.Far: targetselector = User.SelectObjectFar<Player>; break;
                            case Area.Range.Nearby: targetselector = User.SelectObjectNearby<Player>; break;
                            case Area.Range.Room: targetselector = User.SelectObjectRoom<Player>; break;
                            case Area.Range.World: targetselector = User.SelectObjectWorld<Player>; break;
                        }
                        break;
                }

                if (Command.GetArguments(user, targetselector, User.EnterMessage))
                {
                    Object target = user.GetCommandArg<Object>(0);
                    if (target == null)
                        user.Send("Invalid target.");
                    else
                    {
                        if (!user.GetChannelSetting(this))
                            user.SetChannel(this, true, false);
                        this.Send(this.output_.Replace("{msg}", user.GetCommandArg<string>(1)), this.range_, user.Character, target, user.Character.Area, user);
                    }
                    return true;
                }
            }

            return false;
        }
        
        [Obfuscation(Exclude = false)]
        void Send(String str, Area.Range range, Object o1, Object o2, Area area, User user)
        {
            switch (range)
            {
                case Area.Range.Area: if (area != null) area.Send(str, o1, o2, -1, new Function<bool, IObject>[] { this.CheckRequirements }, false); break;
                case Area.Range.Far: if (area != null) area.Send(str, o1, o2, (int)range, new Function<bool, IObject>[] { this.CheckRequirements }, false); break;
                case Area.Range.Nearby: if (area != null) area.Send(str, o1, o2, (int)range, new Function<bool, IObject>[] { this.CheckRequirements }, false); break;
                case Area.Range.Room: if (area != null) area.Send(str, o1, o2, 0, new Function<bool, IObject>[] { this.CheckRequirements }, false); break;
                case Area.Range.Self: o1.Send(str.ToFirstPerson(o1, o2)); break;
                case Area.Range.World:
                    if (o1 != null)
                        o1.Send(String.Format(str.ToFirstPerson(o1, o2)));
                    if (o2 != null)
                    {
                        if (!this.CheckRequirements(o2))
                            user.Send(String.Capitalize(o2.Name) + " can't hear you.");
                        else
                        {
                            if (o2.Location.Equals(o1.Location))
                                o2.Send(String.Format(str.ToSecondPersonAtFirstPerson(o1, o2)));
                            else
                                o2.Send(String.Format(str.ToSecondPerson(o1, o2)));
                        }
                    }

                    string str1 = String.Format(str.ToThirdPersonAtFirstPerson(o1, o2));
                    string str2 = String.Format(str.ToThirdPersonAtSecondPerson(o1, o2));
                    string str3 = String.Format(str.ToThirdPerson(o1, o2));
                    if (!string.IsNullOrEmpty(str1) || !string.IsNullOrEmpty(str2) || !string.IsNullOrEmpty(str3))
                    {
                        foreach (User u in Damdai.Collections.Util.CopyList<User>(Server.Instance.Users))
                        {
                            if (u.Character != null && u.Flags.Get((int)User.UserFlags.LoggedIn) && u.Character != o1 && u.Character != o2 && u.GetChannelSetting(this))
                            {
                                if (o1 != null && u.Character.Location.Equals(o1.Location))
                                    u.Send(str1);
                                else if (o2 != null && u.Character.Location.Equals(o2.Location))
                                    u.Send(str2);
                                else
                                    u.Send(str3);
                            }
                        }
                    }
                    break;
            }
        }
        
        [Obfuscation(Exclude = false)]
        public ICommand Get(string name)
        {
            return Server.Instance.Get<Channel>(name) as ICommand;
        }
        
        [Obfuscation(Exclude = false)]
        bool CheckItemRequirements(Character ch, List<Guid> items)
        {
            if (items == null)
                return true;
            else if (items.TrueForAll(delegate(Guid id)
            {
                foreach (Item item in ch.Equipment.Values)
                    if (item.BlueprintId == id)
                        return true;
                return false;
            }))
                return true;
            else
                return false;
        }
    }
}
