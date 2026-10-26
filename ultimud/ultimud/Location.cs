using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;

namespace Ultimud
{
    [Serializable]
    [Editor(typeof(LocationSelector.Editor), typeof(UITypeEditor))]
    [Obfuscation(Exclude = true)]
    public class Location
    {
        Guid areaId_;
        Point point_;

        public Area Area
        {
            get { return Server.Instance.Find<Area>(this.areaId_); }
            set { this.areaId_ = (value != null ? value.Id : Guid.Empty); }
        }
        public Point Point
        {
            get { return this.point_; }
            set { this.point_ = value; }
        }
        public bool IsValid
        {
            get { return (this.Area != null); }
        }

        public Location()
        {
        }
        public Location(Area area, Point point)
        {
            this.Area = area;
            this.Point = point;
        }
        public Location(Area area, int x, int y)
            : this(area, new Point(x, y))
        {
        }

        public override string ToString()
        {
            Area area = this.Area;
            if (area != null)
                return area.Name + " " + this.Point.ToString();
            else
                return null;
        }
        public override bool Equals(object obj)
        {
            Location other = obj as Location;
            if (other != null)
                return (this.areaId_ == other.areaId_ && this.point_ == other.point_);
            else
                return false;
        }
        
        [Obfuscation(Exclude = false)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
