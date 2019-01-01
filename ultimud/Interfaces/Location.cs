using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Drawing;

namespace Ultimud
{
    [Serializable]
    public struct Location
    {
        #region fields
        Guid areaid_;
        Point point_;
        #endregion

        #region properties
        public Guid AreaId
        {
            get { return this.areaid_; }
            set { this.areaid_ = value; }
        }
        public IArea Area
        {
            get { return Globals.Server.WorldManager.GetObject(this.areaid_) as IArea; }
            set { this.areaid_ = value.Id; }
        }
        public Point Point
        {
            get { return this.point_; }
            set { this.point_ = value; }
        }
        #endregion

        #region constructors
        public Location(Guid areaid, Point point)
        {
            this.areaid_ = areaid;
            this.point_ = point;
        }
        public Location(Guid areaid, int x, int y)
            : this(areaid, new Point(x, y))
        {
        }
        public Location(IArea area, Point p) : this(area == null ? Guid.Empty : area.Id, p) { }
        public Location(IArea area, int x, int y) : this(area == null ? Guid.Empty : area.Id, x, y) { }
        public Location(IArea area) : this(area, area == null ? Point.Empty : area.Start) { }
        #endregion

        #region methods
        public override bool Equals(object obj)
        {
            Location other = (Location)obj;
            return (this.areaid_ == other.areaid_ && this.Point == other.Point);
        }
        public override string ToString()
        {
            IArea area = this.Area;
            if (area != null)
                return area.Name + " " + this.Point.ToString();
            else
                return null;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
