using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Damdai.Attributes
{
    public class DisplayOrderAttribute : Attribute
    {
        int index_;

        #region Properties

        public int Index
        {
            get { return this.index_; }
        }

        #endregion

        public DisplayOrderAttribute(int index)
        {
            this.index_ = index;
        }

        public class Comparer : IComparer<PropertyDescriptor>
        {
            public int Compare(PropertyDescriptor prop1, PropertyDescriptor prop2)
            {
                DisplayOrderAttribute order1 = prop1.Attributes[typeof(DisplayOrderAttribute)] as DisplayOrderAttribute;
                DisplayOrderAttribute order2 = prop2.Attributes[typeof(DisplayOrderAttribute)] as DisplayOrderAttribute;
                if (order1 != null && order2 != null)
                {
                    return order1.Index.CompareTo(order2.Index);
                }
                else if (order1 != null && order2 == null)
                {
                    return 1;
                }
                else if (order1 == null && order2 != null)
                {
                    return -1;
                }
                else
                {
                    return prop1.Name.CompareTo(prop2.Name);
                }
            }
        }
    }
}
