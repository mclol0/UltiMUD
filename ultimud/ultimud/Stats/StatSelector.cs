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
    class StatSelector : UITypeEditor
    {
        ListBox listBox_ = new ListBox();

        public StatSelector()
        {
            this.listBox_.BorderStyle = BorderStyle.None;
        }
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value != null)
            {
                this.listBox_.Items.Clear();

                foreach (StatBlueprint statBlueprint in Server.Instance.DataSets[typeof(StatBlueprint)].Values)
                    this.listBox_.Items.Add(statBlueprint);
                this.listBox_.Items.Add("New stat...");

                Guid statId = (Guid)value;
                StatBlueprint currentStat = Server.Instance.Get<StatBlueprint>(statId);
                if (currentStat != null)
                    this.listBox_.SelectedItem = currentStat.Name;

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                this.listBox_.MouseClick += delegate(object sender, MouseEventArgs e) { service.CloseDropDown(); };
                service.DropDownControl(this.listBox_);

                if (this.listBox_.SelectedItem != null)
                {
                    if (this.listBox_.SelectedItem.ToString() == "New stat...")
                    {
                        Damdai.Forms.PropertyGrid propertyGrid = new Damdai.Forms.PropertyGrid();
                        StatBlueprint statBlueprint = new StatBlueprint();
                        propertyGrid.SelectedObject = statBlueprint;
                        Dialog dialog = new Dialog(propertyGrid, "New Stat");
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Server.Instance.Add<StatBlueprint>(statBlueprint);
                            return statBlueprint.Id;
                        }
                    }
                    else
                    {
                        StatBlueprint selectedStat = this.listBox_.SelectedItem as StatBlueprint;
                        if (selectedStat != null)
                        {
                            return selectedStat.Id;
                        }
                    }
                }
            }

            return value;
        }
    }
}
