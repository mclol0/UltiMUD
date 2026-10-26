using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Damdai.Attributes;

namespace Damdai.Forms
{
    public class PropertyGrid : System.Windows.Forms.PropertyGrid
    {
        Container components_ = null;
        DynamicPropertyTab tab_ = new DynamicPropertyTab();

        public bool ReadOnly
        {
            get { return this.tab_.ReadOnly; }
            set { this.tab_.ReadOnly = value; }
        }

        public PropertyGrid()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components_ != null)
                {
                    components_.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Name = "PropertyGrid";
        }

        protected override PropertyTab CreatePropertyTab(Type tabType)
        {
            return this.tab_;
        }

        public void Hide(Attribute attribute)
        {
            this.tab_.Hide(attribute);
        }

        public void Unhide(Attribute attribute)
        {
            this.tab_.Unhide(attribute);
        }

        public void Disable(Attribute attribute)
        {
            this.tab_.Disable(attribute);
        }

        public void Enable(Attribute attribute)
        {
            this.tab_.Enable(attribute);
        }
    }

    public class DynamicPropertyTab : PropertyTab
    {
        List<Attribute> hiddenlist_ = new List<Attribute>();
        List<Attribute> readonlylist_ = new List<Attribute>();
        bool readonly_;
        static readonly System.Drawing.Bitmap bitmap_ = new System.Drawing.Bitmap(16, 16);

        public bool ReadOnly
        {
            get { return this.readonly_; }
            set { this.readonly_ = value; }
        }

        public void Hide(Attribute attribute)
        {
            if (!this.hiddenlist_.Contains(attribute))
            {
                this.hiddenlist_.Add(attribute);
            }
        }

        public void Unhide(Attribute attribute)
        {
            if (this.hiddenlist_.Contains(attribute))
            {
                this.hiddenlist_.Remove(attribute);
            }
        }

        public void Disable(Attribute attribute)
        {
            if (!this.readonlylist_.Contains(attribute))
            {
                this.readonlylist_.Add(attribute);
            }
        }

        public void Enable(Attribute attribute)
        {
            if (this.readonlylist_.Contains(attribute))
            {
                this.readonlylist_.Remove(attribute);
            }
        }

        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            List<PropertyDescriptor> props = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(component, attributes))
            {
                if (this.hiddenlist_.Count > 0 && this.hiddenlist_.Exists(delegate(Attribute a) { return prop.Attributes.Contains(a); }))
                {
                    continue;
                }

                if (!(prop.Converter is ExpandableObjectConverter) && (this.readonly_ || (this.readonlylist_.Count > 0 && this.readonlylist_.Exists(delegate(Attribute a) { return prop.Attributes.Contains(a); }))))
                {
                    //props.Add(TypeDescriptor.CreateProperty(component.GetType(), prop, new ReadOnlyAttribute(true)));
                    DisplayOrderAttribute displayorder = prop.Attributes[typeof(DisplayOrderAttribute)] as DisplayOrderAttribute;
                    if (displayorder != null)
                    {
                        props.Add(new ReadOnlyProperty(component, prop, displayorder));
                    }
                    else
                    {
                        props.Add(new ReadOnlyProperty(component, prop));
                    }
                }
                else
                {
                    props.Add(prop);
                }
            }
            props.Sort(new DisplayOrderAttribute.Comparer());
            return new PropertyDescriptorCollection(props.ToArray());
        }

        public override PropertyDescriptorCollection GetProperties(object component)
        {
            return this.GetProperties(component, null);
        }

        public override string TabName
        {
            get { return "Properties"; }
        }

        public override System.Drawing.Bitmap Bitmap
        {
            get
            {
                return bitmap_;
            }
        }
    }

    public class ReadOnlyProperty : PropertyDescriptor
    {
        object parent_;
        object value_;

        #region Properties

        public override Type ComponentType
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override Type PropertyType
        {
            get { return typeof(string); }
        }

        #endregion

        public ReadOnlyProperty(object parent, string name, object value, params Attribute[] attributes)
            : base(name, attributes)
        {
            this.parent_ = parent;
            this.value_ = value;
        }

        public ReadOnlyProperty(object parent, PropertyDescriptor property, params Attribute[] attributes)
            : this(parent, property.DisplayName, property.Converter.ConvertToString(property.GetValue(parent)), attributes)
        {
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return this.value_;
        }

        public override void ResetValue(object component)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetValue(object component, object value)
        {
            this.value_ = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
