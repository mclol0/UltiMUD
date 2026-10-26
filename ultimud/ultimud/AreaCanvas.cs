using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Damdai;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal partial class AreaCanvas : UserControl
    {
        internal enum View
        {
            Game,
            Design1,
            Design2,
        }

        internal enum EditModes
        {
            Pan,
            Select,
            Move,
            Manipulate
        }

        int zoom_ = 12;
        Point offset_ = new Point(0, 0);
        Area area_;
        Point selectedpoint_ = Point.Empty;
        bool pointselected_;
        protected Point lastpoint_ = Point.Empty;
        protected Dictionary<MouseButtons, Point> lastMouseClickLocation_ = new Dictionary<MouseButtons, Point>();
        Point mark_ = Point.Empty;
        bool editable_ = false;
        protected List<Keys> keysPressed_ = new List<Keys>();
        View view_ = View.Design1;
        object selectedObject_ = null;
        System.Windows.Forms.Timer redrawtimer_ = new System.Windows.Forms.Timer();
        EditModes mode_ = EditModes.Pan;
        Dictionary<EditModes, Cursor> cursors_ = new Dictionary<EditModes, Cursor>();
        Dictionary<Area, Stack<Area.Layer>> undo_ = new Dictionary<Area, Stack<Area.Layer>>();
        Dictionary<Area, Stack<Area.Layer>> redo_ = new Dictionary<Area, Stack<Area.Layer>>();
        Area.Layer unmodifiedlayer_;

        #region Properties
        public int Zoom
        {
            get { return this.zoom_; }
            set
            {
                Point center = this.Center;
                if (this.Area != null)
                {
                    int delta = value - this.Zoom;
                    this.offset_.Offset(delta, delta);
                    if (this.Editable && this.Area.BackgroundImageSettings != null)
                    {
                        int widthChange = this.area_.BackgroundImageSettings.Rect.Width / this.Zoom * delta;
                        int heightChange = this.area_.BackgroundImageSettings.Rect.Height / this.Zoom * delta;
                        this.Area.BackgroundImageSettings.Rect =
                            new Rectangle(this.Area.BackgroundImageSettings.Rect.Location,
                            new Size(this.Area.BackgroundImageSettings.Rect.Width + widthChange, this.Area.BackgroundImageSettings.Rect.Height + heightChange));
                    }
                }
                this.zoom_ = value;
                this.GoTo(center);
                //this.Invalidate();
            }
        }
        internal Point Offset
        {
            get { return this.offset_; }
        }
        public object SelectedObject
        {
            get { return this.selectedObject_; }
            set { this.selectedObject_ = value; }
        }
        public Point SelectedPoint
        {
            get { return this.selectedpoint_; }
            set { this.selectedpoint_ = value; }
        }
        [Browsable(false), DefaultValue(null)]
        internal Area Area
        {
            get { return this.area_; }
        }
        public bool Editable
        {
            get { return this.editable_; }
            set
            {
                this.editable_ = value;
                if (this.editMapBtn_ != null)
                    this.editMapBtn_.Visible = !value;
            }
        }
        public Point Mark
        {
            get { return this.mark_; }
            set { this.mark_ = value; }
        }
        internal Rectangle ViewPort
        {
            get
            {
                return new Rectangle(
                    this.Offset.X, this.Offset.Y,
                    ((this.Width - (this.Width % this.Zoom)) / this.Zoom) + 1,
                    ((this.Height - (this.Height % this.Zoom)) / this.Zoom) + 1);
            }
        }
        Point Center
        {
            get
            {
                Rectangle rect = this.ViewPort;
                return new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
            }
        }
        List<Area> Areas
        {
            set
            {
                this.toolStripComboBox1.Items.Clear();
                foreach (Area area in value)
                {
                    this.toolStripComboBox1.Items.Add(area);
                }
            }
        }
        protected bool PointSelected
        {
            get { return this.pointselected_; }
            set { this.pointselected_ = value; }
        }
        public View ViewType
        {
            get { return this.view_; }
            set
            {
                this.view_ = value;
                switch (value)
                {
                    case View.Design1:
                        this.BackColor = Color.FromArgb(24, 33, 47);
                        this.gameToolStripMenuItem.Checked = false;
                        this.designexactToolStripMenuItem.Checked = true;
                        this.designfastToolStripMenuItem.Checked = false;
                        break;
                    case View.Design2:
                        this.BackColor = Color.FromArgb(24, 33, 47);
                        this.gameToolStripMenuItem.Checked = false;
                        this.designexactToolStripMenuItem.Checked = false;
                        this.designfastToolStripMenuItem.Checked = true;
                        break;
                    case View.Game:
                        this.BackColor = Color.Black;
                        this.gameToolStripMenuItem.Checked = true;
                        this.designexactToolStripMenuItem.Checked = false;
                        this.designfastToolStripMenuItem.Checked = false;
                        break;
                }
                this.Invalidate();
            }
        }
        internal EditModes Mode
        {
            get { return this.mode_; }
            set
            {
                this.mode_ = value;
                switch (value)
                {
                    case EditModes.Manipulate: this.Cursor = this.cursors_[EditModes.Manipulate]; break;
                    case EditModes.Move: this.Cursor = this.cursors_[EditModes.Move]; break;
                    case EditModes.Pan: this.Cursor = Cursors.SizeAll; break;
                    case EditModes.Select: this.Cursor = Cursors.Arrow; break;
                }
                this.Invalidate();
            }
        }
        #endregion

        #region Events

        internal event Function_void<Point> UpdateLocationEvent;
        internal event Function_void<Point> LocationClickedEvent;
        internal event Function_void<object> SelectedEvent;
        internal event Function_void<object> DeselectedEvent;
        internal event Function_void<Area> AreaSelectedEvent;
        internal event Function_void<Area> EditAreaRequested;
        internal event Function_void UndoAvailable;
        internal event Function_void RedoAvailable;
        internal event Function_void UndoUnavailable;
        internal event Function_void RedoUnavailable;

        ToolStripButton editMapBtn_;

        void Selected(object o)
        {
            this.selectedObject_ = o;
            if (this.SelectedEvent != null)
                this.SelectedEvent(o);
        }
        void Deselected(object o)
        {
            if (this.DeselectedEvent != null)
                this.DeselectedEvent(o);

            if (o is Area.Layer)
            {
                Area.Layer layer = o as Area.Layer;
                if (layer != null && layer.Points.Count == 0)
                    layer.Parent = null;
            }

            this.selectedObject_ = null;
            this.Invalidate();
        }

        #endregion

        public AreaCanvas()
        {
            InitializeComponent();
            StudioTheme.StyleToolStrip(this.toolStrip1, false);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.BackColor = Color.FromArgb(24, 33, 47);

            this.toolStripLabel1.Text = "Tile: 0, 0";
            this.toolStripLabel1.Font = StudioTheme.FontBold;
            this.toolStripLabel1.ForeColor = StudioTheme.TextSecondary;
            this.designexactToolStripMenuItem.Checked = true;

            // Insert clear labels and quick navigation buttons into AreaCanvas toolbar
            ToolStripLabel areaLbl = new ToolStripLabel("🗺️ Area:");
            areaLbl.Font = StudioTheme.FontBold;
            this.toolStrip1.Items.Insert(0, areaLbl);

            int zoomIdx = this.toolStrip1.Items.IndexOf(this.toolStripComboBox2);
            if (zoomIdx >= 0)
            {
                ToolStripLabel zoomLbl = new ToolStripLabel("🔍 Zoom:");
                zoomLbl.Font = StudioTheme.FontBold;
                this.toolStrip1.Items.Insert(zoomIdx, zoomLbl);
            }

            this.toolStripDropDownButton2.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            this.toolStripDropDownButton2.Text = "View Mode";

            ToolStripButton centerStartBtn = new ToolStripButton("⌖ Center Start");
            centerStartBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            centerStartBtn.Font = StudioTheme.FontBold;
            centerStartBtn.ForeColor = StudioTheme.AccentTeal;
            centerStartBtn.ToolTipText = "Center map camera on this Area's starting spawn tile";
            centerStartBtn.Click += delegate
            {
                if (this.Area != null)
                    this.GoTo(this.Area.Start);
            };
            this.toolStrip1.Items.Add(new ToolStripSeparator());
            this.toolStrip1.Items.Add(centerStartBtn);

            this.editMapBtn_ = new ToolStripButton("✏️ Edit Map && Paint Rooms/Paths");
            this.editMapBtn_.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.editMapBtn_.Font = StudioTheme.FontBold;
            this.editMapBtn_.ForeColor = StudioTheme.AccentBlue;
            this.editMapBtn_.ToolTipText = "Open the interactive 2D Map & Area Editor for this Area (or double-click anywhere on the map)";
            this.editMapBtn_.Visible = !this.editable_;
            this.editMapBtn_.Click += delegate
            {
                if (this.Area != null && this.EditAreaRequested != null)
                    this.EditAreaRequested(this.Area);
            };
            this.toolStrip1.Items.Add(new ToolStripSeparator());
            this.toolStrip1.Items.Add(this.editMapBtn_);

            if (Server.Instance != null)
                this.Areas = Server.Instance.GetAreas();
            
            this.MouseWheel += new MouseEventHandler(AreaCanvas_MouseWheel);
            this.DoubleClick += delegate
            {
                if (!this.editable_ && this.Area != null && this.EditAreaRequested != null)
                    this.EditAreaRequested(this.Area);
            };
            this.toolStripComboBox2.Items.AddRange(new object[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });
            this.toolStripComboBox2.SelectedIndex = 10;
            this.redrawtimer_.Tick += new EventHandler(redrawtimer__Tick);
            this.redrawtimer_.Interval = 5000;
            this.redrawtimer_.Start();

            this.cursors_.Add(EditModes.Manipulate, new Cursor(new System.IO.MemoryStream(Ultimud.Properties.Resources.pen_r)));
            this.cursors_.Add(EditModes.Move, new Cursor(new System.IO.MemoryStream(Ultimud.Properties.Resources.hmove)));

            this.Mode = EditModes.Pan;
        }

        void redrawtimer__Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }
        void AreaCanvas_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (this.toolStripComboBox2.SelectedIndex < this.toolStripComboBox2.Items.Count - 1)
                {
                    this.toolStripComboBox2.SelectedIndex++;
                }
            }
            else if (this.Zoom > 2)
            {
                this.toolStripComboBox2.SelectedIndex--;
            }
        }
        internal void UpdateComboBox()
        {
            if (Server.Instance != null)
            {
                this.Areas = Server.Instance.GetAreas();
            }
        }
        internal void SetArea(Area a)
        {
            if (this.Area != a)
            {
                this.Deselect();
                this.area_ = a;

                if (a != null)
                {
                    this.GoTo(this.Area.Start);
                    if (!this.Editable)
                        this.UpdateComboBox();
                    this.toolStripComboBox1.SelectedItem = a;
                    if (!this.undo_.ContainsKey(a)) this.undo_.Add(a, new Stack<Area.Layer>());
                    if (!this.redo_.ContainsKey(a)) this.redo_.Add(a, new Stack<Area.Layer>());
                }
                else
                    this.Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.Visible || this.Area == null || (e.ClipRectangle.Y == 0 && e.ClipRectangle.Height <= 25))
                return;

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(this.BackColor);
            if (this.Area.BackgroundImageSettings != null && this.Area.BackgroundImageSettings.Visible)
                this.DrawBackgroundImage(g);
            this.DrawGrid(g);
            SortedDictionary<int, List<Object>> objects = this.DrawObjects(g);//, this.ClientRectToArea(e.ClipRectangle));
            this.DrawSpecialSymbols(g);
            this.DrawSelectionScreen(g);
            this.DrawCanvasOverlayBanner(g);
        }

        void DrawCanvasOverlayBanner(Graphics g)
        {
            if (this.Height < 90 || this.Width < 260)
                return;

            string hint;
            if (!this.Editable)
            {
                hint = "🖱️ Drag: Pan  |  Scroll: Zoom  |  Double-Click Map (or click 'Edit Map') to Paint Rooms, Roads & Spawns  |  🟢 Green Circle = Start Tile";
            }
            else
            {
                string modeDesc = "Pan Camera (Drag map)";
                if (this.mode_ == EditModes.Select) modeDesc = "Select Object (Left-click tile object)";
                else if (this.mode_ == EditModes.Move) modeDesc = "Move Selected Object (Drag object)";
                else if (this.mode_ == EditModes.Manipulate) modeDesc = "Paint / Edit Points (Left-click tiles to paint points • Delete key removes point)";
                hint = "🛠️ Active Tool: " + modeDesc + "  |  Right-Click Tile: Add Shop / Item Spawn / NPC Spawn / Set Start";
            }

            Rectangle barRect = new Rectangle(8, this.Height - 30, System.Math.Min(this.Width - 16, 760), 22);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(195, 15, 23, 42)))
            using (Pen border = new Pen(Color.FromArgb(140, 71, 85, 105)))
            using (SolidBrush fg = new SolidBrush(Color.FromArgb(226, 232, 240)))
            {
                g.FillRectangle(bg, barRect);
                g.DrawRectangle(border, barRect);
                g.DrawString(hint, StudioTheme.FontSmall, fg, new RectangleF(barRect.X + 8, barRect.Y + 3, barRect.Width - 12, barRect.Height - 4));
            }
        }

        void DrawGrid(Graphics g)
        {
            switch (this.view_)
            {
                case View.Design1:
                    using (Pen pen = new Pen(Color.FromArgb(51, 65, 85)))
                    {
                        for (int i = 0; i <= this.Width / this.Zoom; ++i)
                        {
                            float x = i * this.Zoom;
                            g.DrawLine(pen, x, 0, x, this.Height);
                        }
                        for (int j = 0; j <= this.Height / this.Zoom; ++j)
                        {
                            float y = j * this.Zoom;
                            g.DrawLine(pen, 0, y, this.Width, y);
                        }
                    }
                    break;
                case View.Design2:
                    if (this.Zoom >= 6)
                    {
                        using (Pen pen = new Pen(Color.FromArgb(35, 47, 66)))
                        {
                            for (int i = 0; i <= this.Width / this.Zoom; ++i)
                            {
                                float x = i * this.Zoom;
                                g.DrawLine(pen, x, 0, x, this.Height);
                            }
                            for (int j = 0; j <= this.Height / this.Zoom; ++j)
                            {
                                float y = j * this.Zoom;
                                g.DrawLine(pen, 0, y, this.Width, y);
                            }
                        }
                    }
                    break;
                case View.Game:
                    Font f = new Font("fixedsys", System.Math.Max((int)(this.Zoom / 1.7), 1), FontStyle.Bold);
                    SolidBrush b = new SolidBrush(Telnet.ColorToSystemColor(this.Area.FillColor));
                    SolidBrush b2 = null;
                    Random rand = null;
                    if (this.area_.FillChar2 > 0)
                    {
                        rand = new Random();
                        b2 = new SolidBrush(Telnet.ColorToSystemColor(this.Area.FillColor2));
                    }

                    for (int x = 0; x <= this.Width / this.Zoom; ++x)
                    {
                        int X = x * this.Zoom;
                        for (int y = 0; y <= this.Height / this.Zoom; ++y)
                        {
                            SolidBrush brush = null;
                            char c;
                            if (rand != null && rand.Next(100) <= this.area_.FillChar2Percent)
                            {
                                brush = b2;
                                c = this.Area.FillChar2;
                            }
                            else
                            {
                                brush = b;
                                c = this.Area.FillChar;
                            }

                            g.DrawString(c.ToString(), f, brush, X, y * this.Zoom);
                        }
                    }
                    if (b != null) b.Dispose();
                    if (b2 != null) b2.Dispose();
                    if (f != null) f.Dispose();
                    break;
            }
        }
        SortedDictionary<int, List<Object>> DrawObjects(Graphics g)//, Rectangle clipRectangle)
        {
            Object[] children;
            try
            {
                lock (this.area_.Children)
                {
                    children = this.area_.Children.ToArray();
                }
            }
            catch (Exception)
            {
                return new SortedDictionary<int, List<Object>>();
            }
            SortedDictionary<int, List<Object>> objects = new SortedDictionary<int, List<Object>>();
            foreach (Object child in children)
            {
                if (child != null && this.Contains(child) && child != this.SelectedObject)// && child.Intersects(clipRectangle))
                {
                    if (child is Area.Layer)
                    {
                        Area.Layer layer = child as Area.Layer;
                        if (!objects.ContainsKey(layer.ZIndex))
                            objects.Add(layer.ZIndex, new List<Object>());
                        objects[layer.ZIndex].Add(layer);
                    }
                    else
                    {
                        int index = int.MaxValue - 1;
                        if (!objects.ContainsKey(index))
                            objects.Add(index, new List<Object>());
                        objects[index].Add(child);
                    }
                }
            }

            if (this.SelectedObject is Object)
            {
                if (!objects.ContainsKey(int.MaxValue))
                    objects.Add(int.MaxValue, new List<Object>());
                objects[int.MaxValue].Add(this.SelectedObject as Object);
            }

            foreach (KeyValuePair<int, List<Object>> pair in objects)
                foreach (Object obj in pair.Value)
                    this.Draw(g, obj);

            return objects;
        }
        void DrawSpecialSymbols(Graphics g)
        {
            Rectangle viewPort = this.ViewPort;
            if (viewPort.Contains(this.Area.Start))
            {
                using (Pen pen = new Pen(Color.LimeGreen))
                    g.DrawEllipse(
                        pen,
                        (this.Area.Start.X - this.Offset.X) * this.Zoom,
                        (this.Area.Start.Y - this.Offset.Y) * this.Zoom,
                        this.Zoom,
                        this.Zoom);
            }

            if (this.Mark != Point.Empty && viewPort.Contains(this.Mark))
            {
                using (Pen pen = new Pen(Color.Red, 1))
                    g.DrawEllipse(
                        pen,
                        (this.Mark.X - this.Offset.X) * this.Zoom + 1,
                        (this.Mark.Y - this.Offset.Y) * this.Zoom + 1,
                        this.Zoom - 2,
                        this.Zoom - 2);
            }

            using (Pen pen = new Pen(Color.Magenta, 1))
            {
                Reset[] resets;
                try
                {
                    lock (this.Area.Resets)
                    {
                        resets = this.Area.Resets.ToArray();
                    }
                }
                catch (Exception)
                {
                    resets = new Reset[0];
                }
                foreach (Reset reset in resets)
                {
                    if (reset != null && viewPort.Contains(reset.Location.Point))
                    {
                        g.DrawEllipse(
                            pen,
                            (reset.Location.Point.X - this.Offset.X) * this.Zoom + 2,
                            (reset.Location.Point.Y - this.Offset.Y) * this.Zoom + 2,
                            this.Zoom - 4,
                            this.Zoom - 4);
                    }
                }
            }
        }
        void DrawMouseLocation(Graphics g)
        {
            using (Pen pen = new Pen(Color.Gold))
            {
                Point p = this.PointToClient(MousePosition);
                int x = p.X - (p.X % this.Zoom);
                int y = p.Y - (p.Y % this.Zoom);
                g.DrawRectangle(pen, x, y, this.Zoom, this.Zoom);
                if (this.UpdateLocationEvent != null) this.UpdateLocationEvent(new Point(x / this.Zoom + this.Offset.X, y / this.Zoom + this.Offset.Y));
            }
        }
        void Draw(Graphics g, Object o)
        {
            o.Draw(g, this, this.view_);
        }
        void DrawBackgroundImage(Graphics g)
        {
            if (this.Editable && this.Area.BackgroundImageSettings != null && this.Area.BackgroundImageSettings.Image != null)
            {
                using (ImageAttributes imgAttributes = new ImageAttributes())
                {
                    ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                    new float[] {1,0,0,0,0}, new float[] {0,1,0,0,0}, new float[] {0,0,1,0,0}, new float[] {0,0,0,.2F,0}, new float[] {0,0,0,0,1}});
                    imgAttributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(
                        this.Area.BackgroundImageSettings.Image, 
                        this.ClientRectangle,
                        (this.Offset.X + this.Area.BackgroundImageSettings.Rect.X) * this.Zoom,
                        (this.Offset.Y + this.Area.BackgroundImageSettings.Rect.Y) * this.Zoom, 
                        this.Width, this.Height, GraphicsUnit.Pixel, imgAttributes);
                }
            }
        }
        void DrawSelectionScreen(Graphics g)
        {
            if (!(this.selectedObject_ is Area.Layer) && this.selectedObject_ is ILocation)
            {
                ILocation o = this.selectedObject_ as ILocation;
                int xpos = (o.Location.Point.X - this.Offset.X) * this.Zoom;
                int ypos = (o.Location.Point.Y - this.Offset.Y) * this.Zoom;
                Rectangle rect = new Rectangle(xpos, ypos, this.zoom_, this.zoom_);
                rect.Inflate(rect.Width, rect.Height);
                using (System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    gp.AddEllipse(rect);
                    gp.AddRectangle(new Rectangle(0, 0, this.Width, this.Height));
                    using (Region r = new Region(gp))
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(125, Color.Black)))
                    {
                        g.FillRegion(brush, r);
                    }
                }
                using (Pen p = new Pen(Color.White, 1))
                    g.DrawEllipse(p, rect);
            }
        }
        protected Point ScreenPointToArea(Point point)
        {
            Point p = this.PointToClient(point);
            return this.ClientPointToArea(p);
        }
        protected Point ClientPointToArea(Point point)
        {
            int x = point.X - (point.X % this.Zoom);
            int y = point.Y - (point.Y % this.Zoom);
            return new Point(x / this.Zoom + this.Offset.X, y / this.Zoom + this.Offset.Y);
        }
        protected Rectangle ClientRectToArea(Rectangle rect)
        {
            rect.Location = this.ClientPointToArea(rect.Location);
            return rect;
        }
        private void AreaCanvas2_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = Point.Empty;
            bool invalidate = false;

            switch (this.mode_)
            {
                case EditModes.Move:
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            if (this.SelectedObject != null)
                            {
                                Point point = this.ScreenPointToArea(MousePosition);
                                if (this.lastpoint_ != Point.Empty)
                                {
                                    if (this.selectedObject_ is Area.Layer && this.unmodifiedlayer_ == null) this.unmodifiedlayer_ = ((Area.Layer)this.selectedObject_).Clone() as Area.Layer;
                                    p = ((ILocation)this.SelectedObject).Location.Point;
                                    ((ILocation)this.SelectedObject).Location = new Location(this.Area, new Point(p.X + (point.X - this.lastpoint_.X), p.Y + (point.Y - this.lastpoint_.Y)));
                                    invalidate = true;
                                }
                                this.lastpoint_ = point;
                            }
                            break;
                    }
                    break;
                case EditModes.Manipulate:
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            if (this.pointselected_ && this.SelectedObject is Area.Layer)
                            {
                                Area.Layer layer = this.SelectedObject as Area.Layer;
                                int n = layer.Points.IndexOf(this.SelectedPoint);
                                if (n > -1)
                                {
                                    if (this.unmodifiedlayer_ == null) this.unmodifiedlayer_ = layer.Clone() as Area.Layer;
                                    layer.Points.RemoveAt(n);
                                    this.selectedpoint_ = this.ScreenPointToArea(MousePosition);
                                    layer.Points.Insert(n, this.SelectedPoint);
                                    invalidate = true;
                                }
                            }
                            break;
                    }
                    break;
                case EditModes.Pan:
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            if (this.lastpoint_ != Point.Empty)
                            {
                                Point p1 = MousePosition;
                                Point p2 = this.lastpoint_;
                                Point lastOffset = this.offset_;
                                this.offset_.Offset((p2.X - p1.X) / this.Zoom, (p2.Y - p1.Y) / this.Zoom);
                                if (lastOffset != this.offset_)
                                {
                                    this.lastpoint_ = MousePosition;
                                    invalidate = true;
                                }
                            }
                            else
                                this.lastpoint_ = MousePosition;
                            break;
                    }
                    break;
            }

            p = this.PointToClient(MousePosition);
            int x = p.X - (p.X % this.Zoom);
            int y = p.Y - (p.Y % this.Zoom);
            this.toolStripLabel1.Text = "Tile: " + (x / this.Zoom + this.Offset.X) + ", " + (y / this.Zoom + this.Offset.Y);

            if (invalidate)
                this.Invalidate();
        }
        private void AreaCanvas2_MouseDown(object sender, MouseEventArgs e)
        {
            Point p = this.ScreenPointToArea(MousePosition);
            if (this.Editable)
            {
                switch (this.mode_)
                {
                    case EditModes.Select:
                        switch (e.Button)
                        {
                            case MouseButtons.Left:
                                ContextMenu menu = new ContextMenu();
                                foreach (Object o in this.area_.GetChildren<Object>(p))
                                {
                                    MenuItem mi = new MenuItem(o.ToString(), delegate(object sender2, EventArgs args) { this.Select(((MenuItem)sender2).Tag); });
                                    mi.Tag = o;
                                    menu.MenuItems.Add(mi);
                                }
                                foreach (Reset o in this.area_.Resets.FindAll(delegate(Reset r) { return r.Location.Point == p; }))
                                {
                                    MenuItem mi = new MenuItem(o.ToString(), delegate(object sender2, EventArgs args) { this.Select(((MenuItem)sender2).Tag); });
                                    mi.Tag = o;
                                    menu.MenuItems.Add(mi);
                                }
                                if (menu.MenuItems.Count == 1)
                                    this.Select(menu.MenuItems[0].Tag);
                                else
                                    menu.Show(this, this.PointToClient(MousePosition));
                                break;
                        }
                        break;
                    case EditModes.Manipulate:
                        switch (e.Button)
                        {
                            case MouseButtons.Left:
                                if (this.SelectedObject is Area.Layer)
                                {
                                    Area.Layer layer = this.SelectedObject as Area.Layer;
                                    if (layer.Points.Contains(p))
                                    {
                                        this.selectedpoint_ = p;
                                        this.pointselected_ = true;
                                    }
                                    else
                                    {
                                        this.AddUndo(layer, true);

                                        if (this.pointselected_ && layer.Points.Contains(this.selectedpoint_))
                                            layer.Points.Insert(layer.Points.IndexOf(this.selectedpoint_), p);
                                        else
                                            layer.Points.Add(p);

                                        this.selectedpoint_ = p;
                                        this.pointselected_ = true;
                                    }
                                }
                                break;
                        }
                        break;
                }

                this.Invalidate();
            }

            this.lastMouseClickLocation_[MouseButtons.Left] = MousePosition;
        }
        private void AreaCanvas2_MouseUp(object sender, MouseEventArgs e)
        {
            Point p = this.ScreenPointToArea(MousePosition);
            if (this.LocationClickedEvent != null) this.LocationClickedEvent(p);

            switch (e.Button)
            {
                case MouseButtons.Left:
                    this.lastpoint_ = Point.Empty;
                    if (this.unmodifiedlayer_ != null)
                    {
                        this.AddUndo(this.unmodifiedlayer_, true);
                        this.unmodifiedlayer_ = null;
                    }
                    break;
                case MouseButtons.Right:
                    this.lastpoint_ = p;
                    this.lastMouseClickLocation_[MouseButtons.Right] = MousePosition;
                    break;
            }
        }
        private void AreaCanvas2_KeyDown(object sender, KeyEventArgs e)
        {
            if (!this.keysPressed_.Contains(e.KeyCode))
            {
                this.keysPressed_.Add(e.KeyCode);
                if (e.KeyCode == Keys.G && this.keysPressed_.Contains(Keys.ControlKey))
                    this.GoTo();
            }
        }
        private void AreaCanvas2_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.keysPressed_.Contains(e.KeyCode))
                this.keysPressed_.Remove(e.KeyCode);
        }
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    this.offset_.X -= (e.Control ? 5 : 1);
                    this.Invalidate();
                    break;
                case Keys.Right:
                    this.offset_.X += (e.Control ? 5 : 1);
                    this.Invalidate();
                    break;
                case Keys.Up:
                    this.offset_.Y -= (e.Control ? 5 : 1);
                    this.Invalidate();
                    break;
                case Keys.Down:
                    this.offset_.Y += (e.Control ? 5 : 1);
                    this.Invalidate();
                    break;
                case Keys.Z:
                    if (e.Control) this.Undo();
                    break;
                case Keys.Y:
                    if (e.Control) this.Redo();
                    break;
            }

            this.AreaCanvas2_KeyDown(null, e);
        }
        override protected bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys k)
        {
            switch (k)
            {
                case Keys.Escape:
                    this.Deselect();
                    return true;
                case Keys.Delete:
                    if (this.SelectedObject is Area.Layer && this.pointselected_)
                        this.DeleteSelectedPoint();
                    else if(this.SelectedObject != null)
                        this.DeleteSelectedObject();

                    this.Invalidate();
                    return false;
                case Keys.Space:
                    if (this.SelectedObject != null)
                        this.GoTo(((ILocation)this.SelectedObject).Location.Point);
                    break;
            }

            return false;
        }
        internal void Select(object o)
        {
            this.Selected(o);
            this.Invalidate();
            this.Focus();
        }
        internal void Deselect()
        {
            if (this.Editable)
                this.Deselected(this.SelectedObject);
        }
        void DeleteSelectedPoint()
        {
            Area.Layer layer = this.SelectedObject as Area.Layer;
            if(layer != null && layer.Points.Contains(this.SelectedPoint))
            {
                Point selectedpoint = this.selectedpoint_;
                int index = layer.Points.IndexOf(selectedpoint);
                if (layer.Points.Count > index + 1)
                {
                    this.selectedpoint_ = layer.Points[index + 1];
                    this.pointselected_ = true;
                }
                else
                {
                    this.selectedpoint_ = Point.Empty;
                    this.pointselected_ = false;
                }

                layer.Points.Remove(selectedpoint);
                if (layer.Points.Count == 0)
                {
                    layer.Parent = null;
                    this.Deselected(null);
                    this.selectedObject_ = null;
                }
            }
        }
        void DeleteSelectedObject()
        {
            if (this.SelectedObject != null && 
                MessageBox.Show("Delete selected object?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (this.SelectedObject is Object)
                    ((Object)this.SelectedObject).Parent = null;
                else if (this.SelectedObject is Reset)
                    this.area_.Resets.Remove(this.SelectedObject as Reset);
                this.Deselected(null);
                this.selectedObject_ = null;
            }
        }
        internal bool Contains(Object o)
        {
            return o.Intersects(this.ViewPort);
        }
        internal void Edit(Area.Layer layer)
        {
            if (this.SelectedObject != null)
                this.Deselected(this.SelectedObject);

            this.pointselected_ = false;

            if (layer != null)
            {
                layer.Parent = this.Area;
                this.Select(layer);
                if (layer.Points.Count > 0)
                    this.GoTo(layer.Center);
            }
        }
        void GoTo()
        {
            PropertyGrid propGrid = new PropertyGrid();
            propGrid.ToolbarVisible = false;
            propGrid.HelpVisible = false;
            propGrid.PropertySort = PropertySort.Alphabetical;
            propGrid.SelectedObject = new Point();
            Damdai.Forms.Dialog dialog = new Damdai.Forms.Dialog(propGrid, "Enter Coordinates");
            dialog.Size = new Size(225, 125);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.GoTo((Point)propGrid.SelectedObject);
            }
        }
        internal void GoTo(Point p)
        {
            this.offset_.X = p.X - this.Width / this.Zoom / 2;
            this.offset_.Y = p.Y - this.Height / this.Zoom / 2;
            this.Invalidate();
        }
        internal void GoTo(Location loc)
        {
            this.SetArea(loc.Area);
            this.GoTo(loc.Point);
        }
        private void setThisLocationAsTheStartingLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Area.Start = this.lastpoint_;
            this.Invalidate();
        }
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (!this.Editable)
                e.Cancel = true;
            else
                this.deleteToolStripMenuItem.Enabled = (this.SelectedObject != null);
        }
        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.GoTo();
        }
        private void addShopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shop shop = new Shop();
            shop.Location.Point = this.lastpoint_;
            shop.Parent = this.Area;
            this.Select(shop);
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DeleteSelectedObject();
        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SetArea(this.toolStripComboBox1.SelectedItem as Area);
            this.ResetFocus();
            if (this.AreaSelectedEvent != null)
                this.AreaSelectedEvent(this.Area);
        }
        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Zoom = (int)this.toolStripComboBox2.SelectedItem;
            this.ResetFocus();
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.ViewType = View.Game;
        }
        private void DoInvalidate(object sender, EventArgs e)
        {
            this.Invalidate();
        }
        void ResetFocus()
        {
            this.toolStrip1.Enabled = false;
            this.Focus();
            this.toolStrip1.Enabled = true;
        }
        private void itemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reset reset = new ItemReset(new Location(this.Area, this.lastpoint_));
            this.Area.Resets.Add(reset);
            this.Selected(reset);
            this.Invalidate();
        }
        private void npcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Reset reset = new NpcReset(new Location(this.Area, this.lastpoint_));
            this.Area.Resets.Add(reset);
            this.Selected(reset);
            this.Invalidate();
        }
        private void thisLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Area you sure you want to purge this location of items and npcs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Trace.WriteLine("Purging location: " + this.Area.Name + " " + this.lastpoint_);
                foreach (Object o in this.Area.GetChildren<Object>(this.lastpoint_))
                    if (o is Item || o is Npc)
                        o.Parent = null;
            }
        }
        private void areaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Area you sure you want to purge this area of all items and npcs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Trace.WriteLine("Purging area: " + this.Area.Name);
                this.PurgeArea(this.area_);
            }
        }
        private void worldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Area you sure you want to purge ALL items and npcs?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Trace.WriteLine("Purging world...");
                foreach (Area area in Server.Instance.GetAreas())
                    this.PurgeArea(area);
            }
        }
        void PurgeArea(Area area)
        {
            Debug.WriteLine("Purging area: " + area.Name);
            foreach (Object o in area.GetChildren<Object>())
                if (o is Item || o is Npc)
                    o.Parent = null;
            foreach (Reset r in area.Resets)
                r.Clear();
        }
        private void designexactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ViewType = View.Design1;
        }
        private void designfastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ViewType = View.Design2;
        }
        private void gameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ViewType = View.Game;
        }
        internal bool Undo()
        {
            if (this.undo_[this.area_].Count > 0)
            {
                Area.Layer oldlayer = this.undo_[this.area_].Pop();
                Area.Layer currentlayer = this.area_.Children.Find(delegate(Object o) { return o.Id == oldlayer.Id; }) as Area.Layer;
                if (currentlayer != null)
                {
                    this.redo_[this.area_].Push(currentlayer.Clone() as Area.Layer);
                    if (this.RedoAvailable != null) this.RedoAvailable();
                    currentlayer.Points = new List<Point>(oldlayer.Points);
                    currentlayer.Location.Point = oldlayer.Location.Point;
                    this.Invalidate();
                }
                if (this.undo_[this.area_].Count == 0 && this.UndoUnavailable != null) this.UndoUnavailable();
                return true;
            }
            if (this.UndoUnavailable != null) this.UndoUnavailable();
            return false;
        }
        internal bool Redo()
        {
            if (this.redo_[this.area_].Count > 0)
            {
                Area.Layer oldlayer = this.redo_[this.area_].Pop();
                Area.Layer currentlayer = this.area_.Children.Find(delegate(Object o) { return o.Id == oldlayer.Id; }) as Area.Layer;
                if (currentlayer != null)
                {
                    this.AddUndo(currentlayer, false);
                    currentlayer.Points = new List<Point>(oldlayer.Points);
                    currentlayer.Location.Point = oldlayer.Location.Point;
                    this.Invalidate();
                }
                if (this.redo_[this.area_].Count == 0 && this.RedoUnavailable != null) this.RedoUnavailable();
                return true;
            }
            if (this.RedoUnavailable != null) this.RedoUnavailable();
            return false;
        }
        void AddUndo(Area.Layer l, bool clearredo)
        {
            this.undo_[this.area_].Push(l.Clone() as Area.Layer);
            if (this.UndoAvailable != null) this.UndoAvailable();
            if (clearredo)
            {
                this.redo_[this.area_].Clear();
                if (this.RedoUnavailable != null) this.RedoUnavailable();
            }
        }
    }
}
