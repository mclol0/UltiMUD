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
    [Obfuscation(Exclude = false)]
    public class BlueprintsSelector<T> : UITypeEditor where T : class, IIdentifiable
    {
        Panel panel_ = new Panel();
        ToolStrip toolstrip_ = new ToolStrip();
        CheckedListBox listbox_ = new CheckedListBox();

        public BlueprintsSelector()
        {
            this.panel_.Controls.Add(this.toolstrip_);
            this.panel_.Controls.Add(this.listbox_);
            //this.toolstrip_.Items.Add("New", null, delegate(object sender, EventArgs args)
            //{
            //    System.Windows.Forms.PropertyGrid propgrid = new System.Windows.Forms.PropertyGrid();
            //    propgrid.SelectedObject = new T();
            //    Dialog d = new Dialog(propgrid, "New " + typeof(T).Name);
            //    if (d.ShowDialog() == DialogResult.OK)
            //    {
            //        T t = propgrid.SelectedObject as T;
            //        Globals.Server.AddBlueprint(t);
            //        this.listbox_.Items.Add(t, true);
            //    }
            //});
            this.toolstrip_.Items.Add("Delete", null, delegate(object sender, EventArgs args)
            {
                IIdentifiable o = this.listbox_.SelectedItem as IIdentifiable;
                Globals.Server.DeleteBlueprint(o);
                this.listbox_.Items.Remove(o);
            });
            this.listbox_.Dock = DockStyle.Fill;
            this.listbox_.BorderStyle = BorderStyle.None;
            this.listbox_.CheckOnClick = true;
            this.listbox_.Sorted = true;
            this.listbox_.BringToFront();
        }
        public override bool IsDropDownResizable
        {
            get { return true; }
        }
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            List<T> list = null;

            if (value != null)
                list = (List<T>)value;
            else
                list = new List<T>();

            this.listbox_.Items.Clear();
            foreach (T t in Globals.Server.GetBlueprints<T>())
                this.listbox_.Items.Add(t, list.Contains(t));

            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            service.DropDownControl(this.panel_);
            list.Clear();
            for (int x = 0; x < this.listbox_.CheckedItems.Count; ++x)
                list.Add(this.listbox_.CheckedItems[x] as T);

            return list;
        }
    }
}
