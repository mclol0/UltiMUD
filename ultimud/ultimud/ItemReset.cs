using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class ItemReset : Reset
    {
        List<Guid> items_;

        #region Properties
        [DisplayName("Item")]
        [Description("The item that this reset will spawn.")]
        [TypeConverter(typeof(Server.ObjectBlueprintIdToNameConverter<Item>))]
        [Editor(typeof(Server.ObjectBlueprintIdSelector<Item>), typeof(UITypeEditor))]
        public new Guid ObjectId
        {
            get { return base.ObjectId; }
            set { base.ObjectId = value; }
        }

        [Description("The items that this item will contain.")]
        [Editor(typeof(Server.MultiObjectSelector<Item>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Item>))]
        public List<Guid> Items
        {
            get { return this.items_; }
            set { this.items_ = value; }
        }
        #endregion

        public ItemReset(Location location)
            : base(location)
        {
        }

        [Obfuscation(Exclude = false)]
        protected override bool CheckConditions()
        {
            List<Guid> idsToRemove = new List<Guid>();
            foreach (Guid id in this.objectIds_)
            {
                if (this.Location.Area.FindChild<Item>(id, false) == null)
                {
                    idsToRemove.Add(id);
                }
            }

            foreach (Guid id in idsToRemove)
            {
                this.objectIds_.Remove(id);
            }

            return (this.objectIds_.Count == 0);
        }

        [Obfuscation(Exclude = false)]
        void ResetItems(Item item)
        {
            if (this.Items != null)
            {
                foreach (Guid id in this.Items)
                {
                    Item i = Server.Instance.Get<Item>(id);
                    if (i != null)
                    {
                        i = Item.Create(i);
                        i.Parent = item;
                    }
                }
            }
        }
        
        [Obfuscation(Exclude = false)]
        protected override Object Respawn()
        {
            Item item = base.Respawn() as Item;
            if (item != null) this.ResetItems(item);
            return item;
        }
    }
}
