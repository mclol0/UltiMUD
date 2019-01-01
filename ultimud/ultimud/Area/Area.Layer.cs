using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Reflection;
using LuaInterface;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Layer : Object, ICloneable
        {
            public enum LayerFlag
            {
                NoWalk,
                NoFly,
                NoAttack,
                Indoors,
                Swim,
                NoQuit,
                NoNpc,
            }
            public enum LayerScriptType
            {
                OnObjectMove,
                //OnObjectMoved,
            }

            #region Members
            List<Point> points_ = new List<Point>();
            BitArray layerFlags_ = new BitArray(16);
            int zIndex_;
            char fillChar_ = '^';
            Telnet.Color fillColor_ = Telnet.Color.White;
            char fillChar2_;
            Telnet.Color fillColor2_ = Telnet.Color.Reset;
            int fillChar2Percent_;
            bool fillChar2Randomize_;
            Dictionary<Point, bool> overlay_ = new Dictionary<Point, bool>();
            char ceilingChar_; //Deprecated.
            Telnet.Color ceilingColor_; //Deprecated.
            int height_;
            WeatherConditions weather_ = new WeatherConditions();
            Dictionary<LayerScriptType, string> layerscripts_ = new Dictionary<LayerScriptType, string>();
            //List<StatusEffect> applystatuseffects_ = new List<StatusEffect>();

            [NonSerialized]
            [Obfuscation(Exclude = false)]
            Lua lua_ = new Lua();
            #endregion

            #region Properties
            [Browsable(false)]
            public List<Point> Points
            {
                get { return this.points_; }
                set { this.points_ = value; }
            }

            [Category("Appearance")]
            [DisplayName("FillChar1")]
            [Description("The ASCI character that will be used to fill this layer on the map.")]
            [DefaultValue('^')]
            public char FillChar
            {
                get { return this.fillChar_; }
                set { this.fillChar_ = value; }
            }

            [Category("Appearance")]
            [DisplayName("FillChar1Color")]
            [Description("The color of FillChar.")]
            [DefaultValue(Telnet.Color.White)]
            public Telnet.Color FillColor
            {
                get { return this.fillColor_; }
                set { this.fillColor_ = value; }
            }

            [Category("Appearance")]
            [Description("A secondary ASCI character that will be used to fill this layer on the map.")]
            public char FillChar2
            {
                get { return this.fillChar2_; }
                set
                {
                    this.fillChar2_ = value;
                    if (value > 0)
                    {
                        this.InitOverlay();
                    }
                }
            }

            [Category("Appearance")]
            [DisplayName("FillChar2Color")]
            [Description("The color of FillChar2.")]
            [DefaultValue(Telnet.Color.Reset)]
            public Telnet.Color FillColor2
            {
                get { return this.fillColor2_; }
                set { this.fillColor2_ = value; }
            }

            [Category("Appearance")]
            [Description("The percentage of the layer to fill using FillChar2.")]
            [DefaultValue(0)]
            public int FillChar2Percent
            {
                get { return this.fillChar2Percent_; }
                set
                {
                    this.fillChar2Percent_ = value;
                    this.InitOverlay();
                }
            }

            [Category("Appearance")]
            [Description("Should the locations of FillChar2 be random everytime the map is drawn?")]
            [DefaultValue(false)]
            public bool FillChar2Randomize
            {
                get { return this.fillChar2Randomize_; }
                set { this.fillChar2Randomize_ = value; }
            }

            [TypeConverter(typeof(Damdai.TypeConverters.BitArrayConverter<LayerFlag>))]
            [Description("Various flags that effect the behavior of this layer.")]
            [Editor(typeof(FlagEditor<LayerFlag>), typeof(System.Drawing.Design.UITypeEditor))]
            public BitArray LayerFlags
            {
                get { return this.layerFlags_; }
                set { this.layerFlags_ = value; }
            }

            [Browsable(false)]
            public Rectangle BoundingBox
            {
                get
                {
                    if (this.Points.Count == 0)
                        return Rectangle.Empty;

                    Point[] temp = new Point[2] { this.Points[0], this.Points[0] };
                    for (int x = 1; x < this.Points.Count; ++x)
                    {
                        Point p = this.Points[x];
                        temp[0].X = Math.Min(p.X, temp[0].X);
                        temp[0].Y = Math.Min(p.Y, temp[0].Y);
                        temp[1].X = Math.Max(p.X, temp[1].X);
                        temp[1].Y = Math.Max(p.Y, temp[1].Y);
                    }
                    return new Rectangle(temp[0].X, temp[0].Y, temp[1].X - temp[0].X, temp[1].Y - temp[0].Y);
                }
            }

            [Browsable(false)]
            public override Location Location
            {
                get { return new Location(this.Area, this.Center); }
                set { this.Center = value.Point; }
            }

            [Description("The center point of this layer's bounding box.")]
            public Point Center
            {
                get
                {
                    if (this.Points.Count == 1)
                        return this.Points[0];

                    Rectangle boundingBox = this.BoundingBox;
                    return new Point(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
                }
                set
                {
                    Point difference = new Point(this.Center.X - value.X, this.Center.Y - value.Y);
                    for (int x = 0; x < this.Points.Count; ++x )
                        this.Points[x] = new Point(this.Points[x].X - difference.X, this.Points[x].Y - difference.Y);
                }
            }

            [Browsable(false)]
            public override int IconIndex
            {
                get { return 2; }
            }

            [DisplayName("Z-Index")]
            [Description("The overlapping order of this layer on the map. Layers with a higher z-index will appear on top of overlapping layers with a lower z-index.")]
            [DefaultValue(0)]
            public int ZIndex
            {
                get { return this.zIndex_; }
                set { this.zIndex_ = value; }
            }

            [Browsable(false)]
            new public string MapSymbol
            {
                get { return base.MapSymbol; }
                set { base.MapSymbol = value; }
            }

            [Browsable(false)]
            new public Telnet.Color MapColor
            {
                get { return base.MapColor; }
                set { base.MapColor = value; }
            }

            [Browsable(false)]
            new public Sex Gender
            {
                get { return base.Gender; }
                set { base.Gender = value; }
            }

            [Browsable(false)]
            new public Positions Position
            {
                get { return base.Position; }
                set { base.Position = value; }
            }

            [Browsable(false)]
            public Point NorthPoint
            {
                get
                {
                    Point point = Point.Empty;
                    foreach (Point p in this.Points)
                    {
                        if (point == Point.Empty || p.Y < point.Y)
                        {
                            point = p;
                        }
                    }
                    return point;
                }
            }

            [Browsable(false)]
            new public int Size
            {
                get { return base.Size; }
                set { base.Size = value; }
            }

            [Browsable(false)]
            new public int RemainingSpace
            {
                get { return base.RemainingSpace; }
            }

            Dictionary<Point, bool> Overlay
            {
                get
                {
                    if (this.overlay_ == null)
                    {
                        this.overlay_ = new Dictionary<Point, bool>();
                    }
                    return this.overlay_;
                }
            }

            [Description("The height of this layer. Plays a role in line of sight calculations.")]
            [DefaultValue(0)]
            public virtual int Height
            {
                get { return this.height_; }
                set { this.height_ = value; }
            }

            [Description("The weather conditions of this area.")]
            public WeatherConditions WeatherConditions
            {
                get { return this.weather_; }
                set { this.weather_ = value; }
            }

            [Description("This script is executed when an object moves on this layer.\r\n\r\nObjects: layer, actor\r\nReturns: nothing, 'cancel'\r\n\r\nFor help on LUA scripting, view the 'Scripting API' at www.ultimud.com.")]
            [Editor(typeof(StringEditor), typeof(System.Drawing.Design.UITypeEditor))]
            [Category("Scripts")]
            public string OnObjectMove
            {
                get
                {
                    string script;
                    if (!this.layerscripts_.TryGetValue(LayerScriptType.OnObjectMove, out script))
                        this.layerscripts_.Add(LayerScriptType.OnObjectMove, null);
                    return script;
                }
                set
                {
                    if (this.layerscripts_.ContainsKey(LayerScriptType.OnObjectMove))
                        this.layerscripts_[LayerScriptType.OnObjectMove] = value;
                    else
                        this.layerscripts_.Add(LayerScriptType.OnObjectMove, value);
                }
            }

            [Browsable(false)]
            public new bool IsSwimming
            {
                get { return base.IsSwimming; }
            }

            [Browsable(false)]
            public new List<StatusEffect> StatusEffects
            {
                get { return base.StatusEffects; }
                set { base.StatusEffects = value; }
            }
            #endregion

            public Layer()
            {
                this.ObjectFlags.Set((int)Object.ObjFlags.Indestructible, true);
                this.ObjectFlags.Set((int)Object.ObjFlags.Immovable, true);
                lock (this.Stats)
                    this.Stats.Clear();
                if (Server.Instance != null)
                    this.Id = Server.Instance.NewGuid();
            }

            [Obfuscation(Exclude = false)]
            public object Clone()
            {
                Layer layer = (Layer)this.MemberwiseClone();
                layer.Location = this.Location;
                layer.Parent = null;
                layer.points_ = new List<Point>(this.points_);
                return layer;
            }

            public bool IsPoint(int x, int y)
            {
                foreach (Point p in this.Points)
                    if (p.X == x && p.Y == y)
                        return true;
                return false;
            }
            
            public bool Contains(Point p)
            {
                return this.Contains(p.X, p.Y);
            }
            
            public virtual bool Contains(int x, int y)
            {
                if (this.Points.Count == 0)
                    return false;
                if (this.Points.Count == 1)
                    return (this.points_[0].X == x && this.points_[0].Y == y);

                int counter = 0;
                double xinters;
                Point p1 = this.Points[0];
                for (int i = 1; i <= this.Points.Count; i++)
                {
                    Point p2 = this.Points[i % this.Points.Count];
                    if (y > Math.Min(p1.Y, p2.Y))
                    {
                        if (y <= Math.Max(p1.Y, p2.Y))
                        {
                            if (x <= Math.Max(p1.X, p2.X))
                            {
                                if (p1.Y != p2.Y)
                                {
                                    xinters = (y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                    if (p1.X == p2.X || x <= xinters)
                                        counter++;
                                }
                            }
                        }
                    }
                    p1 = p2;
                }

                return (counter % 2 != 0);
            }
            
            [Obfuscation(Exclude = false)]
            public bool Contains(IEnumerable<Point> points)
            {
                foreach (Point point in points)
                    if (this.Area.GetLayer<Area.Layer>(point, true) == this && this.Contains(point))
                        return true;
                return false;
            }
            
            public override int Distance(Point p)
            {
                //A = P2Y - P1Y
                //B = P1X - P2X
                //C = P2X * P1Y - P2Y * P1X
                //DistToLine = Abs((A * PX + B * PY + C) / Sqr(A * A + B * B))

                if (this.Contains(p))
                    return 0;
                if (this.points_.Count == 1)
                    return Damdai.Math.Distance(this.points_[0], p);

                int distance = -1;

                for (int x = 0; x < this.Points.Count; ++x)
                {
                    Point p1 = this.Points[x];
                    Point p2 = this.Points[(x + 1) % this.Points.Count];
                    //int d = DistanceToLine(p, p1, p2);
                    int d = DistanceToEdge(p, p1, p2);
                    //int d = DistancePointLine(p, p1, p2);
                    if (distance == -1 || d < distance)
                    {
                        distance = d;
                        if (distance == 0)
                            return (distance > 1 ? distance : 1);
                    }
                }

                return (distance > 1 ? distance : 1);
            }
            
            [Obfuscation(Exclude = false)]
            int DistanceToLine(Point p, Point p1, Point p2)
            {
                int a = p2.Y - p1.Y;
                int b = p1.X - p2.X;
                int c = p2.X * p1.Y - p2.Y * p1.X;
                int d = (int)Math.Abs((a * p.X + b * p.Y + c) / Math.Sqrt(a * a + b * b));
                return d;
            }
            
            [Obfuscation(Exclude = false)]
            protected int DistanceToEdge(Point p, Point p1, Point p2)
            {
                double dx = p2.X - p1.X;
                double dy = p2.Y - p1.Y;

                double t = (((p.X - p1.X) * dx) + ((p.Y - p1.Y) * dy)) / (dx * dx + dy * dy);
                //System.Diagnostics.Debug.WriteLine("T: " + t);
                if (t < 0)
                {
                    dx = p.X - p1.X;
                    dy = p.Y - p1.Y;
                }
                else if (t > 1)
                {
                    dx = p.X - p2.X;
                    dy = p.Y - p2.Y;
                }
                else
                {
                    dx = (int)(p.X - (p1.X + t * dx));
                    dy = (int)(p.Y - (p1.Y + t * dy));
                }
                return (int)Math.Sqrt(dx * dx + dy * dy);
            }
            
            [Obfuscation(Exclude = false)]
            float Magnitude(Point p1, Point p2)
            {
                Point v = new Point(p2.X - p1.X, p2.Y - p1.Y);
                return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            }
            
            [Obfuscation(Exclude = false)]
            int DistancePointLine(Point p, Point p1, Point p2)
            {
                float LineMag;
                float U;
                LineMag = Magnitude(p2, p1);
                U = ((p.X - p1.X) * (p2.X - p1.X)) + ((p.Y - p1.Y) * (p2.Y - p1.Y)) / (LineMag * LineMag);

                if (U < 0 || U > 1)
                {
                    //return 100;   // closest point does not fall within the line segment
                }
                Point Intersection = new Point((int)(p1.X + U * (p2.X - p1.X)), (int)(p1.Y + U * (p2.Y - p1.Y)));
                return (int)Magnitude(p, Intersection);
            }
            
            [Obfuscation(Exclude = false)]
            public override bool Intersects(Rectangle rect)
            {
                return this.BoundingBox.IntersectsWith(rect);
            }
            
            [Obfuscation(Exclude = false)]
            public override void Draw(Map map)
            {
                if (this.ObjectFlags.Get((int)Object.ObjFlags.Invisible) || this.Points.Count == 0)
                    return;

                Random rand = null;
                if (this.FillChar2 > 0 && this.FillChar2Randomize)
                    rand = new Random();

                Rectangle boundingBox = this.BoundingBox;

                for (int y = boundingBox.Y; y <= boundingBox.Y + boundingBox.Height; ++y)
                {
                    int Y = y - map.Offset.Y;
                    for (int x = boundingBox.X; x <= boundingBox.X + boundingBox.Width; ++x)
                    {
                        int X = x - map.Offset.X;
                        if (X < 0 || X > (map.Size - 1) || Y < 0 || Y > (map.Size - 1))
                            continue;

                        if (this.Contains(x, y))
                        {
                            if (this.FillChar2 > 0 && ((rand != null && rand.Next(100) <= this.FillChar2Percent) || this.Overlay.ContainsKey(new Point(x, y))))
                            {
                                map.Grid[X, Y] = this.FillChar2 + " ";
                                map.Color[X, Y] = this.FillColor2;
                            }
                            else
                            {
                                map.Grid[X, Y] = this.FillChar + " ";
                                map.Color[X, Y] = this.FillColor;
                            }
                        }
                    }
                }
            }
            
            [Obfuscation(Exclude = false)]
            internal override void Draw(Graphics g, AreaCanvas canvas, AreaCanvas.View view)
            {
                Color c = Telnet.ColorToSystemColor(this.FillColor);

                if (!this.ObjectFlags.Get((int)Object.ObjFlags.Invisible) && this.Points.Count > 0)
                {
                    switch (view)
                    {
                        case AreaCanvas.View.Design1:
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb((this == canvas.SelectedObject ? 75 : 50), c)))
                                this.Fill1(g, canvas, brush);
                            break;
                        case AreaCanvas.View.Design2:
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb((this == canvas.SelectedObject ? 75 : 50), c)))
                                this.Fill3(g, canvas, brush);
                            break;
                        case AreaCanvas.View.Game:
                            using (SolidBrush brush = new SolidBrush(c))
                                this.Fill2(g, canvas, brush);
                            break;
                    }
                }

                if (view != AreaCanvas.View.Game || this == canvas.SelectedObject)
                    this.DrawSelection(g, canvas, c);
                this.DrawPoints(g, canvas);
            }
            
            [Obfuscation(Exclude = false)]
            virtual internal void Fill1(Graphics g, AreaCanvas canvas, SolidBrush brush)
            {
                Rectangle boundingBox = this.BoundingBox;
                Rectangle viewport = canvas.ViewPort;
                for (int x = boundingBox.X; x <= boundingBox.Width + boundingBox.X; ++x)
                {
                    int xpos = (x - canvas.Offset.X) * canvas.Zoom;
                    for (int y = boundingBox.Y; y <= boundingBox.Height + boundingBox.Y; ++y)
                    {
                        if (viewport.Contains(x, y) && this.Contains(x, y))
                        {
                            int ypos = (y - canvas.Offset.Y) * canvas.Zoom;
                            g.FillRectangle(brush, xpos, ypos, canvas.Zoom, canvas.Zoom);
                        }
                    }
                }
            }
            
            [Obfuscation(Exclude = false)]
            virtual internal void Fill2(Graphics g, AreaCanvas canvas, SolidBrush brush)
            {
                //SolidBrush bgBrush = new SolidBrush(Telnet.BgColorToSystemColor(this.BgColor));
                Font font = new Font("fixedsys", Math.Max((int)(canvas.Zoom / 1.7), 1), FontStyle.Bold);
                Rectangle boundingBox = this.BoundingBox;
                SolidBrush bgBrush = new SolidBrush(Color.Black);
                SolidBrush fillChar2Brush = null;
                Random rand = null;
                if (this.FillChar2 > 0)
                {
                    fillChar2Brush = new SolidBrush(Telnet.ColorToSystemColor(this.FillColor2));
                    if (this.FillChar2Randomize)
                        rand = new Random();
                }
                Rectangle viewport = canvas.ViewPort;
                for (int x = boundingBox.X; x <= boundingBox.Width + boundingBox.X; ++x)
                {
                    int xpos = (x - canvas.Offset.X) * canvas.Zoom;
                    for (int y = boundingBox.Y; y <= boundingBox.Height + boundingBox.Y; ++y)
                    {
                        if (viewport.Contains(x, y) && this.Contains(x, y))
                        {
                            int ypos = (y - canvas.Offset.Y) * canvas.Zoom;
                            g.FillRectangle(bgBrush, xpos, ypos, canvas.Zoom, canvas.Zoom);
                            SolidBrush b = null;
                            char c;
                            if (this.FillChar2 > 0 && ((rand != null && rand.Next(100) <= this.FillChar2Percent) || this.Overlay.ContainsKey(new Point(x, y))))
                            {
                                b = fillChar2Brush;
                                c = this.FillChar2;
                            }
                            else
                            {
                                b = brush;
                                c = this.FillChar;
                            }

                            g.DrawString(c.ToString(), font, b, xpos, ypos);
                        }
                    }
                }

                if (bgBrush != null) bgBrush.Dispose();
                if (fillChar2Brush != null) fillChar2Brush.Dispose();
                if (font != null) font.Dispose();
            }
            
            [Obfuscation(Exclude = false)]
            virtual internal void Fill3(Graphics g, AreaCanvas canvas, SolidBrush brush)
            {
                PointF[] points = new PointF[this.points_.Count];
                for (int x = 0; x < this.points_.Count; ++x)
                {
                    Point p = this.points_[x];
                    points[x] = new PointF((p.X - canvas.Offset.X) * canvas.Zoom + canvas.Zoom,
                        (p.Y - canvas.Offset.Y) * canvas.Zoom + canvas.Zoom);
                }
                g.FillPolygon(brush, points);
            }
            
            [Obfuscation(Exclude = false)]
            virtual internal void DrawPoints(Graphics g, AreaCanvas canvas)
            {
                Pen pen1 = new Pen(Color.Aqua);
                Pen pen2 = new Pen(Color.FromArgb(100, Color.White));
                Rectangle viewport = canvas.ViewPort;
                float n = canvas.Zoom / 2;
                if (this == canvas.SelectedObject)
                {
                    foreach (Point p in this.points_)
                    {
                        if (viewport.Contains(p))
                        {
                            g.DrawEllipse(
                                pen1,
                                (p.X - canvas.Offset.X) * canvas.Zoom + (n + n / 2),
                                (p.Y - canvas.Offset.Y) * canvas.Zoom + (n + n / 2),
                                canvas.Zoom / 2,
                                canvas.Zoom / 2);
                            g.DrawRectangle(
                                pen2,
                                (p.X - canvas.Offset.X) * canvas.Zoom,
                                (p.Y - canvas.Offset.Y) * canvas.Zoom,
                                canvas.Zoom,
                                canvas.Zoom);
                        }
                    }
                }
                pen1.Dispose();
                pen2.Dispose();
            }
            
            [Obfuscation(Exclude = false)]
            virtual internal void DrawSelection(Graphics g, AreaCanvas canvas, Color color)
            {
                PointF[] points = new PointF[this.Points.Count];
                int count = 0;
                foreach (Point p in this.Points)
                {
                    points[count++] = new PointF(
                        (p.X - canvas.Offset.X) * canvas.Zoom + canvas.Zoom,
                        (p.Y - canvas.Offset.Y) * canvas.Zoom + canvas.Zoom);
                }

                if (points.Length >= 2)
                {
                    using(Pen pen = new Pen(color, 2))
                        g.DrawPolygon(pen, points);
                    if (this == canvas.SelectedObject
                        && canvas.SelectedPoint != Point.Empty
                        && this.Points.Contains(canvas.SelectedPoint))
                    {
                        float n = canvas.Zoom / 2;
                        using (SolidBrush brush = new SolidBrush(Color.Red))
                            g.FillEllipse(
                                //new SolidBrush(Color.FromArgb(50, Color.White)),
                                brush,
                                (canvas.SelectedPoint.X - canvas.Offset.X) * canvas.Zoom + (n + n / 2),
                                (canvas.SelectedPoint.Y - canvas.Offset.Y) * canvas.Zoom + (n + n / 2),
                                n, n);
                    }
                }
            }
            
            [Obfuscation(Exclude = false)]
            int Area2(Point a, Point b, Point c)
            {
                return
                    a.X * b.Y - a.Y * b.X +
                    a.Y * c.X - a.X * c.Y +
                    b.X * c.Y - c.X * b.Y;
            }
            
            [Obfuscation(Exclude = false)]
            public override string ToString()
            {
                if (string.IsNullOrEmpty(this.Name))
                    return base.ToString();
                else
                    return this.Name;
            }
            
            [Obfuscation(Exclude = false)]
            void InitOverlay()
            {
                this.Overlay.Clear();
                if (this.FillChar2 > 0)
                {
                    Rectangle rect = this.BoundingBox;
                    for (int x = rect.X; x <= rect.X + rect.Width; ++x)
                        for (int y = rect.Y; y <= rect.Y + rect.Height; ++y)
                            if (this.Contains(x, y) && Server.RandomNumber.Next(100) <= this.FillChar2Percent)
                                this.Overlay.Add(new Point(x, y), true);
                }
            }
            
            public Point GetNearestPoint(Point p)
            {
                Point nearestPoint = Point.Empty;
                int distance = 0;
                List<Point> points = null;
                lock (this.points_)
                    points = new List<Point>(this.points_);
                foreach (Point point in points)
                {
                    int x = Damdai.Math.Distance(point, p);
                    if (nearestPoint == Point.Empty || x < distance)
                    {
                        distance = x;
                        nearestPoint = point;
                    }
                }
                return nearestPoint;
            }
            
            public override void Send(string str)
            {
                base.Send(str);
            }
            
            [Obfuscation(Exclude = false)]
            public override void Update()
            {
                base.Update();
                if (this.WeatherConditions != null && Server.RandomNumber.Next(300) == 1)
                    this.WeatherConditions.Update(this);
            }
            
            [Obfuscation(Exclude = false)]
            public object[] DoScript(LayerScriptType type, Object actor)
            {
                string script;
                if (!this.layerscripts_.TryGetValue(type, out script) || string.IsNullOrEmpty(script))
                    return null;

                try
                {
                    lock (this.lua_)
                    {
                        this.lua_["actor"] = actor;
                        this.lua_["layer"] = this;
                        return this.lua_.DoString(script);
                    }
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message, "ERROR: Layer: " + type);
                    Debug.WriteLine(script, "Script");
                    Debug.WriteLine(e.ToString(), "StackTrace");
#if DEBUG
                    actor.Send(e.Message);
#endif
                }
                return null;
            }

            [OnDeserialized]
            [Obfuscation(Exclude = false)]
            internal new void OnDeserialized(StreamingContext context)
            {
                base.OnDeserialized(context);
                this.lua_ = new Lua();
                if (this.weather_ == null)
                    this.weather_ = new WeatherConditions();
                if (this.layerscripts_ == null)
                    this.layerscripts_ = new Dictionary<LayerScriptType, string>();
            }

            [Obfuscation(Exclude = false)]
            public class ZIndexComparer<T> : IComparer<T> where T : Layer
            {
                public int Compare(T x, T y)
                {
                    if (x.ZIndex < y.ZIndex)
                        return 1;
                    else if (x.ZIndex == y.ZIndex)
                        return 0;
                    else
                        return -1;
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public T GetLayer<T>(Point p, bool invis, params T[] ignore) where T : Layer
        {
            List<T> layers = this.GetChildren<T>(p);
            if (layers.Count > 0)
            {
                layers.Sort(new Layer.ZIndexComparer<T>());
                List<T> ignoreList = null;
                if (ignore != null)
                    ignoreList = new List<T>(ignore);
                foreach (T l in layers)
                {
                    if ((!invis && l.ObjectFlags.Get((int)Object.ObjFlags.Invisible)) || ignoreList != null && ignoreList.Contains(l))
                        continue;
                    else
                        return l;
                }
            }
            return null;
        }

        [Obfuscation(Exclude = false)]
        public List<T> GetLayers<T>(Point p, bool invis, params T[] ignore) where T : Layer
        {
            List<T> list = this.GetChildren<T>(p);
            if (!invis)
            {
                List<T> temp = list;
                list = new List<T>(temp.Count);
                foreach (T t in list)
                    if (!t.ObjectFlags.Get((int)Object.ObjFlags.Invisible))
                        list.Add(t);
            }
            return list;
        }
    }
}
