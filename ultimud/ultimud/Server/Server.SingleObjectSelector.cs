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
        public class SingleObjectSelector<T> : UITypeEditor where T : class, IIdentifiable, new()
        {
            Panel panel_ = new Panel();
            ToolStrip toolstrip_ = new ToolStrip();
            ListBox listbox_ = new ListBox();
            IWindowsFormsEditorService service_;

            public SingleObjectSelector()
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
                        this.listbox_.Items.Add(t);
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
                this.listbox_.Sorted = true;
                this.listbox_.BringToFront();
                this.listbox_.SelectionMode = SelectionMode.One;
                this.listbox_.MouseClick += delegate(object sender, MouseEventArgs e)
                {
                    if (this.service_ != null)
                        this.service_.CloseDropDown();
                };
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                this.listbox_.Items.Clear();
                Type type = typeof(T);
                Debug.Assert(Server.Instance.DataSets.ContainsKey(type), type.Name + " data set not found.");
                foreach (T t in Server.Instance.DataSets[type].Values)
                {
                    this.listbox_.Items.Add(t);
                    if (value is Guid && t.Id == (Guid)value)
                        this.listbox_.SelectedItem = t;
                }

                this.service_ = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                if (this.service_ != null)
                    this.service_.DropDownControl(this.panel_);

                T selection = this.listbox_.SelectedItem as T;
                if (selection != null)
                    return selection.Id;

                return value;
            }
        }
    }
}
