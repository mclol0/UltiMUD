using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.Serialization;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Shop : Object
    {
        List<Guid> items_ = new List<Guid>();
        List<Item> stock_ = new List<Item>();
        int markup_ = 25;
        int markdown_ = 25;
#pragma warning disable 0169, 0414
        List<string> itemtypes_; //Deprecated.
#pragma warning restore 0169, 0414
        List<Guid> itemtags_ = new List<Guid>();

        #region Properties
        [Description("The items that this shop has for sale.")]
        [Editor(typeof(Server.MultiObjectSelector<Item>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Item>))]
        public List<Guid> Items
        {
            get { return this.items_; }
            set { this.items_ = value; }
        }

        [Description("The percentage of an items value that is added to this items selling price.")]
        [DefaultValue(25)]
        public int MarkUp
        {
            get { return this.markup_; }
            set { this.markup_ = value; }
        }

        [Description("The percentage of an items value that is subtracted from this items buying price.")]
        [DefaultValue(25)]
        public int MarkDown
        {
            get { return this.markdown_; }
            set { this.markdown_ = value; }
        }

        [Description("The tags that must be present on items that this shop will buy.")]
        [Editor(typeof(Server.MultiObjectSelector<Tag>), typeof(UITypeEditor))]
        [TypeConverter(typeof(Server.ObjectCollectionConverter<Tag>))]
        public List<Guid> ItemTags
        {
            get { return this.itemtags_; }
            set { this.itemtags_ = value; }
        }

        [Browsable(false)]
        new public Sex Gender
        {
            get { return base.Gender; }
            set { base.Gender = value; }
        }

        [Browsable(false)]
        new public Positions Position
        {
            get { return base.Position; }
            set { base.Position = value; }
        }

        public override int IconIndex
        {
            get
            {
                return 8;
            }
        }

        [Browsable(false)]
        public List<Item> Stock
        {
            get { return this.stock_; }
        }
        #endregion

        public Shop()
        {
            this.DeathMessage = "{1} crumbles to the ground!";
            this.MapSymbol = "$$";
            this.MapColor = Telnet.Color.Green;
        }

        public Item FindItem(string name)
        {
            name = name.ToLower();
            int num = 1;
            if (System.Text.RegularExpressions.Regex.IsMatch(name, @"^[0-9]+\..+"))
            {
                string[] parts = name.Split(".".ToCharArray(), 2);
                num = int.Parse(parts[0]);
                name = parts[1];
            }

            int count = 0;
            foreach (Guid id in this.Items)
            {
                Item item = Server.Instance.Get<Item>(id);
                if (item != null && String.FindArgument(item.Name, name) && ++count == num)
                    return Item.Create(item);
            }

            foreach (Item item in this.stock_)
                if (item != null && String.FindArgument(item.Name, name) && ++count == num)
                    return item;

            return null;
        }
        public void Sell(Item item, Character ch)
        {
            int price = this.SellPrice(item);
            if (ch.Value >= price)
            {
                if (ch.CheckInventoryLimit())
                {
                    ch.Value -= price;
                    this.Value += price;
                    item.Parent = ch;

                    //if (this.stock_.Contains(item))
                    //{
                        this.stock_.Remove(item);
                    //}

                    String str = new String("You buy {2} for $" + price + ".", "You buy {2} for $" + price + ".", null, null, "{1} buys {2}.", null);
                    ch.Area.Send(str, ch, item, 0);
                }
            }
            else
            {
                ch.Send("You don't have enough money.");
            }
        }
        public void Buy(Item item, Character ch)
        {
            if (this.ItemTags != null && this.ItemTags.Count > 0 && !item.Tags.Exists(delegate(Guid id)
            {
                return this.ItemTags.Contains(id);
            }))
            {
                ch.Send("The shop isn't interested in buying that.");
                return;
            }

            if (item.Value <= 0)
            {
                ch.Send("That item has no value.");
                return;
            }

            int price = this.BuyPrice(item);

            if (price <= 0)
            {
                ch.Send("You will not make any money selling that item here.");
                return;
            }

            if (this.Value >= price)
            {
                ch.Value += price;
                this.Value -= price;
                item.Parent = null;

                if (!this.Items.Contains(item.BlueprintId))
                    this.stock_.Add(item);

                String str = new String("You sell {2} for $" + price + ".", "You sell {2} for $" + price + ".", null, null, "{1} sells {2}.", null);
                ch.Area.Send(str, ch, item, 0);
            }
            else
            {
                ch.Send("This shop can't afford to buy that right now.");
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
                        g.DrawImage(Ultimud.Properties.Resources.shop,
                            (this.Location.Point.X - canvas.Offset.X) * canvas.Zoom,
                            (this.Location.Point.Y - canvas.Offset.Y) * canvas.Zoom,
                            canvas.Zoom,
                            canvas.Zoom);
                        break;
                    case AreaCanvas.View.Game:
                        using (Font font = new Font("fixedsys", Math.Max((int)(canvas.Zoom / 1.7), 1), FontStyle.Bold))
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

        public int SellPrice(Item item)
        {
            return (int)(item.Value * (float)(1 + ((float)this.MarkUp/100)));
        }
        public int BuyPrice(Item item)
        {
            return Math.Max(0, (int)(item.Value * (1.0f - ((float)this.MarkDown / 100.0f))));
        }

        [Obfuscation(Exclude = false)]
        public override string ToString()
        {
            return (string.IsNullOrEmpty(this.Name) ? base.ToString() : this.Name);
        }

        [OnDeserialized]
        [Obfuscation(Exclude = false)]
        new void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);
            if (this.itemtags_ == null) this.itemtags_ = new List<Guid>();
            this.itemtypes_ = null;
        }
    }
}
