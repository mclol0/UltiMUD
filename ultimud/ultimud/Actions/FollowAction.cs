using System;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class FollowAction : Action
    {
        Object following_;
        Area.Direction startingDirection_;
        Queue<Point> lastPoints_ = new Queue<Point>();
        int speed_;

        #region Properties
        public Object Following
        {
            get { return this.following_; }
        }
        #endregion

        public FollowAction(Object follower, Object following, Area.Direction startingDirection, int speed)
            : base(follower)
        {
            this.following_ = following;
            this.startingDirection_ = startingDirection;
            this.speed_ = speed;
        }

        [Obfuscation(Exclude = false)]
        public override void Start()
        {
            this.object_.Position = Object.Positions.Moving;
            String str = new String("You start following {2}.", "You start following {2}.", "{1} starts following you.", "{1} starts following you.", "{1} starts following {2}.", null);
            this.object_.Area.Send(str, this.object_, this.following_, 0);
            this.lastPoints_.Enqueue(this.object_.Location.Point);
            if (this.following_ is Area.Path)
            {
                Area.Direction dir = Area.ReverseDirection(this.startingDirection_);
                this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, dir));
                switch (dir)
                {
                    case Area.Direction.East:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Northeast));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Southeast));
                        break;
                    case Area.Direction.North:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Northeast));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Northwest));
                        break;
                    case Area.Direction.Northeast:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.North));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.East));
                        break;
                    case Area.Direction.Northwest:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.North));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.West));
                        break;
                    case Area.Direction.South:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Southeast));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Southwest));
                        break;
                    case Area.Direction.Southeast:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.South));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.East));
                        break;
                    case Area.Direction.Southwest:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.South));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.West));
                        break;
                    case Area.Direction.West:
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Southwest));
                        this.lastPoints_.Enqueue(Area.GetNextPoint(this.object_.Location.Point, Area.Direction.Northwest));
                        break;
                }
            }

            if (this.object_ is Character)
            {
                Character ch = this.object_ as Character;
                if (ch.User != null)
                {
                    ch.User.SendHelp("Type LOOK periodically to check your current location.");
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public override bool Stop(bool silent)
        {
            object_.Position = Object.Positions.Stopped;
            if (!silent)
            {
                String str = new String("You stop following {2}.", "You stop following {2}.", "{1} stops following you.", "{1} stops following you.", "{1} stops following {2}.", null);
                this.object_.Area.Send(str, this.object_, this.following_, 0);
            }
            this.object_.RemoveAction(this);
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override bool Wait()
        {
            paused_ = true;
            object_.Position = Object.Positions.Stopped;
            object_.Send("You wait.");
            if (this.object_ is Character)
            {
                Character ch = this.object_ as Character;
                if (ch.User != null)
                {
                    ch.User.SendHelp("Type 'go' to resume.");
                }
            }
            object_.Area.Send(object_.Name + " stops.", object_.Location, 0, object_, new Function<bool, IObject>[] { Object.CanHear }, object_);
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Go()
        {
            paused_ = false;
            object_.Position = Object.Positions.Moving;
            String str = new String("You continue following {2}.", "You continue following {2}.", "{1} continues to follow you.", "{1} continues to follow you.", "{1} continues to follow {2}.", null);
            this.object_.Area.Send(str, this.object_, this.following_, 0);
        }

        [Obfuscation(Exclude = false)]
        public override bool Finish()
        {
            object_.Position = Object.Positions.Stopped;
            object_.Send("You have reached the end of " + this.following_.Name + ".");
            this.Stop(false);
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if (this.paused_)
            {
                return;
            }

            if (this.object_.Parent == null 
                || !(this.object_.Parent is Area)
                || this.following_ == null 
                || this.object_.Parent != this.following_.Parent)
            {
                this.Stop(false);
                return;
            }

            if (((DateTime.Now - lastUpdate_).TotalMilliseconds / 500) >= (6 - this.speed_))
            {
                if (this.following_ is Area.Path)
                {
                    Area.Path path = this.following_ as Area.Path;
                    Point nextPoint;
                    if (!path.GetNextPoint(this.object_.Location.Point, this.lastPoints_, out nextPoint))
                        this.Finish();
                    else if (!this.object_.Move(this.object_.GetDirection(nextPoint), new String(null, null, null, null, "{1} leaves {dir}.", null), new String(null, null, null, null, "{1} arrives from the {dir}.", null), false))
                    {
                        this.Stop(false);
                        return;
                    }

                    this.lastPoints_.Enqueue(nextPoint);
                    //if (this.lastPoints_.Count > 5)
                    //    this.lastPoints_.Dequeue();
                }
                else if (this.following_ is Object)
                {
                    if (this.object_.Location.Equals(this.following_.Location))
                        return;

                    Point p = ((Object)this.following_).Location.Point;
                    if (this.object_.Distance(this.following_) > 10)
                    {
                        this.object_.Send("You have lost track of " + this.following_.Name + ".");
                        this.Stop(false);
                    }
                    else if (!object_.Move(object_.GetDirection(p), new String(null, null, null, null, "{1} leaves {dir}.", null), new String(null, null, null, null, "{1} arrives from the {dir}.", null), false))
                        this.Stop(false);                    
                }
                this.lastUpdate_ = DateTime.Now;
            }
        }
    }
}
