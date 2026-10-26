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
    internal class CharacterAbilitySelector : UITypeEditor
    {
        NumericUpDown control_ = new NumericUpDown();
        Damdai.Forms.ListView view_ = new Damdai.Forms.ListView();

        internal CharacterAbilitySelector()
        {
            this.control_.Maximum = int.MaxValue;
            this.control_.Minimum = 0;

            this.view_.BorderStyle = BorderStyle.None;
            this.view_.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.view_.CheckBoxes = true;

            this.view_.Columns.Add("Ability");
            this.view_.Columns.Add("Cost/Chance");
            this.view_.Columns[1].TextAlign = HorizontalAlignment.Right;
            this.view_.SubItemEndEditing += new Damdai.Forms.SubItemEndEditingEventHandler(this.view_SubItemEndEditing);
            this.view_.SubItemClicked += new Damdai.Forms.SubItemEventHandler(this.view_SubItemClicked);
            this.control_.Parent = this.view_;
            this.control_.Visible = false;

            this.view_.Size = new System.Drawing.Size(200, 300);
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            this.view_.Items.Clear();

            Dictionary<Guid, int> dict = value as Dictionary<Guid, int>;
            if (dict == null)
                dict = new Dictionary<Guid, int>();

            foreach (Ability ability in Server.Instance.DataSets[typeof(Ability)].Values)
            {
                int level = 0;
                bool found = dict.TryGetValue(ability.Id, out level);
                ListViewItem i = new ListViewItem(new string[] { ability.Name, level.ToString() });
                i.Tag = ability;
                i.Checked = found;
                this.view_.Items.Add(i);
            }
            this.view_.Sorting = SortOrder.Ascending;
            this.view_.Sort();
            this.view_.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            service.DropDownControl(this.view_);
            dict.Clear();
            foreach (ListViewItem i in this.view_.Items)
                if (i.Checked)
                    dict.Add(((Ability)i.Tag).Id, int.Parse(i.SubItems[1].Text));

            return dict;
        }

        private void view_SubItemClicked(object sender, Damdai.Forms.SubItemEventArgs e)
        {
            if (e.SubItem == 1)
            {
                int val = 0;
                int.TryParse(e.Item.SubItems[e.SubItem].Text, out val);
                if (val < this.control_.Minimum) val = (int)this.control_.Minimum;
                if (val > this.control_.Maximum) val = (int)this.control_.Maximum;
                this.control_.Value = val;
                this.view_.StartEditing(this.control_, e.Item, e.SubItem);
                this.control_.Select(0, 10);
            }
        }

        private void view_SubItemEndEditing(object sender, Damdai.Forms.SubItemEndEditingEventArgs e)
        {
            if (e.SubItem == 1)
                e.Item.SubItems[e.SubItem].Text = this.control_.Value.ToString();
        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }
    }
}
