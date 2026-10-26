using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Damdai.Forms;

namespace Ultimud
{
    class AbilitySelector : UITypeEditor
    {
        CheckedListBox listBox_ = new CheckedListBox();

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            List<Guid> selectedAbilities = null;

            if (value != null)
            {
                selectedAbilities = (List<Guid>)value;
            }
            else
            {
                selectedAbilities = new List<Guid>();
            }

            this.listBox_.Items.Clear();

            foreach (Ability ability in Server.Instance.Abilities.Values)
            {
                this.listBox_.Items.Add(ability, selectedAbilities.Contains(ability.Id));
            }

            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            service.DropDownControl(this.listBox_);

            selectedAbilities.Clear();

            for (int x = 0; x < this.listBox_.CheckedItems.Count; ++x)
            {
                selectedAbilities.Add(((Ability)this.listBox_.CheckedItems[x]).Id);
            }

            return selectedAbilities;
        }
    }
}
