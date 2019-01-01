using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;

namespace Ultimud
{
    public class INpcSelector : UITypeEditor
    {
        NumericUpDown control_ = new NumericUpDown();
        Damdai.Forms.ListView view_ = new Damdai.Forms.ListView();

        public INpcSelector()
        {
            //this.control_.Maximum = 100;
            this.control_.Minimum = 0;
            this.view_.BorderStyle = BorderStyle.None;
            this.view_.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.view_.CheckBoxes = true;
            this.view_.Columns.Add("Npc");
            this.view_.Columns.Add("Chance/Cost");
            this.view_.SubItemEndEditing += this.view_SubItemEndEditing;
            this.view_.SubItemClicked += this.view_SubItemClicked;
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

            Dictionary<INpc, int> dict = value as Dictionary<INpc, int>;
            if (dict == null)
                dict = new Dictionary<INpc, int>();

            foreach (INpc npc in Globals.Server.GetBlueprints<INpc>())
            {
                int chance = 0;
                bool found = dict.TryGetValue(npc, out chance);
                ListViewItem i = new ListViewItem(new string[] { npc.Name, chance.ToString() });
                i.Tag = npc;
                i.Checked = found;
                this.view_.Items.Add(i);
            }

            IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            service.DropDownControl(this.view_);

            dict.Clear();
            foreach (ListViewItem i in this.view_.Items)
                if (i.Checked)
                    dict.Add(i.Tag as INpc, int.Parse(i.SubItems[1].Text));

            return dict;
        }
        private void view_SubItemClicked(object sender, Damdai.Forms.SubItemEventArgs e)
        {
            if (e.SubItem == 1)
            {
                this.control_.Value = int.Parse(e.Item.SubItems[e.SubItem].Text);
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
