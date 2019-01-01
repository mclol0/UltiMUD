using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using System.Drawing;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Character : Object
    {
        public enum BodyParts
        {
            None,
            Head,
            Face,
            Ears,
            Neck,
            Shoulders,
            Body,
            Back,
            Arms,
            Elbows,
            Wrists,
            Hands,
            Held,
            Fingers,
            Waist,
            Legs,
            Knees,
            Feet
        }

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        User user_;

        [Obfuscation(Exclude = false)]
        [NonSerialized]
        Timer safeTimer_;
        
        [Obfuscation(Exclude = false)]
        [NonSerialized] 
        Character master_;
        
        Dictionary<Guid, int> abilities_ = new Dictionary<Guid, int>();
        Dictionary<BodyParts, Item> equipment_ = new Dictionary<BodyParts, Item>();
        Guid raceId_;
        Guid jobId_;
        Guid masterid_;

        #region Properties
        [Browsable(false)]
        public User User
        {
            get { return this.user_; }
            set { this.user_ = value; }
        }

        [Description("The abilities that this character knows.")]
        [Editor(typeof(CharacterAbilitySelector), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public Dictionary<Guid, int> Abilities
        {
            get { return this.abilities_; }
            set { this.abilities_ = value; }
        }

        [Browsable(false)]
        public bool Recovering
        {
            get
            {
                Action action = this.CurrentAction;
                return (action != null && action is RecoverAction);
            }
        }

        [Browsable(false)]
        public override int IconIndex
        {
            get
            {
                return 5;
            }
        }

        [Description("The items that this character is wearing.")]
        [Browsable(false)]
        public Dictionary<BodyParts, Item> Equipment
        {
            get
            {
                if (this.equipment_ == null)
                {
                    this.equipment_ = new Dictionary<BodyParts, Item>();
                }
                return this.equipment_;
            }
            set { this.equipment_ = value; }
        }

        [DisplayName("Race")]
        [Description("This characters race.")]
        [Editor(typeof(Server.SingleObjectSelector<Race>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Race>))]
        public Guid RaceId
        {
            get { return this.raceId_; }
            set { this.raceId_ = value; }
        }

        [Browsable(false)]
        public Race Race
        {
            get
            {
                IIdentifiable race = null;
                Server.Instance.DataSets[typeof(Race)].TryGetValue(this.raceId_, out race);
                return race as Race;
            }
            set { this.raceId_ = value.Id; }
        }

        [DisplayName("Job")]
        [Description("This characters job.")]
        [Editor(typeof(Server.SingleObjectSelector<Job>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.IdToNameConverter<Job>))]
        public Guid JobId
        {
            get { return this.jobId_; }
            set { this.jobId_ = value; }
        }

        [Browsable(false)]
        public Job Job
        {
            get
            {
                IIdentifiable job = null;
                Server.Instance.DataSets[typeof(Job)].TryGetValue(this.jobId_, out job);
                return job as Job;
            }
            set { this.jobId_ = value.Id; }
        }

        [Description("The items that this character is carrying.")]
        [Browsable(false)]
        public List<Item> Inventory
        {
            get
            {
                List<Item> items = new List<Item>();
                foreach (Item item in this.GetChildren<Item>())
                {
                    if (this.GetEquippedItem(item.EquipLocation) != item)
                    {
                        items.Add(item);
                    }
                }
                return items;
            }
        }

        [Browsable(false)]
        public new int RemainingSpace
        {
            get { return base.RemainingSpace; }
        }

        [Browsable(false)]
        public Timer SafeTimer
        {
            get { return this.safeTimer_; }
            set { this.safeTimer_ = value; }
        }

        [ReadOnly(true)]
        public Character Master
        {
            get
            {
                if (this.master_ == null && this.masterid_ != Guid.Empty)
                {
                    this.master_ = Server.Instance.Find<Character>(this.masterid_);
                    if (this.master_ == null) this.masterid_ = Guid.Empty;
                }
                return this.master_;
            }
            set
            {
                this.master_ = value;
                this.masterid_ = (value != null ? value.Id : Guid.Empty);
            }
        }
        #endregion

        protected Character()
        {
            this.DeathMessage = "{1} drops dead!";
            this.CanRegen = true;
        }

        public static Character Create(Character blueprint)
        {
            Character ch = blueprint.MemberwiseClone() as Character;
            ch.Init(blueprint);
            return ch;
        }

        public void Init(Character other)
        {
            this.Init(other, true);
        }
        public void Init(Character other, bool cloneitems)
        {
            base.Init(other);
            this.raceId_ = other.raceId_;
            this.jobId_ = other.jobId_;
            this.Abilities = new Dictionary<Guid,int>();
            foreach (KeyValuePair<Guid, int> pair in other.Abilities)
                this.Abilities.Add(pair.Key, pair.Value);

            if (cloneitems)
            {
                this.Equipment = new Dictionary<BodyParts, Item>();
                foreach (KeyValuePair<BodyParts, Item> pair in other.Equipment)
                    this.Equipment.Add(pair.Key, Item.Create(pair.Value));
            }
        }
        public override void Send(string str, IObject actor)
        {
            if (this.User != null)
                this.User.Send(str);
        }
        public string GenerateLookDescription(string startingPhrase, int minDistance, int maxDistance)
        {
            Dictionary<string, List<Object>> objects = Utils.ConsolidateList<Object>(this.NearbyObjects<Object>(this.Location.Point, minDistance, maxDistance, true, null, this));

            if (objects.Count > 0)
            {
                string description = startingPhrase;
                int count = 0;
                foreach (KeyValuePair<string, List<Object>> pair in objects)
                {
                    if (count++ > 0)
                    {
                        if (count == objects.Count)
                            description += " and ";
                        else
                            description += ", ";
                    }
                    description += Server.Instance.Settings.LookFormat.FormatObject(this, pair.Value[0], pair.Value.Count);
                }
                description += ". ";
                return String.ReplaceVars(description, "n/a");
            }

            return string.Empty;
        }
        public void Use(Item item, bool silent)
        {
            if (!item.CanUse(this) && !silent)
            {
                this.Send("You can't use that.");
                return;
            }

            if (item.ObjectFlags.Get((int)ObjFlags.Edible))
            {
                this.Eat(item);
                item.OnUsed(this);
            }
            else if (item.EquipLocation != BodyParts.None)
            {
                this.Equip(item, silent);
                item.OnUsed(this);
            }
            else
            {
                this.Send("You can't use that.");
            }
        }
        public void Eat(Item item)
        {
            if (!item.ObjectFlags.Get((int)ObjFlags.Edible))
                this.Send("You can't eat that.");
            else
            {
                this.Area.Send(new String("You eat {2}.", "You eat {2}.", null, null, "{1} eats {2}.", null), this, item, 0);
                item.Apply(this);
                item.Parent = null;
            }
        }
        public void Equip(Item item, bool silent)
        {
            if (this.Equipment.ContainsKey(item.EquipLocation))
            {
                if (this.GetEquippedItem(item.EquipLocation) == item)
                {
                    if (!silent)
                        this.Send("You're already using that.");
                    return;
                }
                if (!this.Remove(item.EquipLocation))
                    return;
            }

            this.Equipment.Add(item.EquipLocation, item);
            if (!silent)
            {
                string verb = (item.EquipLocation == BodyParts.Held ? "hold" : "wear");
                String str = new String("You " + verb + " {2}.", "You " + verb + " {2}.", null, null, "{1} " + verb + "s {2}.", null);
                this.Area.Send(str, this, item, 0);
            }
            item.Apply(this);
        }
        public bool Remove(BodyParts location, bool silent)
        {
            if (this.Equipment.ContainsKey(location))
            {
                if (!this.CheckInventoryLimit())
                    return false;
                if (!silent)
                    this.Area.Send(new String("You remove {2}.", "You remove {2}.", null, null, "{1} removes {2}.", null), this, this.Equipment[location], 0);
                Item item = this.Equipment[location];
                this.Equipment.Remove(location);
                item.Undo(this);
            }
            return true;
        }
        public bool Remove(BodyParts location)
        {
            return this.Remove(location, false);
        }
        public bool Remove(Item item, bool silent)
        {
            if (this.GetEquippedItem(item.EquipLocation) == item)
                return this.Remove(item.EquipLocation, silent);
            return false;
        }
        public bool Remove(Item item)
        {
            return this.Remove(item, false);
        }
        public void PickUp(Item item)
        {
            if (this.CheckInventoryLimit())
            {
                this.Area.Send(new String("You pick up {2}.", "You pick up {2}.", null, null, "{1} picks up {2}.", null), this, item, 0);
                item.Parent = this;
            }
        }
        public void Drop(Item item, bool silent)
        {
            this.Remove(item, silent);
            if (!silent)
                this.Area.Send(new String("You drop {2}.", "You drop {2}.", null, null, "{1} drops {2}.", null), this, item, 0);
            item.Location = this.Location;
        }
        public void DropMoney(int amount, bool silent)
        {
            Item money = new Item();
            money.Name = "a pile of money";
            money.Value = this.Value;
            money.ObjectFlags.Set((int)ObjFlags.Money, true);
            money.MapColor = Telnet.Color.BrightYellow;
            money.MapSymbol = "$";
            this.Drop(money, silent);
        }
        public void DropItems(bool silent)
        {
            foreach (Item item in this.GetChildren<Item>())
                this.Drop(item, silent);

            if (this.Value > 0)
                this.DropMoney(this.Value, silent);
        }
        public void Give(Item item, Character ch)
        {
            this.Remove(item);
            String str = new String(
                "You give {2} " + item.Name + ".", "You give {2} " + item.Name + ".",
                "{1} gives you " + item.Name + ".", "{1} gives you " + item.Name + ".",
                "{1} gives {2} " + item.Name + ".", null);
            this.Area.Send(str, this, ch, 0);
            item.Parent = ch;
        }
        public void Put(Item item, IContainer container)
        {
            this.Remove(item);
            if (!item.CanFit(container))
            {
                this.Send("That won't fit in there.");
                return;
            }

            String str = new String(
                "You put " + item.Name + " inside " + container.Name + ".", null,
                null, null,
                "{1} puts " + item.Name + " inside " + container.Name + ".", null);
            this.Area.Send(str, this, null, 0);
            item.Parent = container as Object;
        }
        public void Get(Item item, IContainer container)
        {
            if (this.CheckInventoryLimit())
            {
                String str = new String(
                    "You get " + item.Name + " from " + container.Name + ".", null,
                    null, null,
                    "{1} gets " + item.Name + " from " + container.Name + ".", null);
                this.Area.Send(str, this, null, 0);
                item.Parent = this;
            }
        }
        public void Faint(Object cause, Function_void executeOnRecover)
        {
            if (this.Position != Positions.Unconscious && this.Position != Positions.Recovering)
            {
                String str = new String(
                    "You lose consciousness!", "You lose consciousness!", 
                    "{1} loses consciousness!", "{1} loses consciousness!",
                    "{1} loses consciousness!", null);
                this.Area.Send(str, this, cause, 0, null, null);
                base.Faint(cause);
                this.ReplaceCurrentAction(new RecoverAction(this, 15, false, executeOnRecover));
            }
        }
        public override void Faint(Object cause)
        {
            this.Faint(cause, null);
        }
        public override void Die(Object killer)
        {
            bool alreadyDead = (this.Position == Positions.Dead);
            base.Die(killer);
            if (!alreadyDead)
            {
                Race race = this.Race;
                if (race != null)
                    this.Area.Send(race.DeathMessage, this, killer, 0);
                Server.Instance.Settings.DeathSettings.Apply(this, killer);
                if (Server.Instance.Settings.PvpSettings.SafeTimer != null && Server.Instance.Settings.PvpSettings.SafeTimer.Ticks > 0)
                {
                    this.safeTimer_ = new Timer(Server.Instance.Settings.PvpSettings.SafeTimer);
                    this.Send("You are safe from attack for " + Server.Instance.Settings.PvpSettings.SafeTimer.ToString() + ".");
                }
            }
        }
        public Item GetEquippedItem(BodyParts location)
        {
            Item item = null;
            if (this.Equipment.TryGetValue(location, out item))
                return item;
            else
                return null;
        }
        public Item GetEquippedItem(string name)
        {
            foreach (Item item in this.Equipment.Values)
                if (String.FindArgument(item.Name, name))
                    return item;
            return null;
        }
        public void Open(ICloseable o)
        {
            if (!o.Closed)
            {
                this.Send(String.Capitalize(o.Name) + " is already open.");
                return;
            }
            else if (o.Locked)
            {
                this.Send(String.Capitalize(o.Name) + " is locked.");
                return;
            }

            o.Closed = false;
            String str = new String("You open {2}.", "You open {2}.", null, null, "{1} opens {2}.", null);
            this.Area.Send(str, this, o, 0);
        }
        public void Close(ICloseable o)
        {
            if (!o.Closeable)
            {
                this.Send("You can't close that.");
                return;
            }
            else if (o.Closed)
            {
                this.Send(String.Capitalize(o.Name) + " is already closed.");
                return;
            }

            o.Closed = true;
            String str = new String("You close {2}.", "You close {2}.", null, null, "{1} closes {2}.", null);
            this.Area.Send(str, this, o, 0);
        }
        public void Lock(ICloseable o)
        {
            if (!o.Closed)
            {
                this.Send(String.Capitalize(o.Name) + " isn't closed.");
                return;
            }
            else if (o.Locked)
            {
                this.Send(String.Capitalize(o.Name) + " is already locked.");
                return;
            }
            else if (o.Key == null)
            {
                this.Send("You can't lock that.");
                return;
            }

            Item key = this.FindChild<Item>(o.Key.Id, false);
            if (key == null)
            {
                this.Send("You don't have the key.");
                return;
            }
            else
            {
                o.Locked = true;
                String str = new String("You lock {2}.", "You lock {2}.", null, null, "{1} locks {2}.", null);
                this.Area.Send(str, this, o, 0);
            }
        }
        public void Unlock(ICloseable o)
        {
            if (!o.Locked)
            {
                this.Send(String.Capitalize(o.Name) + " isn't locked.");
                return;
            }
            else if (o.Key == null)
            {
                this.Send("You can't unlock that.");
                return;
            }

            Item key = this.FindChild<Item>(o.Key.Id, false);
            if (key == null)
            {
                this.Send("You don't have the key.");
                return;
            }
            else
            {
                o.Locked = false;
                String str = new String("You unlock {2}.", "You unlock {2}.", null, null, "{1} unlocks {2}.", null);
                this.Area.Send(str, this, o, 0);
            }
        }
        public bool CheckInventoryLimit()
        {
            if (Server.Instance.Settings.InventoryLimit > 0 && 
                this.Inventory.Count + 1 > Server.Instance.Settings.InventoryLimit)
            {
                this.Send("Your inventory is full.");
                if (this.User != null)
                    this.User.SendHelp("Drop something or put something in a container to make room.");
                return false;
            }
            return true;
        }
        public bool CanUse(Ability ability, bool checkrequirements)
        {
            if (ability.RaceRestrictions != null && ability.RaceRestrictions.Count > 0 && !ability.RaceRestrictions.Contains(this.raceId_))
                return false;
            if (ability.JobRestrictions != null && ability.JobRestrictions.Count > 0 && !ability.JobRestrictions.Contains(this.jobId_))
                return false;

            if (checkrequirements)
                foreach (Guid abilityId in ability.Requirements)
                    if (!this.Abilities.ContainsKey(abilityId))
                        return false;

            return true;
        }
        public Ability GetRandomAggressiveAbility(IObject target)
        {
            Dictionary<Guid, int> abilities = null;
            lock(this.abilities_)
                abilities = new Dictionary<Guid, int>(this.abilities_);
            List<Ability> aggressiveAbilities = new List<Ability>();
            foreach (Guid id in abilities.Keys)
            {
                Ability ability = Server.Instance.Get<Ability>(id);
                if (ability != null && ability.Style == Ability.Styles.Aggressive && !ability.Auto && ability.Target != Ability.Targets.Self && ability.CheckRange(this, target))
                    aggressiveAbilities.Add(ability);
            }
            if (aggressiveAbilities.Count > 0)
                return aggressiveAbilities[Server.RandomNumber.Next(0, aggressiveAbilities.Count)];
            else
                return null;
        }
        public Item FindItemInInventory(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                int num = 1;
                int count = 0;

                if (System.Text.RegularExpressions.Regex.IsMatch(name, @"^[0-9]+\..+"))
                {
                    string[] parts = name.Split(".".ToCharArray(), 2);
                    num = int.Parse(parts[0]);
                    name = parts[1];
                }

                foreach (Item item in this.Inventory)
                    if (String.FindArgument(item.Name, name) && ++count == num)
                        return item;
            }
            return null;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if (this.safeTimer_ != null && !this.safeTimer_.Update())
            {
                this.safeTimer_ = null;
                this.Send("You are no longer safe from attack.");
            }

            base.Update();
        }
        
        [Obfuscation(Exclude = false)]
        protected override void OnMoved()
        {
            List<Area.NpcRegion> list = this.Area.GetLayers<Area.NpcRegion>(this.Location.Point, true);
            foreach (Area.NpcRegion region in list)
                if (region.CheckEncounter(this))
                    return;
        }
        
        public override bool CanMove(System.Drawing.Point location, bool force)
        {
            if (!force && this.NearbyObjects<Character>(0, false).Exists(delegate(Character ch) 
            {
                if (ch.CurrentAction != null && ch.CurrentAction is AbilityAction)
                {
                    AbilityAction action = ch.CurrentAction as AbilityAction;
                    if (action.Ability.Style == Ability.Styles.Aggressive && action.Target == this)
                        return true;
                }
                return false;
            }))
            {
                this.Send("Someone is attacking you, defend yourself!");
                return false;
            }
            return base.CanMove(location, force);
        }
        public void DoCommand(string str, int delay)
        {
            User user = this.user_;
            if (user == null)
                user = new User(this);

            if (delay > 0)
                this.EnqueueAction(new CommandAction(str, delay, user));
            else
            {
                user.Command = str;
                Server.Instance.ProcessCommand(user, true);
                user.FinishInput();
            }
        }
        public void DoCommand(string str)
        {
            this.DoCommand(str, 0);
        }
        public bool Carrying(string itemname)
        {
            return (this.FindItemInInventory(itemname) != null);
        }
        public static bool IsPlayer(Object o)
        {
            return (!(o is Npc) && o is Character);
        }
        public void Improve(Ability ability)
        {
            ++this.abilities_[ability.Id];
        }
        public int GetLevel(Ability ability)
        {
            return this.abilities_[ability.Id];
        }
        public bool HasAbility(string name)
        {
            Ability a = Server.Instance.Get<Ability>(name);
            return this.abilities_.ContainsKey(a.Id);
        }
        public int GetAbilityLevel(string abilityname)
        {
            Ability ability = Server.Instance.Get<Ability>(abilityname);
            lock (this.abilities_)
            {
                if (ability != null && this.abilities_.ContainsKey(ability.Id))
                    return this.GetLevel(ability);
                else
                {
                    Debug.WriteLine(this.Name + " does not have a '" + abilityname + "' ability.", "DEBUG");
                    return 0;
                }
            }
        }

        [Obfuscation(Exclude = false)]
        internal override void Draw(Graphics g, AreaCanvas canvas, AreaCanvas.View view)
        {
            if (canvas.Contains(this))
            {
                switch (view)
                {
                    case AreaCanvas.View.Design1:
                    case AreaCanvas.View.Design2:
                        g.DrawImage(Ultimud.Properties.Resources.man,
                            (this.Location.Point.X - canvas.Offset.X) * canvas.Zoom,
                            (this.Location.Point.Y - canvas.Offset.Y) * canvas.Zoom,
                            canvas.Zoom,
                            canvas.Zoom);
                        break;
                    case AreaCanvas.View.Game:
                        using (Font font = new Font("fixedsys", System.Math.Max((int)(canvas.Zoom / 1.7), 1), FontStyle.Bold))
                        {
                            int xpos = (this.Location.Point.X - canvas.Offset.X) * canvas.Zoom;
                            int ypos = (this.Location.Point.Y - canvas.Offset.Y) * canvas.Zoom;
                            g.FillRectangle(Brushes.Black, xpos, ypos, canvas.Zoom, canvas.Zoom);
                            using (SolidBrush b = new SolidBrush(Telnet.ColorToSystemColor(this.MapColor)))
                                g.DrawString(this.MapSymbol, font, b, xpos, ypos);
                        }
                        break;
                }
            }
        }
    }
}
