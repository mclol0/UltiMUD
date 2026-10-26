using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal partial class AreaEditor : Form
    {
        Area area_;
        Random rand_ = new Random();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        internal AreaEditor(Area area, Object o)
        {
            InitializeComponent();

            this.SetArea(area);

            this.areaCanvas1.Editable = true;
            this.areaCanvas1.SelectedEvent += this.areaCanvas1_SelectedEvent;
            this.areaCanvas1.DeselectedEvent += this.areaCanvas1_DeselectedEvent;
            this.areaCanvas1.AreaSelectedEvent += this.areaCanvas1_AreaSelectedEvent;
            this.areaCanvas1.RedoAvailable += delegate() { this.toolStripButton2.Enabled = true; };
            this.areaCanvas1.RedoUnavailable += delegate() { this.toolStripButton2.Enabled = false; };
            this.areaCanvas1.UndoAvailable += delegate() { this.toolStripButton1.Enabled = true; };
            this.areaCanvas1.UndoUnavailable += delegate() { this.toolStripButton1.Enabled = false; };
            this.toolStripStatusLabel1.Text = null;
            IntPtr hIcon = Ultimud.Properties.Resources.area.GetHicon();
            using (Icon tmpIcon = Icon.FromHandle(hIcon))
            {
                this.Icon = (Icon)tmpIcon.Clone();
            }
            DestroyIcon(hIcon);
            if (o != null)
            {
                this.areaCanvas1.Select(o);
                this.areaCanvas1.GoTo(o.Location);
            }
        }

        internal AreaEditor(Area area)
            : this(area, null)
        {
        }

        void InitializeObjectSelector()
        {
            this.listView1.Items.Clear();
            foreach (Object o in Damdai.Collections.Util.CopyList<Object>(this.area_.Children))
            {
                ListViewGroup g = this.listView1.Groups[o.GetType().Name];
                if (g == null)
                {
                    g = new ListViewGroup(o.GetType().Name, o.GetType().Name);
                    this.listView1.Groups.Add(g);
                }
                ListViewItem i = new ListViewItem(new string[] { o.ToString(), o.Location.Point.ToString() });
                i.Tag = o;
                i.Group = g;
                i.ImageIndex = o.IconIndex;
                this.listView1.Items.Add(i);
            }
            foreach (Reset o in Damdai.Collections.Util.CopyList<Reset>(this.area_.Resets))
            {
                ListViewGroup g = this.listView1.Groups[o.GetType().Name];
                if (g == null)
                {
                    g = new ListViewGroup(o.GetType().Name, o.GetType().Name);
                    this.listView1.Groups.Add(g);
                }
                ListViewItem i = new ListViewItem(new string[] { o.ToString(), o.Location.Point.ToString() });
                i.Tag = o;
                i.Group = g;
                this.listView1.Items.Add(i);
            }
            this.listView1.Sort();
        }
        void SetArea(Area area)
        {
            this.area_ = area;
            this.propertyGrid1.SelectedObject = this.area_;
            this.toolStripLabel1.Text = area.ToString();
            this.areaCanvas1.SetArea(this.area_);
            this.InitializeObjectSelector();
        }
        void areaCanvas1_AreaSelectedEvent(Area area)
        {
            this.SetArea(area);
        }
        void areaCanvas1_DeselectedEvent(object o)
        {
            this.propertyGrid1.SelectedObject = this.area_;
            this.toolStripLabel1.Text = this.area_.ToString();
            this.toolStripButton12.Enabled = this.toolStripButton13.Enabled = false;
            if (!this.toolStripButton10.Checked && !this.toolStripButton11.Checked) this.toolStripButton10_Click(null, null);
        }
        void areaCanvas1_SelectedEvent(object o)
        {
            this.propertyGrid1.SelectedObject = o;
            this.toolStripLabel1.Text = (o != null ? o.ToString() : string.Empty);
            this.toolStripButton12.Enabled = true;
            this.toolStripButton13.Enabled = (o is Area.Layer);
            foreach (ListViewItem i in this.listView1.Items)
            {
                if (i.Tag == o)
                {
                    i.Selected = true;
                    i.EnsureVisible();
                    break;
                }
            }
        }
        void Save()
        {
            Server.Instance.Save();
        }
        void New<T>() where T : Area.Layer, new()
        {
            this.areaCanvas1.Edit(new T());
            this.toolStripButton13_Click(null, null);
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.Save();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.New<Area.Region>();
        }
        private void layerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.New<Area.Layer>();
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.New<Area.Path>();
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            this.New<Area.Exit>();
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.New<Area.Wall>();
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Area area = new Area("New Area");
            this.areaCanvas1.toolStripComboBox1.Items.Add(area);
            this.SetArea(area);
        }
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            this.New<Area.Door>();
        }
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            this.New<Area.NpcRegion>();
        }
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            this.InitializeObjectSelector();
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.areaCanvas1.Invalidate();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (Area area in this.areaCanvas1.toolStripComboBox1.Items)
            {
                if (area.Parent == null && area != Server.Instance.MainArea)
                {
                    if (MessageBox.Show("There is an area with no location.\r\nIf you close this window, this area will be lost.\r\nDo you still want to close?",
                        "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        this.SetArea(area);
                        e.Cancel = true;
                        return;
                    }
                }
            }
            base.OnClosing(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!this.propertyGrid1.ContainsFocus)
            {
                switch (e.KeyChar)
                {
                    case 'q': this.toolStripButton10_Click(null, null); break;
                    case 'w': this.toolStripButton11_Click(null, null); break;
                    case 'e': this.toolStripButton12_Click(null, null); break;
                    case 'r': this.toolStripButton13_Click(null, null); break;
                    case (char)Keys.Q: this.toolStripButton10_Click(null, null); break;
                    case (char)Keys.W: this.toolStripButton11_Click(null, null); break;
                    case (char)Keys.E: this.toolStripButton12_Click(null, null); break;
                    case (char)Keys.R: this.toolStripButton13_Click(null, null); break;
                }
            }
            base.OnKeyPress(e);
        }
        protected override bool ProcessKeyPreview(ref Message m)
        {
            switch (m.Msg)
            {
                case 256:
                    switch ((Keys)(int)m.WParam)
                    {
                        case Keys.Delete:
                            if (this.propertyGrid1.SelectedGridItem != null && this.propertyGrid1.SelectedGridItem.Label == "Image")
                            {
                                this.area_.BackgroundImageSettings.Image = null;
                                this.propertyGrid1.SelectedGridItem = this.propertyGrid1.SelectedGridItem;
                                this.areaCanvas1.Invalidate();
                            }
                            break;
                        case Keys.Escape:
                            this.areaCanvas1.Deselect();
                            break;
                    }
                    break;
            }
            return base.ProcessKeyPreview(ref m);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Escape:
                    this.areaCanvas1.Deselect();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            ILocation o = this.listView1.SelectedItems[0].Tag as ILocation;
            if (o == null) return;
            else if (o is Area.Layer) this.areaCanvas1.Edit(o as Area.Layer);
            else this.areaCanvas1.Select(o);
            this.areaCanvas1.GoTo(o.Location);
            this.propertyGrid1.SelectedObject = o;
            this.toolStripLabel1.Text = o.ToString();
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                ListViewItem i = this.listView1.SelectedItems[0];
                if (i.Tag is Reset)
                {
                    Reset r = i.Tag as Reset;
                    this.area_.Resets.Remove(r);
                }
                else
                {
                    Object o = i.Tag as Object;
                    o.Parent = null;
                    if (this.areaCanvas1.SelectedObject == o) this.areaCanvas1.Deselect();
                }
                i.Remove();
                this.areaCanvas1.Invalidate();
            }
        }
        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ILocation o = this.listView1.SelectedItems[0].Tag as ILocation;
            this.areaCanvas1.Edit(null);
            this.areaCanvas1.GoTo(o.Location);
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            this.areaCanvas1.Mode = AreaCanvas.EditModes.Pan;
            this.toolStripButton10.Checked = true;
            this.toolStripButton11.Checked = this.toolStripButton12.Checked = this.toolStripButton13.Checked = false;
        }
        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            this.areaCanvas1.Mode = AreaCanvas.EditModes.Select;
            this.toolStripButton11.Checked = true;
            this.toolStripButton10.Checked = this.toolStripButton12.Checked = this.toolStripButton13.Checked = false;
        }
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton12.Enabled)
            {
                this.areaCanvas1.Mode = AreaCanvas.EditModes.Move;
                this.toolStripButton12.Checked = true;
                this.toolStripButton11.Checked = this.toolStripButton10.Checked = this.toolStripButton13.Checked = false;
            }
        }
        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton13.Enabled)
            {
                this.areaCanvas1.Mode = AreaCanvas.EditModes.Manipulate;
                this.toolStripButton13.Checked = true;
                this.toolStripButton11.Checked = this.toolStripButton12.Checked = this.toolStripButton10.Checked = false;
            }
        }
        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            this.areaCanvas1.Undo();
        }
        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            this.areaCanvas1.Redo();
        }
    }
}
