using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using Damdai.Forms;

namespace Ultimud
{
    public partial class Server
    {
        [Obfuscation(Exclude = false)]
        public class ObjectBlueprintSelector<T> : UITypeEditor where T : Object
        {
            ListBox listbox_ = new ListBox();
            public ObjectBlueprintSelector()
            {
                this.listbox_.BorderStyle = BorderStyle.None;
                this.listbox_.Sorted = true;
            }
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }
            public override bool IsDropDownResizable
            {
                get { return true; }
            }
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                this.listbox_.Items.Clear();
                this.listbox_.Items.Add("-None-");
                Type type = typeof(T);
                foreach (T t in Server.Instance.DataSets[type].Values)
                {
                    this.listbox_.Items.Add(t);
                    if (t == value)
                        this.listbox_.SelectedItem = t;
                }

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                this.listbox_.MouseClick += delegate(object sender, MouseEventArgs e) { service.CloseDropDown(); };
                service.DropDownControl(this.listbox_);
                if (this.listbox_.SelectedItem != null)
                    return (this.listbox_.SelectedItem is string ? null : this.listbox_.SelectedItem);

                return value;
            }
        }

        [Obfuscation(Exclude = false)]
        public class ObjectBlueprintIdSelector<T> : UITypeEditor where T : Object
        {
            ListBox listbox_ = new ListBox();
            public ObjectBlueprintIdSelector()
            {
                this.listbox_.BorderStyle = BorderStyle.None;
                this.listbox_.Sorted = true;
            }
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }
            public override bool IsDropDownResizable
            {
                get { return true; }
            }
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                this.listbox_.Items.Clear();
                Type type = typeof(T);
                foreach (T t in Server.Instance.DataSets[type].Values)
                {
                    this.listbox_.Items.Add(t);
                    if (value != null && t.Id == (Guid)value)
                        this.listbox_.SelectedItem = t;
                }

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                this.listbox_.MouseClick += delegate(object sender, MouseEventArgs e) { service.CloseDropDown(); };
                service.DropDownControl(this.listbox_);
                if (this.listbox_.SelectedItem != null)
                    return ((Object)this.listbox_.SelectedItem).Id;

                return value;
            }
        }
    }
}
