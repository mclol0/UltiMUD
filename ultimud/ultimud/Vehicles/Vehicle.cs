//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.ComponentModel;

//namespace Ultimud
//{
//    public class Vehicle : Object
//    {
//        int speed_;
//        int capacity_;
//        Area.Path path_;

//        #region Properties

//        [Description("The speed of this vehicle.")]
//        public int Speed
//        {
//            get { return this.speed_; }
//            set { this.speed_ = value; }
//        }

//        [Description("The amount of people this vehicle can hold.")]
//        public int Capacity
//        {
//            get { return this.capacity_; }
//            set { this.capacity_ = value; }
//        }

//        [Description("The path that this vehicle is bound.")]
//        public Area.Path Path
//        {
//            get { return this.path_; }
//        }

//        #endregion

//        public Vehicle(Area.Path path)
//        {
//            this.ObjectFlags.Set((int)ObjFlags.Container, true);
//            this.path_ = path;
//        }
//    }
//}
