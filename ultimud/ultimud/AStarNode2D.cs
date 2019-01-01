using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Reflection;
using Damdai.Pathfinding;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal class AStarNode2D : AStarNode
    {
        Location location_ = new Location();
        Object object_;

        internal Location Location
        {
            get { return this.location_; }
        }

        #region Constructors

        /// <summary>
        /// Constructor for a node in a 2-dimensional map
        /// </summary>
        /// <param name="AParent">Parent of the node</param>
        /// <param name="AGoalNode">Goal node</param>
        /// <param name="ACost">Accumulative cost</param>
        /// <param name="AX">X-coordinate</param>
        /// <param name="AY">Y-coordinate</param>
        internal AStarNode2D(AStarNode parent, AStarNode goalNode, double cost, Location location, Object o)
            : base(parent, goalNode, cost)
        {
            this.location_ = location;
            this.object_ = o;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds a successor to a list if it is not impassible or the parent node
        /// </summary>
        /// <param name="ASuccessors">List of successors</param>
        /// <param name="AX">X-coordinate</param>
        /// <param name="AY">Y-coordinate</param>
        private void AddSuccessor(List<AStarNode> successors, Point p)
        {
            //Area.Layer layer = this.location_.Area.GetLayer<Area.Layer>(p, false);
            //if (((AStarNode2D)this.GoalNode).Location.Point != p && layer != null && layer.LayerFlags.Get((int)Area.Layer.LayerFlag.NoWalk))
            if (((AStarNode2D)this.GoalNode).Location.Point != p && !this.object_.CanMove(p, false))
                return;
            AStarNode2D newNode = new AStarNode2D(this, this.GoalNode, this.Cost + 1, new Location(this.location_.Area, p), this.object_);
            if (newNode.IsSameState(this.Parent))
                return;
            successors.Add(newNode);
        }

        #endregion

        #region Overidden Methods

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="ANode">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public override bool IsSameState(AStarNode node)
        {
            if (node == null)
            {
                return false;
            }

            AStarNode2D n = node as AStarNode2D;
            return (n.location_.Point.X == this.location_.Point.X && n.location_.Point.Y == this.location_.Point.Y);
        }

        /// <summary>
        /// Calculates the estimated cost for the remaining trip to the goal.
        /// </summary>
        public override void Calculate()
        {
            if (this.GoalNode != null)
            {
                AStarNode2D goalNode = this.GoalNode as AStarNode2D;
                double xd = this.location_.Point.X - goalNode.location_.Point.X;
                double yd = this.location_.Point.Y - goalNode.location_.Point.Y;
                // "Euclidean distance" - Used when search can move at any angle.
                //GoalEstimate = Math.Sqrt((xd*xd) + (yd*yd));
                // "Manhattan Distance" - Used when search can only move vertically and 
                // horizontally.
                //GoalEstimate = Math.Abs(xd) + Math.Abs(yd); 
                // "Diagonal Distance" - Used when the search can move in 8 directions.
                this.GoalEstimate = Math.Max(Math.Abs(xd), Math.Abs(yd));
            }
            else
            {
                this.GoalEstimate = 0;
            }
        }

        /// <summary>
        /// Gets all successors nodes from the current node and adds them to the successor list
        /// </summary>
        /// <param name="ASuccessors">List in which the successors will be added</param>
        public override void GetSuccessors(List<AStarNode> successors)
        {
            successors.Clear();
            this.AddSuccessor(successors, new Point(this.location_.Point.X - 1, this.location_.Point.Y));
            this.AddSuccessor(successors, new Point(this.location_.Point.X - 1, this.location_.Point.Y - 1));
            this.AddSuccessor(successors, new Point(this.location_.Point.X, this.location_.Point.Y - 1));
            this.AddSuccessor(successors, new Point(this.location_.Point.X + 1, this.location_.Point.Y - 1));
            this.AddSuccessor(successors, new Point(this.location_.Point.X + 1, this.location_.Point.Y));
            this.AddSuccessor(successors, new Point(this.location_.Point.X + 1, this.location_.Point.Y + 1));
            this.AddSuccessor(successors, new Point(this.location_.Point.X, this.location_.Point.Y + 1));
            this.AddSuccessor(successors, new Point(this.location_.Point.X - 1, this.location_.Point.Y + 1));
        }

        /// <summary>
        /// Prints information about the current node
        /// </summary>
        public override void PrintNodeInfo()
        {
            Console.WriteLine("X:\t{0}\tY:\t{1}\tCost:\t{2}\tEst:\t{3}\tTotal:\t{4}", this.location_.Point.X, this.location_.Point.Y, Cost, GoalEstimate, TotalCost);
        }

        #endregion
    }
}
