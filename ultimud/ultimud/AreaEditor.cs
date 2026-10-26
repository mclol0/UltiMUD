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
            this.ApplyModernAreaEditorStyle();

            this.SetArea(area);

            this.areaCanvas1.Editable = true;
            this.areaCanvas1.SelectedEvent += this.areaCanvas1_SelectedEvent;
            this.areaCanvas1.DeselectedEvent += this.areaCanvas1_DeselectedEvent;
            this.areaCanvas1.AreaSelectedEvent += this.areaCanvas1_AreaSelectedEvent;
            this.areaCanvas1.RedoAvailable += delegate() { this.toolStripButton2.Enabled = true; };
            this.areaCanvas1.RedoUnavailable += delegate() { this.toolStripButton2.Enabled = false; };
            this.areaCanvas1.UndoAvailable += delegate() { this.toolStripButton1.Enabled = true; };
            this.areaCanvas1.UndoUnavailable += delegate() { this.toolStripButton1.Enabled = false; };
            this.UpdateStatusHint("Pan Mode (Q): Drag the map to pan around, or click '+ Road/Path', '+ Room/Region', '+ Wall', or '+ Door' to start painting.");
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

        void ApplyModernAreaEditorStyle()
        {
            this.Font = StudioTheme.FontBase;
            this.Size = new Size(1200, 780);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "UltiMUD 2D Map & Area Builder";

            StudioTheme.StyleToolStrip(this.toolStrip1, true);
            StudioTheme.StyleToolStrip(this.toolStrip2, false);
            StudioTheme.StyleToolStrip(this.toolStrip3, false);
            StudioTheme.StyleStatusStrip(this.statusStrip1);
            StudioTheme.StylePropertyGrid(this.propertyGrid1);
            StudioTheme.StyleListView(this.listView1);

            ToolStripButton saveMapBtn = new ToolStripButton("💾 Save Map");
            saveMapBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            saveMapBtn.Font = StudioTheme.FontBold;
            saveMapBtn.ForeColor = Color.FromArgb(191, 219, 254);
            saveMapBtn.ToolTipText = "Save all map areas and layers";
            saveMapBtn.Click += this.toolStripButton1_Click;
            this.toolStrip1.Items.Insert(0, new ToolStripSeparator());
            this.toolStrip1.Items.Insert(0, saveMapBtn);

            ToolStripButton newAreaBtn = new ToolStripButton("🗺️ + New Area");
            newAreaBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            newAreaBtn.Font = StudioTheme.FontBold;
            newAreaBtn.ForeColor = Color.FromArgb(241, 245, 249);
            newAreaBtn.ToolTipText = "Create a brand new Area / Zone in the world";
            newAreaBtn.Click += this.toolStripButton6_Click;
            this.toolStrip1.Items.Insert(0, newAreaBtn);

            this.toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripButton1.Text = "Undo";

            this.toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripButton2.Text = "Redo";

            this.toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripDropDownButton1.Text = "+ Add Layer";

            this.toolStripButton10.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripButton10.Text = "Pan (Q)";

            this.toolStripButton11.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripButton11.Text = "Select (W)";

            this.toolStripButton12.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripButton12.Text = "Move (E)";

            this.toolStripButton13.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripButton13.Text = "Paint Tiles (R)";

            // Add 1-click map creation buttons right on the main toolbar
            this.toolStrip1.Items.Add(new ToolStripSeparator());

            ToolStripButton quickPathBtn = new ToolStripButton("🛣️ + Road/Path");
            quickPathBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            quickPathBtn.Font = StudioTheme.FontBold;
            quickPathBtn.ForeColor = Color.FromArgb(134, 239, 172);
            quickPathBtn.ToolTipText = "Create a new Road/Path layer and start painting tiles on the map";
            quickPathBtn.Click += this.toolStripButton3_Click;
            this.toolStrip1.Items.Add(quickPathBtn);

            ToolStripButton quickRoomBtn = new ToolStripButton("🏛️ + Room/Region");
            quickRoomBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            quickRoomBtn.Font = StudioTheme.FontBold;
            quickRoomBtn.ForeColor = Color.FromArgb(147, 197, 253);
            quickRoomBtn.ToolTipText = "Create a new Room/Region polygon and click corner points on the map";
            quickRoomBtn.Click += this.toolStripButton2_Click;
            this.toolStrip1.Items.Add(quickRoomBtn);

            ToolStripButton quickWallBtn = new ToolStripButton("🧱 + Wall");
            quickWallBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            quickWallBtn.Font = StudioTheme.FontBold;
            quickWallBtn.ForeColor = Color.FromArgb(253, 186, 116);
            quickWallBtn.ToolTipText = "Create a new impassable Wall layer and paint wall tiles";
            quickWallBtn.Click += this.toolStripButton5_Click;
            this.toolStrip1.Items.Add(quickWallBtn);

            ToolStripButton quickDoorBtn = new ToolStripButton("🚪 + Door");
            quickDoorBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            quickDoorBtn.Font = StudioTheme.FontBold;
            quickDoorBtn.ForeColor = Color.FromArgb(253, 224, 71);
            quickDoorBtn.ToolTipText = "Create a new openable/lockable Door on the map";
            quickDoorBtn.Click += this.toolStripButton7_Click;
            this.toolStrip1.Items.Add(quickDoorBtn);

            ToolStripButton quickExitBtn = new ToolStripButton("🌀 + Exit/Portal");
            quickExitBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            quickExitBtn.Font = StudioTheme.FontBold;
            quickExitBtn.ForeColor = Color.FromArgb(216, 180, 254);
            quickExitBtn.ToolTipText = "Create a new Exit/Portal that teleports players to another Area or coordinate";
            quickExitBtn.Click += this.toolStripButton4_Click;
            this.toolStrip1.Items.Add(quickExitBtn);

            this.toolStripButton9.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripButton9.Text = "Refresh Map Objects";
        }

        void UpdateStatusHint(string text)
        {
            if (this.toolStripStatusLabel1 != null)
                this.toolStripStatusLabel1.Text = text;
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
            T layer = new T();
            layer.Name = "New " + typeof(T).Name;
            this.areaCanvas1.Edit(layer);
            this.InitializeObjectSelector();
            this.toolStripButton13_Click(null, null);
            this.UpdateStatusHint("Painting new " + typeof(T).Name + ": Left-click map tiles to add points. Customize Name, Color & FillChar in the Property Inspector on the left. Press Esc when done.");
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
            this.UpdateStatusHint("Pan Mode (Q): Left-drag on the map to move the camera. Scroll wheel to zoom. Right-click any tile to place a Shop, Item Spawn, NPC Spawn, or Start Location.");
        }
        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            this.areaCanvas1.Mode = AreaCanvas.EditModes.Select;
            this.toolStripButton11.Checked = true;
            this.toolStripButton10.Checked = this.toolStripButton12.Checked = this.toolStripButton13.Checked = false;
            this.UpdateStatusHint("Select Mode (W): Left-click any tile on the map to select the Road, Room, Wall, Door, Shop, or Spawn at that tile.");
        }
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton12.Enabled)
            {
                this.areaCanvas1.Mode = AreaCanvas.EditModes.Move;
                this.toolStripButton12.Checked = true;
                this.toolStripButton11.Checked = this.toolStripButton10.Checked = this.toolStripButton13.Checked = false;
                this.UpdateStatusHint("Move Mode (E): Left-drag on the map to move the currently selected object or layer to a new tile coordinate.");
            }
        }
        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton13.Enabled)
            {
                this.areaCanvas1.Mode = AreaCanvas.EditModes.Manipulate;
                this.toolStripButton13.Checked = true;
                this.toolStripButton11.Checked = this.toolStripButton12.Checked = this.toolStripButton10.Checked = false;
                this.UpdateStatusHint("Paint Tiles Mode (R): Left-click tiles on the map to add/drag points for the selected Layer/Path/Region/Wall. Press Delete to remove a point, or Esc to finish.");
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
