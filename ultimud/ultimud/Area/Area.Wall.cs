using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Wall : Path
        {
            public Wall()
            {
                this.LayerFlags.Set((int)LayerFlag.NoWalk, true);
                this.LayerFlags.Set((int)LayerFlag.NoFly, true);
                this.FillChar = '#';
                this.Height = 1;
            }

            public override int IconIndex
            {
                get { return 7; }
            }
            
            override public bool Contains(int x, int y)
            {
                if (this.Points.Count == 0)
                    return false;
                if(this.Points.Count == 1)
                    return (this.Points[0].X == x && this.Points[0].Y == y);

                for (int i = 0; i < this.Points.Count - 1; ++i)
                {
                    Point a = this.Points[i];
                    Point b = this.Points[i + 1];
                    /*
                    int distance = System.Math.Abs((a.Y-b.Y)*(x-a.X) - (a.X-b.X)*(y-a.Y));
                    */
                    PointF v = new PointF(-(b.Y - a.Y), b.X - a.X);
                    float l = (float)System.Math.Sqrt(v.X * v.X + v.Y * v.Y);
                    v.X /= l;
                    v.Y /= l;
                    float c = -(v.X * a.X + v.Y * a.Y);
                    float distance = System.Math.Abs(v.X * x + v.Y * y + c);
                    Point[] boundingBox = new Point[2];
                    boundingBox[0].X = Math.Min(a.X, b.X);
                    boundingBox[0].Y = Math.Min(a.Y, b.Y);
                    boundingBox[1].X = Math.Max(a.X, b.X);
                    boundingBox[1].Y = Math.Max(a.Y, b.Y);
                    if (distance > .75 || x < boundingBox[0].X || x > boundingBox[1].X || y < boundingBox[0].Y || y > boundingBox[1].Y)
                    {
                        continue;
                    }
                    return true;
                }
                return false;
            }
        }
    }
}
