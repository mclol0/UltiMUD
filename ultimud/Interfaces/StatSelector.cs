using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Reflection;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    public class StatSelector : UITypeEditor
    {
        ListBox listbox_ = new ListBox();

        public StatSelector()
        {
            this.listbox_.BorderStyle = BorderStyle.None;
        }
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value != null)
            {
                this.listbox_.Items.Clear();

                foreach (IStatBlueprint statblueprint in Globals.Server.BlueprintManager.GetBlueprints<IStatBlueprint>())
                    this.listbox_.Items.Add(statblueprint);
                this.listbox_.Items.Add(" <New>");

                IStatBlueprint currentstatblueprint = value as IStatBlueprint;
                if (currentstatblueprint != null)
                    this.listbox_.SelectedItem = currentstatblueprint.Name;

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                this.listbox_.MouseClick += delegate(object sender, MouseEventArgs e) { service.CloseDropDown(); };
                service.DropDownControl(this.listbox_);

                if (this.listbox_.SelectedItem != null)
                {
                    if (this.listbox_.SelectedItem.ToString() == " <New>")
                    {
                        //Damdai.Forms.PropertyGrid propgrid = new Damdai.Forms.PropertyGrid();
                        //StatBlueprint statblueprint = new StatBlueprint();
                        //propgrid.SelectedObject = statblueprint;
                        //Dialog dialog = new Dialog(propgrid, "New Stat");
                        //if (dialog.ShowDialog() == DialogResult.OK)
                        //{
                        //    Globals.Server.AddBlueprint(statblueprint);
                        //    return statblueprint;
                        //}
                    }
                    else
                        return this.listbox_.SelectedItem as IStatBlueprint;
                }
            }

            return value;
        }
    }
}
