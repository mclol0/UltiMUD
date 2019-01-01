//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.ComponentModel;

//namespace Ultimud
//{
//    [Serializable]
//    public class TurnBasedFightSystem : IFightSystem
//    {
//        [NonSerialized]
//        Dictionary<Object, Object> participants_;
//        [NonSerialized]
//        System.Timers.Timer updateTimer_;
//        [NonSerialized]
//        Object currentTurn_;
//        [NonSerialized]
//        DateTime started_;
//        [NonSerialized]
//        DateTime turnStarted_;

//        int timeLimit_ = 600;
//        int turnLength_ = 20;

//        #region Properties

//        [Browsable(false)]
//        public Dictionary<Object, Object> Participants
//        {
//            get { return this.participants_; }
//        }

//        [Description("The number of seconds a fight can last.")]
//        [DefaultValue(600)]
//        public int TimeLimit
//        {
//            get { return this.timeLimit_; }
//            set { this.timeLimit_ = value; }
//        }

//        [Description("The number of seconds a character has to perform their turn.")]
//        [DefaultValue(20)]
//        public int TurnLength
//        {
//            get { return this.turnLength_; }
//            set { this.TurnLength = value; }
//        }

//        #endregion

//        void updateTimer__Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//        {
//            this.Update();
//        }

//        public bool CanUseCommand(User user, Command command)
//        {
//            return true;
//        }

//        public bool CanUseAbility(Object actor, Ability ability)
//        {
//            if (actor != this.currentTurn_)
//            {
//                actor.Send("It isn't your turn.");
//                return false;
//            }
//            return true;
//        }

//        public bool ProcessInput(User user, string input)
//        {
//            return false;
//        }

//        public void Update()
//        {
//            if ((DateTime.Now - this.started_).TotalSeconds > this.TimeLimit)
//            {
//                this.Finish("The time limit has been reached!");
//            }
//            else if (this.currentTurn_ == null || !this.participants_.ContainsKey(this.currentTurn_))
//            {
//                this.NextTurn();
//            }
//            else if (this.currentTurn_ != null && (DateTime.Now - this.turnStarted_).TotalSeconds > this.TurnLength)
//            {
//                this.currentTurn_.Send("You have missed your turn!");
//                this.NextTurn();
//            }

//            Server.Instance.FlushOutput();
//        }

//        public void Start()
//        {
//            this.started_ = DateTime.Now;
//            this.updateTimer_.Start();
//        }

//        public void Finish(string message)
//        {
//            this.updateTimer_.Stop();
//            Object[] participants = new Object[this.participants_.Keys.Count];
//            this.participants_.Keys.CopyTo(participants, 0);
//            foreach (Object o in participants)
//            {
//                o.Send(message);
//                this.RemoveParticipant(o);
//            }
//        }

//        public void Merge(IFightSystem other)
//        {
//            foreach (KeyValuePair<Object, Object> pair in other.Participants)
//            {
//                if (!this.Participants.ContainsKey(pair.Key))
//                {
//                    this.Participants.Add(pair.Key, pair.Value);
//                }
//            }

//            other = this;
//        }

//        public void AddParticipant(Object o, Object target)
//        {
//            this.Participants.Add(o, target);
//            o.FightSystem = this;
//            target.FightSystem = this;
//            o.DieEvent += this.RemoveParticipant;
//            o.AbilityPerformedEvent += o_AbilityPerformedEvent;
//        }

//        void o_AbilityPerformedEvent(Ability ability, Object actor, Object target, Ability.Status status)
//        {
//            this.NextTurn();
//        }

//        public void AddParticipants(Object o, Object target)
//        {
//            this.AddParticipant(o, target);
//            this.AddParticipant(target, o);
//        }

//        public void RemoveParticipant(Object o)
//        {
//            this.participants_.Remove(o);
//            o.FightSystem = null;
//            o.DieEvent -= new Object.DieHandler(this.RemoveParticipant);
//            if (this.participants_.Count < 2)
//            {
//                this.Finish("The fight is over.");
//            }
//        }

//        public bool CheckDefend(Ability ability, Object actor, Object target)
//        {
//            foreach (Guid abilityId in ability.Defense)
//            {
//                Ability defenseAbility = Server.Instance.Get<Ability>(abilityId);
//                if (defenseAbility != null)
//                {
//                    return defenseAbility.OnPerform(target, actor, false, null);
//                }
//            }

//            return false;
//        }

//        public object Clone()
//        {
//            TurnBasedFightSystem newFightSystem = this.MemberwiseClone() as TurnBasedFightSystem;
//            newFightSystem.participants_ = new Dictionary<Object, Object>();
//            newFightSystem.updateTimer_ = new System.Timers.Timer();
//            newFightSystem.updateTimer_.Elapsed += new System.Timers.ElapsedEventHandler(newFightSystem.updateTimer__Elapsed);
//            newFightSystem.updateTimer_.Interval = 1000;
//            return newFightSystem;
//        }

//        void NextTurn()
//        {
//            lock (this.Participants)
//            {
//                Dictionary<Object, Object>.KeyCollection.Enumerator enumerator = this.participants_.Keys.GetEnumerator();
//                bool found = false;
//                while (enumerator.MoveNext())
//                {
//                    if (this.currentTurn_ == null)
//                    {
//                        this.currentTurn_ = enumerator.Current;
//                        found = true;
//                        break;
//                    }
//                    else if (this.currentTurn_ == enumerator.Current)
//                    {
//                        break;
//                    }
//                }

//                if (!found)
//                {
//                    if (enumerator.MoveNext())
//                    {
//                        this.currentTurn_ = enumerator.Current;
//                    }
//                    else
//                    {
//                        enumerator = this.participants_.Keys.GetEnumerator();
//                        enumerator.MoveNext();
//                        this.currentTurn_ = enumerator.Current;
//                    }
//                }

//                this.currentTurn_.Send("You have " + this.TurnLength + " seconds to complete your turn.");
//                this.Broadcast("It's " + this.currentTurn_.Name + "'s turn.", this.currentTurn_);
//                this.turnStarted_ = DateTime.Now;
//            }
//        }

//        void Broadcast(string message, params Object[] ignore)
//        {
//            List<Object> ignoreList = new List<Object>(ignore);
//            lock (this.Participants)
//            {
//                foreach (Object o in this.Participants.Keys)
//                {
//                    if (!ignoreList.Contains(o))
//                    {
//                        o.Send(message);
//                    }
//                }
//            }
//        }

//        public void Update(Npc npc)
//        {
//        }

//        public bool ConvertAlias(User user)
//        {
//            return true;
//        }
//    }
//}
