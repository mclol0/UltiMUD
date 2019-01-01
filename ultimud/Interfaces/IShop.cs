using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IShop : IObject
    {
        List<Guid> Items { get; set; }
        List<IItem> Stock { get; }
        List<Guid> ItemTags { get; set; }
        int MarkUp { get; set; }
        int MarkDown { get; set; }

        IItem FindItem(string name);
        int SellPrice(IItem item);
        int BuyPrice(IItem item);
    }
}
