using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Damdai
{
    public class Math
    {
        public static float Slope(Point p1, Point p2)
        {
            return (p2.Y - p1.Y) / (p2.X = p1.X);
        }
        public static List<Point> Line(Point p1, Point p2)
        {
            return Line(p1.X, p2.X, p1.Y, p2.Y);
        }
        public static List<Point> Line(int x0, int x1, int y0, int y1)
        {
            bool steep = System.Math.Abs(y1 - y0) > System.Math.Abs(x1 - x0);
            if (steep)
            {
                Damdai.Util.Swap(ref x0, ref y0);
                Damdai.Util.Swap(ref x1, ref y1);
            }

            if (x0 > x1)
            {
                Damdai.Util.Swap(ref x0, ref x1);
                Damdai.Util.Swap(ref y0, ref y1);
            }

            int deltax = x1 - x0;
            int deltay = System.Math.Abs(y1 - y0);
            int error = 0;
            int ystep;
            int y = y0;
            if (y0 < y1)
                ystep = 1;
            else
                ystep = -1;

            List<Point> line = new List<Point>();
            for (int x = x0; x <= x1; ++x)
            {
                if (steep)
                    line.Add(new Point(y, x));
                else
                    line.Add(new Point(x, y));
                error = error + deltay;
                if (2 * error >= deltax)
                {
                    y += ystep;
                    error -= deltax;
                }
            }

            line.RemoveAt(0);
            if (line.Count > 0)
                line.RemoveAt(line.Count - 1);
            return line;
        }
        public static int Distance(Point p1, Point p2)
        {
            return System.Math.Abs(p1.X - p2.X) + System.Math.Abs(p1.Y - p2.Y);
        }
        public static bool PointInPolygon(List<Point> points, int x, int y)
        {
            if (points.Count == 0)
                return false;
            if (points.Count == 1)
                return (points[0].X == x && points[0].Y == y);

            int counter = 0;
            double xinters;
            Point p1 = points[0];
            for (int i = 1; i <= points.Count; i++)
            {
                Point p2 = points[i % points.Count];
                if (y > System.Math.Min(p1.Y, p2.Y))
                {
                    if (y <= System.Math.Max(p1.Y, p2.Y))
                    {
                        if (x <= System.Math.Max(p1.X, p2.X))
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
        public static bool PointInPath(List<Point> points, int x, int y, float thickness)
        {
            if (points.Count == 0)
                return false;
            if (points.Count == 1)
                return (points[0].X == x && points[0].Y == y);

            for (int i = 0; i < points.Count - 1; ++i)
            {
                Point a = points[i];
                Point b = points[i + 1];
                PointF v = new PointF(-(b.Y - a.Y), b.X - a.X);
                float l = (float)System.Math.Sqrt(v.X * v.X + v.Y * v.Y);
                v.X /= l;
                v.Y /= l;
                float c = -(v.X * a.X + v.Y * a.Y);
                float distance = System.Math.Abs(v.X * x + v.Y * y + c);
                Point[] boundingBox = new Point[2];
                boundingBox[0].X = System.Math.Min(a.X, b.X);
                boundingBox[0].Y = System.Math.Min(a.Y, b.Y);
                boundingBox[1].X = System.Math.Max(a.X, b.X);
                boundingBox[1].Y = System.Math.Max(a.Y, b.Y);
                if (distance > thickness || x < boundingBox[0].X || x > boundingBox[1].X || y < boundingBox[0].Y || y > boundingBox[1].Y)
                    continue;
                return true;
            }
            return false;
        }
        public static int DistanceFromPolygon(Point p, List<Point> polygon)
        {
            if (PointInPolygon(polygon, p.X, p.Y))
                return 0;
            if (polygon.Count == 1)
                return Distance(polygon[0], p);

            int distance = -1;

            for (int x = 0; x < polygon.Count; ++x)
            {
                Point p1 = polygon[x];
                Point p2 = polygon[(x + 1) % polygon.Count];
                //int d = DistanceToLine(p, p1, p2);
                int d = DistanceToLine(p, p1, p2);
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
        public static int DistanceFromPath(Point p, List<Point> path, float thickness)
        {
            if (PointInPath(path, p.X, p.Y, thickness))
                return 0;
            if (path.Count == 1)
                return Distance(path[0], p);

            int distance = -1;

            for (int x = 0; x < path.Count - 1; ++x)
            {
                Point p1 = path[x];
                Point p2 = path[x + 1];
                int d = DistanceToLine(p, p1, p2);
                if (distance == -1 || d < distance)
                {
                    distance = d;
                    if (distance == 0)
                        return (distance > 1 ? distance : 1);
                }
            }

            return (distance > 1 ? distance : 1);
        }
        public static int DistanceToLine(Point p, Point p1, Point p2)
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
            return (int)System.Math.Sqrt(dx * dx + dy * dy);
        }
        public static Rectangle BoundingBox(List<Point> points)
        {
            if (points.Count == 0)
                return Rectangle.Empty;

            Point[] temp = new Point[2] { points[0], points[0] };
            for (int x = 1; x < points.Count; ++x)
            {
                Point p = points[x];
                temp[0].X = System.Math.Min(p.X, temp[0].X);
                temp[0].Y = System.Math.Min(p.Y, temp[0].Y);
                temp[1].X = System.Math.Max(p.X, temp[1].X);
                temp[1].Y = System.Math.Max(p.Y, temp[1].Y);
            }
            return new Rectangle(temp[0].X, temp[0].Y, temp[1].X - temp[0].X, temp[1].Y - temp[0].Y);
        }
        public static Point Center(List<Point> polygon)
        {
            if (polygon.Count == 1)
                return polygon[0];

            Rectangle bb = BoundingBox(polygon);
            return new Point(bb.X + bb.Width / 2, bb.Y + bb.Height / 2);
        }
    }
}
