using System;
using System.Collections;
using System.Collections.Generic;
using Damdai.Collections;

namespace Damdai.Pathfinding
{
    /// <summary>
    /// Class for performing A* pathfinding
    /// </summary>
    public sealed class AStar<T> where T : AStarNode
    {
        T startNode_;
        T goalNode_;
        Heap<T> openList_ = new Heap<T>();
        Heap<T> closedList_ = new Heap<T>();
        List<AStarNode> successors_ = new List<AStarNode>();
        List<T> solution_ = new List<T>();
        int maxIterations_ = 2500;

        #region Properties

        /// <summary>
        /// Holds the solution after pathfinding is done. <see>FindPath()</see>
        /// </summary>
        public List<T> Solution
        {
            get { return this.solution_; }
        }

        public int MaxIterations
        {
            get { return this.maxIterations_; }
            set { this.maxIterations_ = value; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Prints all the nodes in a list
        /// </summary>
        /// <param name="ANodeList">List to print</param>
        private void PrintNodeList(object list)
        {
            Console.WriteLine("Node list:");
            foreach (AStarNode n in (list as IEnumerable))
            {
                n.PrintNodeInfo();
            }
            Console.WriteLine("=====");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds the shortest path from the start node to the goal node
        /// </summary>
        /// <param name="AStartNode">Start node</param>
        /// <param name="AGoalNode">Goal node</param>
        public void FindPath(T startNode, T goalNode)
        {
            this.startNode_ = startNode;
            this.goalNode_ = goalNode;

            this.openList_.Add(this.startNode_);
            int iterations = 0;
            while (this.openList_.Count > 0 && ++iterations <= this.maxIterations_)
            {
                // Get the node with the lowest TotalCost
                T currentNode = this.openList_.Pop();

                // If the node is the goal copy the path to the solution array
                if (currentNode.IsGoal())
                {
                    while (currentNode != null)
                    {
                        this.solution_.Insert(0, currentNode);
                        currentNode = currentNode.Parent as T;
                    }
                    break;
                }

                // Get successors to the current node
                currentNode.GetSuccessors(this.successors_);
                foreach (T successorNode in this.successors_)
                {
                    // Test if the currect successor node is on the open list, if it is and
                    // the TotalCost is higher, we will throw away the current successor.
                    T openNode = null;
                    if (this.openList_.Contains(successorNode))
                        openNode = this.openList_[this.openList_.IndexOf(successorNode)];
                    if (openNode != null && successorNode.TotalCost > openNode.TotalCost)
                        continue;

                    // Test if the currect successor node is on the closed list, if it is and
                    // the TotalCost is higher, we will throw away the current successor.
                    T closedNode = null;
                    if (this.closedList_.Contains(successorNode))
                        closedNode = this.closedList_[this.closedList_.IndexOf(successorNode)];
                    if (closedNode != null && successorNode.TotalCost > closedNode.TotalCost)
                        continue;

                    // Remove the old successor from the open list
                    if (openNode != null)
                        this.openList_.Remove(openNode);

                    // Remove the old successor from the closed list
                    if (closedNode != null)
                        this.closedList_.Remove(closedNode);

                    // Add the current successor to the open list
                    this.openList_.Push(successorNode);
                }
                // Add the current node to the closed list
                this.closedList_.Add(currentNode);
            }
        }

        #endregion
    }

    /// <summary>
    /// Base class for pathfinding nodes, it holds no actual information about the map. 
    /// An inherited class must be constructed from this class and all virtual methods must be 
    /// implemented. Note, that calling base() in the overridden methods is not needed.
    /// </summary>
    public class AStarNode : IComparable
    {
        private AStarNode parent_;
        private double cost_;
        private double goalEstimate_;
        private AStarNode goalNode_;

        #region Properties

        /// <summary>
        /// The parent of the node.
        /// </summary>
        public AStarNode Parent
        {
            get
            {
                return parent_;
            }
            set
            {
                parent_ = value;
            }
        }

        /// <summary>
        /// The accumulative cost of the path until now.
        /// </summary>
        public double Cost
        {
            set { cost_ = value; }
            get { return cost_; }
        }

        /// <summary>
        /// The estimated cost to the goal from here.
        /// </summary>
        public double GoalEstimate
        {
            set
            {
                goalEstimate_ = value;
            }
            get
            {
                Calculate();
                return (goalEstimate_);
            }
        }

        /// <summary>
        /// The cost plus the estimated cost to the goal from here.
        /// </summary>
        public double TotalCost
        {
            get
            {
                return (Cost + GoalEstimate);
            }
        }

        /// <summary>
        /// The goal node.
        /// </summary>
        public AStarNode GoalNode
        {
            set
            {
                goalNode_ = value;
                Calculate();
            }
            get
            {
                return goalNode_;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="AParent">The node's parent</param>
        /// <param name="AGoalNode">The goal node</param>
        /// <param name="ACost">The accumulative cost until now</param>
        public AStarNode(AStarNode AParent, AStarNode AGoalNode, double ACost)
        {
            parent_ = AParent;
            cost_ = ACost;
            GoalNode = AGoalNode;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the current node is the goal.
        /// </summary>
        /// <returns>Returns true if current node is the goal</returns>
        public bool IsGoal()
        {
            return this.IsSameState(this.GoalNode);
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Determines wheather the current node is the same state as the on passed.
        /// </summary>
        /// <param name="ANode">AStarNode to compare the current node to</param>
        /// <returns>Returns true if they are the same state</returns>
        public virtual bool IsSameState(AStarNode node)
        {
            return false;
        }

        /// <summary>
        /// Calculates the estimated cost for the remaining trip to the goal.
        /// </summary>
        public virtual void Calculate()
        {
            this.GoalEstimate = 0;
        }

        /// <summary>
        /// Gets all successors nodes from the current node and adds them to the successor list
        /// </summary>
        /// <param name="ASuccessors">List in which the successors will be added</param>
        public virtual void GetSuccessors(List<AStarNode> successors)
        {
        }

        /// <summary>
        /// Prints information about the current node
        /// </summary>
        public virtual void PrintNodeInfo()
        {
        }

        #endregion

        #region Overridden Methods

        public override bool Equals(object obj)
        {
            return this.IsSameState((AStarNode)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return (-this.TotalCost.CompareTo(((AStarNode)obj).TotalCost));
        }

        #endregion
    }
}