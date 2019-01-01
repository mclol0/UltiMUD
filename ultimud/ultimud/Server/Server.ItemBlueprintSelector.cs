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
    public partial class Server
    {
        public class ItemBlueprintSelector : UITypeEditor
        {
            System.Windows.Forms.ListView view_ = new System.Windows.Forms.ListView();

            public ItemBlueprintSelector()
            {
                this.view_.BorderStyle = BorderStyle.None;
                this.view_.Size = new System.Drawing.Size(200, 400);
                this.view_.Columns.Add("Name", this.view_.Width - 5);
                this.view_.MultiSelect = false;
                this.view_.HeaderStyle = ColumnHeaderStyle.None;
                this.view_.View = View.Details;
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override bool IsDropDownResizable
            {
                get
                {
                    return true;
                }
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                this.view_.Items.Clear();

                foreach (Item itemBlueprint in Server.Instance.DataSets[typeof(Item)].Values)
                {
                    ListViewItem item = new ListViewItem(itemBlueprint.Name);
                    item.Tag = itemBlueprint;
                    this.view_.Items.Add(item);

                    if (itemBlueprint == value)
                    {
                        item.Selected = true;
                    }
                }

                IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                service.DropDownControl(this.view_);

                if (this.view_.SelectedItems.Count > 0)
                {
                    ListViewItem selection = this.view_.SelectedItems[0];
                    return selection.Tag as Item;
                }

                return null;
            }
        }
    }
}
