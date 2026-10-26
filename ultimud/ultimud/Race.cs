using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.Serialization;
using Damdai.TypeConverters;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Race : IIdentifiable
    {
        Guid id_;
        string name_;
        List<StatModifier> stats_ = new List<StatModifier>();
        Location startLocation_;
        List<Guid> items_;
        int money_;
        List<Guid> abilities_ = new List<Guid>();
        String deathMessage_ = new String();
        Location deathLocation_;
        bool selectable_ = true;

        #region Properties
        [Browsable(false)]
        public Guid Id
        {
            get { return this.id_; }
            set { this.id_ = value; }
        }

        [Description("The name of this race.")]
        public string Name
        {
            get { return this.name_; }
            set { this.name_ = Telnet.Decolorize(value); }
        }

        [Description("The base stat modifiers for this race. Race and job stat modifiers are combined to determine character's starting stat values.")]
        [TypeConverter(typeof(ListConverter<StatModifier>))]
        public List<StatModifier> BaseStatModifiers
        {
            get { return this.stats_; }
            set { this.stats_ = value; }
        }

        [Description("The starting location for players of this race.")]
        public Location StartLocation
        {
            get { return this.startLocation_; }
            set { this.startLocation_ = value; }
        }

        [Description("The items that players of this race start with.")]
        [Editor(typeof(Server.MultiObjectSelector<Item>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Item>))]
        public List<Guid> StartItems
        {
            get { return this.items_; }
            set { this.items_ = value; }
        }

        [Description("The amount of money that players of this race start with.")]
        [DefaultValue(0)]
        public int StartMoney
        {
            get { return this.money_; }
            set { this.money_ = value; }
        }

        [Description("This race's starting abilities.")]
        [Editor(typeof(Server.MultiObjectSelector<Ability>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Ability>))]
        public List<Guid> Abilities
        {
            get { return this.abilities_; }
            set { this.abilities_ = value; }
        }

        [Description("The message that is displayed when someone of this race dies.")]
        public String DeathMessage
        {
            get { return this.deathMessage_; }
            set { this.deathMessage_ = value; }
        }

        [Description("This overrides the death settings transfer location.")]
        public Location DeathLocation
        {
            get { return this.deathLocation_; }
            set { this.deathLocation_ = value; }
        }

        [Description("Is this race selectable by players at creation time?")]
        [DefaultValue(true)]
        public bool Selectable
        {
            get { return this.selectable_; }
            set { this.selectable_ = value; }
        }
        #endregion

        internal static void Init(string loadpath, string serverpath)
        {
            if (!Server.Instance.DataSets.ContainsKey(typeof(Race)))
                Server.Instance.DataSets.Add(typeof(Race), new Dictionary<Guid, IIdentifiable>());
        }
        
        [Obfuscation(Exclude = false)]
        public void Apply(Character ch)
        {
            StatModifier.Apply(this.BaseStatModifiers, ch, ch, ch, null, true);
            this.ApplyStartMoney(ch);
            this.ApplyStartItems(ch);
        }
        
        [Obfuscation(Exclude = false)]
        void ApplyStartMoney(Character ch)
        {
            ch.Value += this.StartMoney;
        }
        
        [Obfuscation(Exclude = false)]
        void ApplyStartItems(Character ch)
        {
            if (this.StartItems != null)
            {
                foreach (Guid id in this.StartItems)
                {
                    Item item = Server.Instance.Get<Item>(id);
                    if (item != null)
                    {
                        Item newItem = Item.Create(item);
                        newItem.Parent = ch;
                    }
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name))
                return base.ToString();
            else
                return this.Name;
        }
        
        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        internal void OnDeserialized(StreamingContext context)
        {
            if (this.DeathMessage == null)
                this.DeathMessage = new String();
        }
    }
}
