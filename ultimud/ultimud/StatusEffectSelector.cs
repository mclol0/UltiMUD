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
    class StatusEffectSelector : UITypeEditor
    {
        ListBox listBox_ = new ListBox();
        IWindowsFormsEditorService service_;

        public StatusEffectSelector()
        {
            this.listBox_.BorderStyle = BorderStyle.None;
            this.listBox_.Sorted = true;
            this.listBox_.MouseClick += delegate(object sender, MouseEventArgs e)
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
            if (value != null)
            {
                this.listBox_.Items.Clear();

                foreach (StatusEffectBlueprint statBlueprint in Server.Instance.DataSets[typeof(StatusEffectBlueprint)].Values)
                    this.listBox_.Items.Add(statBlueprint);
                this.listBox_.Items.Add(" <New>");

                Guid statId = (Guid)value;
                StatusEffectBlueprint currentStat = Server.Instance.Get<StatusEffectBlueprint>(statId);
                if (currentStat != null)
                    this.listBox_.SelectedItem = currentStat;

                this.service_ = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                this.service_.DropDownControl(this.listBox_);

                if (this.listBox_.SelectedItem != null)
                {
                    if (this.listBox_.SelectedItem.ToString() == " <New>")
                    {
                        Damdai.Forms.PropertyGrid propertyGrid = new Damdai.Forms.PropertyGrid();
                        StatusEffectBlueprint statBlueprint = new StatusEffectBlueprint();
                        propertyGrid.SelectedObject = statBlueprint;
                        Dialog dialog = new Dialog(propertyGrid, "New Status Effect");
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Server.Instance.Add<StatusEffectBlueprint>(statBlueprint);
                            return statBlueprint.Id;
                        }
                    }
                    else
                    {
                        StatusEffectBlueprint selectedStat = this.listBox_.SelectedItem as StatusEffectBlueprint;
                        if (selectedStat != null)
                            return selectedStat.Id;
                    }
                }
            }

            return value;
        }
    }
}
