using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class Vehicle : Object
    {
        int speed_ = 3;
        int capacity_ = 10;
        Area.Path path_;

        #region Properties

        [Description("The speed of this vehicle.")]
        [DefaultValue(3)]
        public int Speed
        {
            get { return this.speed_; }
            set { this.speed_ = value; }
        }

        [Description("The amount of people this vehicle can hold.")]
        [DefaultValue(10)]
        public int Capacity
        {
            get { return this.capacity_; }
            set { this.capacity_ = value; }
        }

        [Description("The path that this vehicle is bound.")]
        public Area.Path Path
        {
            get { return this.path_; }
            set { this.path_ = value; }
        }

        #endregion

        public Vehicle()
        {
            this.ObjectFlags.Set((int)ObjFlags.Container, true);
            this.ObjectFlags.Set((int)ObjFlags.Vehicle, true);
        }

        public Vehicle(Area.Path path)
            : this()
        {
            this.path_ = path;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            base.Update();
            if (this.path_ != null && this.CurrentAction == null && this.Area != null)
            {
                this.EnqueueAction(new FollowAction(this, this.path_, Area.Direction.North, this.speed_));
            }
        }
    }
}
