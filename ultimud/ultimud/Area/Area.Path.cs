using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Path : Layer
        {
            public Path()
            {
                this.FillChar = '.';
            }

            [Obfuscation(Exclude = false)]
            public override int IconIndex
            {
                get { return 3; }
            }

            override public bool Contains(int x, int y)
            {
                if (this.Points.Count == 0)
                    return false;
                if (this.Points.Count == 1)
                    return (this.Points[0].X == x && this.Points[0].Y == y);

                for (int i = 0; i < this.Points.Count - 1; ++i)
                {
                    Point a = this.Points[i];
                    Point b = this.Points[i + 1];
                    /*
                    int distance = System.Math.Abs((a.Y-b.Y)*(x-a.X) - (a.X-b.X)*(y-a.Y));
                    */
                    PointF v = new PointF(-(b.Y - a.Y), b.X - a.X);
                    float l = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
                    v.X /= l;
                    v.Y /= l;
                    float c = -(v.X * a.X + v.Y * a.Y);
                    float distance = Math.Abs(v.X * x + v.Y * y + c);
                    Point[] boundingBox = new Point[2];
                    boundingBox[0].X = Math.Min(a.X, b.X);
                    boundingBox[0].Y = Math.Min(a.Y, b.Y);
                    boundingBox[1].X = Math.Max(a.X, b.X);
                    boundingBox[1].Y = Math.Max(a.Y, b.Y);
                    if (distance > .5 || x < boundingBox[0].X || x > boundingBox[1].X || y < boundingBox[0].Y || y > boundingBox[1].Y)
                        continue;
                    return true;
                }
                return false;
            }
            
            [Obfuscation(Exclude = false)]
            override internal void DrawPoints(Graphics g, AreaCanvas canvas)
            {
                using (Pen pen = new Pen(Color.Aqua))
                {
                    Rectangle viewport = canvas.ViewPort;
                    if (this == canvas.SelectedObject)
                    {
                        foreach (Point p in this.Points)
                        {
                            if (viewport.Contains(p))
                            {
                                g.DrawRectangle(
                                    pen,
                                    (p.X - canvas.Offset.X) * canvas.Zoom,
                                    (p.Y - canvas.Offset.Y) * canvas.Zoom,
                                    canvas.Zoom,
                                    canvas.Zoom);
                            }
                        }
                    }
                }
            }
            
            [Obfuscation(Exclude = false)]
            override internal void DrawSelection(System.Drawing.Graphics g, AreaCanvas canvas, System.Drawing.Color color)
            {
                Point[] points = new Point[this.Points.Count];
                int count = 0;
                foreach (Point p in this.Points)
                {
                    points[count++] = new Point(
                        (p.X - canvas.Offset.X) * canvas.Zoom + 5,
                        (p.Y - canvas.Offset.Y) * canvas.Zoom + 5);
                }

                if (points.Length >= 2)
                {
                    using (Pen pen = new Pen(color, 4))
                        g.DrawLines(pen, points);
                    if (this == canvas.SelectedObject
                        && canvas.SelectedPoint != Point.Empty
                        && this.Points.Contains(canvas.SelectedPoint))
                    {
                        using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.White)))
                            g.FillRectangle(
                                brush,
                                (canvas.SelectedPoint.X - canvas.Offset.X) * canvas.Zoom,
                                (canvas.SelectedPoint.Y - canvas.Offset.Y) * canvas.Zoom,
                                canvas.Zoom,
                                canvas.Zoom);
                        using (Pen pen = new Pen(Color.White))
                            g.DrawRectangle(
                                pen,
                                (canvas.SelectedPoint.X - canvas.Offset.X) * canvas.Zoom,
                                (canvas.SelectedPoint.Y - canvas.Offset.Y) * canvas.Zoom,
                                canvas.Zoom,
                                canvas.Zoom);
                    }
                }
            }
            
            public override int Distance(Point p)
            {
                if (this.Contains(p))
                    return 0;
                if (this.Points.Count == 1)
                    return Damdai.Math.Distance(this.Points[0], p);

                int distance = -1;

                for (int x = 0; x < this.Points.Count - 1; ++x)
                {
                    Point p1 = this.Points[x];
                    Point p2 = this.Points[x + 1];
                    int d = DistanceToEdge(p, p1, p2);
                    if (distance == -1 || d < distance)
                    {
                        distance = d;
                        if (distance == 0)
                            return (distance > 1 ? distance : 1);
                    }
                }

                return (distance > 1 ? distance : 1);
            }
            
            public Point[] GetNearestPoints(Point p)
            {
                if (this.Points.Count == 0)
                    return new Point[] { this.Points[0], this.Points[0] };

                Point[] points = new Point[2];

                for (int x = 0; x < this.Points.Count-1; ++x)
                {
                    Point p1 = this.Points[x];
                    Point p2 = this.Points[x + 1];

                    int val = (p1.Y - p2.Y) * (p.X - p1.X) - (p1.X - p2.X) * (p.Y - p1.Y);
                    //System.Diagnostics.Debug.WriteLine(val);
                    if (System.Math.Abs(val) <= 6)
                    {
                        val = (p2.X - p1.X) * (p.X - p1.X) + (p2.Y - p1.Y) * (p.Y - p1.X);
                        int val2 = (p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.X);
                        //System.Diagnostics.Debug.WriteLine(val + " " + val2);
                        if (-55 <= val && val <= val2)
                        {
                            points[0] = (p1 != p ? p1 : x - 1 >= 0 ? this.Points[x - 1] : Point.Empty);
                            points[1] = (p2 != p ? p2 : this.Points.Count - 1 >= x + 2 ? this.Points[x + 2] : Point.Empty);
                            //System.Diagnostics.Debug.WriteLine(points[0] + " " + points[1]);
                            return points;
                        }
                    }
                }

                points[0] = this.Points[0];
                points[1] = this.Points[this.Points.Count-1];
                return points;
            }
                        
            [Obfuscation(Exclude = false)]
            public bool GetNextPoint(Point start, Queue<Point> ignore, out Point point)
            {
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    Point p = Area.GetNextPoint(start, dir);
                    if (!ignore.Contains(p) && this.Contains(p))
                    {
                        point = p;
                        return true;
                    }
                }

                point = Point.Empty;
                return false;
            }
            
            [Obfuscation(Exclude = false)]
            override internal void Fill3(Graphics g, AreaCanvas canvas, SolidBrush brush)
            {
                return;
            }
        }
    }
}
