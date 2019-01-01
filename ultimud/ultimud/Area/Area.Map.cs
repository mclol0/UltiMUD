using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;

namespace Ultimud
{
    public partial class Area
    {
        [Obfuscation(Exclude = true)]
        public class Map
        {
            [Obfuscation(Exclude = false)] string[,] grid_ = new string[Server.Instance.Settings.MapSize, Server.Instance.Settings.MapSize];
            [Obfuscation(Exclude = false)] Telnet.Color[,] color_ = new Telnet.Color[Server.Instance.Settings.MapSize, Server.Instance.Settings.MapSize];
            [Obfuscation(Exclude = false)] Point offset_;
            [Obfuscation(Exclude = false)] Area area_;
            [Obfuscation(Exclude = false)] Object pov_;

            #region Properties
            public int Size
            {
                get { return Server.Instance.Settings.MapSize; }
            }
            public string[,] Grid
            {
                get { return this.grid_; }
                set { this.grid_ = value; }
            }
            public Telnet.Color[,] Color
            {
                get { return this.color_; }
                set { this.color_ = value; }
            }
            public Rectangle ViewPort
            {
                get { return new Rectangle(this.Offset.X, this.Offset.Y, this.Size, this.Size); }
            }
            public Point Offset
            {
                get { return this.offset_; }
            }
            public Object Pov
            {
                get { return this.pov_; }
            }
            #endregion

            public Map(Object o)
            {
                this.pov_ = o;
                this.offset_ = new Point(o.Location.Point.X - (this.Size / 2), o.Location.Point.Y - (this.Size / 2));
                this.area_ = o.Area;

                this.Init();
                this.DrawObjects();
                if (this.area_.LineOfSight)
                    this.ApplyLineOfSight();
            }

            [Obfuscation(Exclude = false)]
            void Init()
            {
                Random rand = null;
                if (this.area_.FillChar2 > 0)
                    rand = new Random();

                for (int x = 0; x < this.Size; ++x)
                {
                    for (int y = 0; y < this.Size; ++y)
                    {
                        if (rand != null && rand.Next(100) <= this.area_.FillChar2Percent)
                        {
                            this.Grid[x, y] = this.area_.FillChar2 + " ";
                            this.Color[x, y] = this.area_.FillColor2;
                        }
                        else
                        {
                            this.Grid[x, y] = this.area_.FillChar + " ";
                            this.Color[x, y] = this.area_.FillColor;
                        }
                        //this.BgColor[x, y] = Telnet.BgColor.BgDefault;
                    }
                }
            }

            [Obfuscation(Exclude = false)]
            void DrawObjects()
            {
                SortedDictionary<int, List<Object>> objects = new SortedDictionary<int, List<Object>>();
                List<Object> children = new List<Object>(this.area_.Children);
                foreach (Object child in children)
                {
                    if (this.Contains(child) && (!this.area_.LineOfSight || this.Pov.CanSee(child, this.area_.FogOfWar)))
                    {
                        int zindex;
                        if (child is Layer)
                        {
                            Area.Layer layer = child as Layer;
                            zindex = layer.ZIndex;
                        }
                        else if (child is Shop || child is Area)
                            zindex = int.MaxValue;
                        else if (child is Player)
                            zindex = int.MaxValue - 1;
                        else if (child is Character)
                            zindex = int.MaxValue - 2;
                        else if (child is Item)
                            zindex = int.MaxValue - 3;
                        else
                            zindex = int.MaxValue - 4;

                        if (!objects.ContainsKey(zindex))
                            objects.Add(zindex, new List<Object>());
                        objects[zindex].Add(child);
                    }
                }

                foreach (KeyValuePair<int, List<Object>> pair in objects)
                    foreach (Object obj in pair.Value)
                        this.Draw(obj);
            }
            
            [Obfuscation(Exclude = false)]
            void ApplyLineOfSight()
            {
                for (int x = this.Offset.X; x < this.Offset.X + this.Size; ++x)
                {
                    int X = x - this.Offset.X;
                    for (int y = this.Offset.Y; y < this.Offset.Y + this.Size; ++y)
                    {
                        int Y = y - this.Offset.Y;
                        if (!this.Pov.CanSee(new Point(x, y)))
                        {
                            if (this.area_.FogOfWar)
                                this.Color[X, Y] = Telnet.Color.Gray;
                            else
                                this.Grid[X, Y] = null;
                        }
                    }
                }
            }
            
            public override string ToString()
            {
                string str = Telnet.GetColorCode(this.area_.FillColor);
                Telnet.Color color = Telnet.Color.Reset;
                //Telnet.BgColor bgColor = Telnet.BgColor.BgDefault;

                for (int y = 0; y < this.Size; ++y)
                {
                    for (int x = 0; x < this.Size; ++x)
                    {
                        if (x == (this.Size / 2) && y == (this.Size / 2))
                        {
                            str += Server.Instance.Settings.GetColorCode(Settings.Text.MapColorSelf) + this.pov_.Name.Substring(0, 2).ToUpper();
                            color = Telnet.Color.BrightCyan;
                            //bgColor = Telnet.BgColor.BgDefault;
                        }
                        else
                        {
                            if (this.Grid[x, y] == null)
                                str += "  ";

                            //if (this.BgColor[x, y] != bgColor)
                            //{
                            //    bgColor = this.BgColor[x, y];
                            //    str += Telnet.GetColorCode(bgColor;)
                            //}

                            if (this.Color[x, y] != color)
                            {
                                color = this.Color[x, y];
                                str += Telnet.GetColorCode(color);
                            }

                            str += this.Grid[x, y];
                        }
                    }
                    str += "\r\n";
                }
                return str.Trim('\r', '\n') + Telnet.ColorCode.Reset;
            }
            
            public bool Contains(Object o)
            {
                return o.Intersects(this.ViewPort);
            }
            
            [Obfuscation(Exclude = false)]
            void Draw(Object o)
            {
                o.Draw(this);
            }
        }
    }
}
