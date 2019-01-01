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
    public class BlueprintSelector<T> : UITypeEditor where T : class, IIdentifiable
    {
        Panel panel_ = new Panel();
        ToolStrip toolstrip_ = new ToolStrip();
        ListBox listbox_ = new ListBox();

        public BlueprintSelector()
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
            //        this.listbox_.Items.Add(t);
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
            this.listbox_.Sorted = true;
            this.listbox_.BringToFront();
            this.listbox_.SelectionMode = SelectionMode.One;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            this.listbox_.Items.Clear();
            foreach (T t in Globals.Server.GetBlueprints<T>())
            {
                this.listbox_.Items.Add(t);
                if (t == value)
                    this.listbox_.SelectedItem = t;
            }

            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            //this.listbox_.MouseClick += delegate(object sender, MouseEventArgs e) { service.CloseDropDown(); };
            service.DropDownControl(this.panel_);

            return this.listbox_.SelectedItem as T;
        }
    }
}
