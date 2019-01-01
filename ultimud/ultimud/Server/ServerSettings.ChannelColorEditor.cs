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
    partial class Settings
    {
        [Obfuscation(Exclude = false)]
        class ChannelColorEditor : UITypeEditor
        {
            ComboBox color_ = new ComboBox();
            Damdai.Forms.ListView view_ = new Damdai.Forms.ListView();

            public ChannelColorEditor()
            {
                foreach (string color in Enum.GetNames(typeof(Telnet.Color)))
                    this.color_.Items.Add(color);

                this.view_.BorderStyle = BorderStyle.None;
                this.view_.HeaderStyle = ColumnHeaderStyle.Nonclickable;

                this.view_.Columns.Add("Channel");
                this.view_.Columns.Add("Color");

                this.view_.SubItemEndEditing += new Damdai.Forms.SubItemEndEditingEventHandler(this.view_SubItemEndEditing);
                this.view_.SubItemClicked += new Damdai.Forms.SubItemEventHandler(this.view_SubItemClicked);

                this.view_.BackColor = System.Drawing.Color.Black;
                this.view_.ForeColor = System.Drawing.Color.White;

                this.view_.Height = 175;

                this.color_.Parent = this.view_;
                this.color_.Visible = false;
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                if (value != null)
                {
                    Dictionary<Settings.Text, Telnet.Color> colors = value as Dictionary<Settings.Text, Telnet.Color>;

                    this.view_.Items.Clear();

                    string[] channels = Enum.GetNames(typeof(Settings.Text));
                    for (int x = 0; x < channels.Length; ++x)
                    {
                        Telnet.Color color = Telnet.Color.Reset;
                        colors.TryGetValue((Text)x, out color);
                        ListViewItem i = new ListViewItem(new string[] { channels[x], color.ToString() });
                        i.Tag = color;
                        i.ForeColor = Telnet.ColorToSystemColor(color);
                        this.view_.Items.Add(i);
                    }

                    //foreach (KeyValuePair<ServerSettings.Text, Telnet.Color> pair in colors)
                    //{
                    //    ListViewItem i = new ListViewItem(new string[] { pair.Key.ToString(), pair.Value.ToString() });
                    //    i.Tag = pair.Value;
                    //    i.ForeColor = Telnet.ColorToSystemColor(pair.Value);
                    //    this.view_.Items.Add(i);
                    //}

                    IWindowsFormsEditorService service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                    service.DropDownControl(this.view_);

                    foreach (ListViewItem i in this.view_.Items)
                    {
                        colors[(Settings.Text)i.Index] = (Telnet.Color)i.Tag;
                    }

                    return colors;
                }

                return value;
            }

            private void view_SubItemClicked(object sender, Damdai.Forms.SubItemEventArgs e)
            {
                if (e.SubItem == 1)
                {
                    this.color_.SelectedIndex = (int)(Telnet.Color)e.Item.Tag;
                    this.view_.StartEditing(this.color_, e.Item, e.SubItem);
                }
            }

            private void view_SubItemEndEditing(object sender, Damdai.Forms.SubItemEndEditingEventArgs e)
            {
                if (e.SubItem == 1)
                {
                    Telnet.Color color = (Telnet.Color)this.color_.SelectedIndex;
                    e.Item.SubItems[e.SubItem].Text = color.ToString();
                    e.Item.Tag = color;
                }
            }

            public override bool IsDropDownResizable
            {
                get
                {
                    return true;
                }
            }
        }
    }
}
