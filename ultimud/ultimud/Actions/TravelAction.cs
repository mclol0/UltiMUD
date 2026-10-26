using System;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using Damdai.Pathfinding;
using Damdai;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class TravelAction : Action
    {
        object destination_;
        int speed_;

        [NonSerialized]
        [Obfuscation(Exclude = false)]
        AStar<AStarNode2D> astar_;

        [NonSerialized]
        [Obfuscation(Exclude = false)]
        List<AStarNode2D>.Enumerator astarEnumerator_;

        #region Properties
        public int Speed
        {
            get { return this.speed_; }
        }
        #endregion

        public TravelAction(Object traveler, object destination, int speed)
            : base(traveler)
        {
            this.destination_ = destination;
            this.speed_ = speed;

            if (!(destination is Area.Direction))
            {
                Point dest = Point.Empty;
                if (destination is Area.Layer)
                    dest = ((Area.Layer)destination).GetNearestPoint(traveler.Location.Point);
                else if (destination is Object)
                    dest = ((Object)destination).Location.Point;
                else if (destination is Point)
                    dest = (Point)destination;

                this.astar_ = new AStar<AStarNode2D>();
                AStarNode2D goalNode = new AStarNode2D(null, null, 0, new Location(traveler.Location.Area, dest), traveler);
                AStarNode2D startNode = new AStarNode2D(null, goalNode, 0, traveler.Location, traveler);
                startNode.GoalNode = goalNode;
                this.astar_.FindPath(startNode, goalNode);
                this.astarEnumerator_ = this.astar_.Solution.GetEnumerator();
                this.astarEnumerator_.MoveNext();
            }
        }

        [Obfuscation(Exclude = false)]
        public override void Start()
        {
            if (this.astar_ != null && this.astar_.Solution.Count == 0)
            {
                this.object_.Send("You cannot find a path to your destination.");
                this.Stop(true);
                return;
            }

            this.object_.Position = Object.Positions.Moving;

            if (this.destination_ is Area.Direction)
            {
                string dir = ((Area.Direction)this.destination_).ToString().ToLower();
                this.object_.Send("You start moving " + dir + ".");
                this.object_.Area.Send(this.object_.Name + " starts moving " + dir + ".", this.object_.Location, 0, this.object_, new Function<bool, IObject>[] { Object.CanHear }, this.object_);
            }
            else if (destination_ is Object)
            {
                this.object_.Send("You start moving towards " + ((Object)this.destination_).Name + ".");
                this.object_.Area.Send(object_.Name + " starts moving " 
                    + this.object_.GetDirection(((Object)destination_).Location.Point).ToString().ToLower() + ".",
                    object_.Location, 0, object_, new Function<bool, IObject>[] { Object.CanHear }, object_);
            }
            else if (destination_ is Point)
            {
                string dir = object_.GetDirection((Point)destination_).ToString().ToLower();
                object_.Send("You start moving " + dir + ".");
                object_.Area.Send(object_.Name + " starts moving " + dir + ".", object_.Location, 0, object_, new Function<bool, IObject>[] { Object.CanHear }, object_);
            }

            if (this.object_ is Character)
            {
                Character ch = this.object_ as Character;
                if (ch.User != null)
                    ch.User.SendHelp("Type LOOK periodically to check your current location.");
            }
        }

        [Obfuscation(Exclude = false)]
        public override bool Stop(bool silent)
        {
            object_.Position = Object.Positions.Stopped;
            if (!silent)
            {
                object_.Send("You stop.");
                object_.Area.Send(object_.Name + " stops.", object_.Location, 0, object_, new Function<bool, IObject>[] { Object.CanHear }, object_);
            }
            this.object_.RemoveAction(this);
            this.astarEnumerator_.Dispose();
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
                    ch.User.SendHelp("Type 'go' to resume.");
            }
            object_.Area.Send(object_.Name + " stops.", object_.Location, 0, object_, new Function<bool, IObject>[] { Object.CanHear }, object_);
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Go()
        {
            paused_ = false;
            object_.Position = Object.Positions.Moving;
            object_.Send("You continue moving.");
            object_.Area.Send(object_.Name + " continues moving.", object_.Location, 0, object_, new Function<bool, IObject>[] { Object.CanHear }, object_);
        }

        [Obfuscation(Exclude = false)]
        public override bool Finish()
        {
            object_.Position = Object.Positions.Stopped;
            object_.Send("You have arrived at your destination.");
            this.Stop(false);
            this.astarEnumerator_.Dispose();
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if (this.paused_)
                return;

            if (this.object_.Parent != null && this.object_.Parent is Area && 
                ((DateTime.Now - this.lastUpdate_).TotalMilliseconds / 500) >= (6 - this.Speed))
            {
                if (this.astar_ != null && !this.astarEnumerator_.MoveNext())
                {
                    if (this.astar_.Solution.Count == 0)
                    {
                        this.object_.Send("You cannot find a path to your destination.");
                        this.Stop(true);
                    }
                    else
                    {
                        this.Finish();
                    }
                }
                else if (this.destination_ == null)
                {
                    if (!this.object_.Move(
                        Area.Direction.Random,
                        new String(null, null, null, null, "{1} leaves {dir}.", null),
                        new String(null, null, null, null, "{1} wanders by aimlessly.", null), false))
                    {
                        this.object_.Send("Unable to move.");
                        this.Stop(false);
                    }
                }
                else
                {
                    String leavemsg = new String(null, null, null, null, "{1} leaves {dir}.", null);
                    String arrivemsg = new String(null, null, null, null, "{1} arrives from the {dir}.", null);
                    if (destination_ is Area.Direction)
                    {
                        if (!object_.Move((Area.Direction)destination_, leavemsg, arrivemsg, false))
                        {
                            this.object_.Send("Unable to move " + ((Area.Direction)destination_).ToString().ToLower() + ".");
                            this.Stop(false);
                        }
                    }
                    else
                    {
                        if (this.astar_ != null && !object_.Move(object_.GetDirection(((AStarNode2D)this.astarEnumerator_.Current).Location.Point), leavemsg, arrivemsg, false))
                        {
                            Area.Direction failedDir = object_.GetDirection(((AStarNode2D)this.astarEnumerator_.Current).Location.Point);
                            this.object_.Send("Unable to move " + failedDir.ToString().ToLower() + ".");
                            this.Stop(false);
                        }
                    }
                }
                this.lastUpdate_ = DateTime.Now;
                Server.Instance.FlushOutput();
            }
        }
    }
}
