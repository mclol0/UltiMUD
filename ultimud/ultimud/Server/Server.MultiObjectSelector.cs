using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;
using System.Diagnostics;
using Damdai.Forms;

namespace Ultimud
{
    public partial class Server
    {
        [Obfuscation(Exclude = false)]
        public class MultiObjectSelector<T> : UITypeEditor where T : class, IIdentifiable, new()
        {
            Panel panel_ = new Panel();
            ToolStrip toolstrip_ = new ToolStrip();
            CheckedListBox listbox_ = new CheckedListBox();

            public MultiObjectSelector()
            {
                this.panel_.Controls.Add(this.toolstrip_);
                this.panel_.Controls.Add(this.listbox_);
                this.toolstrip_.Items.Add("New", null, delegate(object sender, EventArgs args)
                {
                    System.Windows.Forms.PropertyGrid propgrid = new System.Windows.Forms.PropertyGrid();
                    propgrid.SelectedObject = new T();
                    Dialog d = new Dialog(propgrid, "New " + typeof(T).Name);
                    if (d.ShowDialog() == DialogResult.OK)
                    {
                        T t = propgrid.SelectedObject as T;
                        Server.Instance.Add<T>(t);
                        this.listbox_.Items.Add(t, true);
                    }
                });
                this.toolstrip_.Items.Add("Delete", null, delegate(object sender, EventArgs args)
                {
                    if (this.listbox_.SelectedItem != null)
                    {
                        IIdentifiable o = this.listbox_.SelectedItem as IIdentifiable;
                        if (o != null)
                        {
                            Server.Instance.DataSets[typeof(T)].Remove(o.Id);
                            this.listbox_.Items.Remove(o);
                        }
                    }
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
                List<Guid> list = null;

                if (value != null)
                    list = (List<Guid>)value;
                else
                    list = new List<Guid>();

                this.listbox_.Items.Clear();
                Type type = typeof(T);
                Dictionary<Guid, IIdentifiable> dataset = null;
                Trace.Assert(Server.Instance.DataSets.TryGetValue(type, out dataset), type.Name + " dataset not found.");
                foreach(T t in dataset.Values)
                    this.listbox_.Items.Add(t, list.Contains(t.Id));

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                if (service != null)
                    service.DropDownControl(this.panel_);
                list.Clear();
                for (int x = 0; x < this.listbox_.CheckedItems.Count; ++x)
                    list.Add(((T)this.listbox_.CheckedItems[x]).Id);

                return list;
            }
        }
    }
}
