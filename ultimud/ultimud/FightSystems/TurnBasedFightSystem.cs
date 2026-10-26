using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class TurnBasedFightSystem : IFightSystem
    {
        [NonSerialized]
        [Obfuscation(Exclude = false)]
        Dictionary<Object, Object> participants_;
        [NonSerialized]
        [Obfuscation(Exclude = false)]
        System.Timers.Timer updateTimer_;
        [NonSerialized]
        [Obfuscation(Exclude = false)]
        Object currentTurn_;
        [NonSerialized]
        [Obfuscation(Exclude = false)]
        DateTime started_;
        [NonSerialized]
        [Obfuscation(Exclude = false)]
        DateTime turnStarted_;

        int timeLimit_ = 600;
        int turnLength_ = 20;

        #region Properties

        [Browsable(false)]
        public Dictionary<Object, Object> Participants
        {
            get { return this.participants_; }
        }

        [Description("The number of seconds a fight can last.")]
        [DefaultValue(600)]
        public int TimeLimit
        {
            get { return this.timeLimit_; }
            set { this.timeLimit_ = value; }
        }

        [Description("The number of seconds a character has to perform their turn.")]
        [DefaultValue(20)]
        public int TurnLength
        {
            get { return this.turnLength_; }
            set { this.turnLength_ = value; }
        }

        #endregion

        [Obfuscation(Exclude = false)]
        void updateTimer__Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Update();
        }

        [Obfuscation(Exclude = false)]
        public bool CanUseCommand(User user, Command command)
        {
            return true;
        }

        [Obfuscation(Exclude = false)]
        public bool CanUseAbility(Object actor, Ability ability)
        {
            if (actor != this.currentTurn_)
            {
                actor.Send("It isn't your turn.");
                return false;
            }
            return true;
        }

        [Obfuscation(Exclude = false)]
        public bool ProcessInput(User user, string input)
        {
            return false;
        }

        [Obfuscation(Exclude = false)]
        public void Update()
        {
            if ((DateTime.Now - this.started_).TotalSeconds > this.TimeLimit)
            {
                this.Finish("The time limit has been reached!");
            }
            else if (this.currentTurn_ == null || !this.participants_.ContainsKey(this.currentTurn_))
            {
                this.NextTurn();
            }
            else if (this.currentTurn_ != null && (DateTime.Now - this.turnStarted_).TotalSeconds > this.TurnLength)
            {
                this.currentTurn_.Send("You have missed your turn!");
                this.NextTurn();
            }

            Server.Instance.FlushOutput();
        }

        [Obfuscation(Exclude = false)]
        public void Start()
        {
            this.started_ = DateTime.Now;
            this.NextTurn();
            this.updateTimer_.Start();
        }

        public void Finish(string message)
        {
            this.updateTimer_.Stop();
            Object[] participants = new Object[this.participants_.Keys.Count];
            this.participants_.Keys.CopyTo(participants, 0);
            foreach (Object o in participants)
            {
                o.Send(message);
                this.RemoveParticipant(o);
            }
        }

        [Obfuscation(Exclude = false)]
        public void Merge(IFightSystem other)
        {
            if (other == null || other == this)
                return;

            TurnBasedFightSystem otherTurn = other as TurnBasedFightSystem;
            if (otherTurn != null && otherTurn.updateTimer_ != null)
                otherTurn.updateTimer_.Stop();

            Dictionary<Object, Object> copy = new Dictionary<Object, Object>(other.Participants);
            other.Participants.Clear();
            foreach (KeyValuePair<Object, Object> pair in copy)
            {
                pair.Key.DieEvent -= other.RemoveParticipant;
                if (otherTurn != null)
                    pair.Key.AbilityPerformedEvent -= otherTurn.o_AbilityPerformedEvent;
                this.AddParticipant(pair.Key, pair.Value);
            }
        }

        [Obfuscation(Exclude = false)]
        public void AddParticipant(Object o, Object target)
        {
            lock (this.Participants)
            {
                if (!this.Participants.ContainsKey(o))
                {
                    this.Participants.Add(o, target);
                    o.DieEvent += this.RemoveParticipant;
                    o.AbilityPerformedEvent += o_AbilityPerformedEvent;
                }
            }
            o.FightSystem = this;
            target.FightSystem = this;
        }

        [Obfuscation(Exclude = false)]
        void o_AbilityPerformedEvent(Ability ability, Object actor, Object target, Ability.Status status)
        {
            if (actor == this.currentTurn_ && status != Ability.Status.Preparing)
                this.NextTurn();
        }

        [Obfuscation(Exclude = false)]
        public void AddParticipants(Object o, Object target)
        {
            this.AddParticipant(o, target);
            this.AddParticipant(target, o);
        }

        public void RemoveParticipant(Object o)
        {
            if (this.participants_ != null)
            {
                this.participants_.Remove(o);
            }
            o.FightSystem = null;
            o.DieEvent -= new Object.DieHandler(this.RemoveParticipant);
            o.AbilityPerformedEvent -= this.o_AbilityPerformedEvent;
            if (this.participants_ != null)
            {
                if (this.participants_.Count < 2)
                {
                    this.Finish("The fight is over.");
                }
                else if (this.currentTurn_ == o)
                {
                    this.NextTurn();
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public bool CheckDefend(Ability ability, Object actor, Object target)
        {
            foreach (Guid abilityId in ability.Defense)
            {
                Ability defenseAbility = Server.Instance.Get<Ability>(abilityId);
                if (defenseAbility != null && (!(target is Character) || ((Character)target).Abilities.ContainsKey(defenseAbility.Id)))
                {
                    if (defenseAbility.OnPerform(target, actor, false, null))
                        return true;
                }
            }

            return false;
        }

        [Obfuscation(Exclude = false)]
        public object Clone()
        {
            TurnBasedFightSystem newFightSystem = this.MemberwiseClone() as TurnBasedFightSystem;
            newFightSystem.participants_ = new Dictionary<Object, Object>();
            newFightSystem.updateTimer_ = new System.Timers.Timer();
            newFightSystem.updateTimer_.Elapsed += new System.Timers.ElapsedEventHandler(newFightSystem.updateTimer__Elapsed);
            newFightSystem.updateTimer_.Interval = 1000;
            return newFightSystem;
        }

        [Obfuscation(Exclude = false)]
        void NextTurn()
        {
            lock (this.Participants)
            {
                if (this.participants_.Count == 0)
                {
                    this.currentTurn_ = null;
                    return;
                }

                Dictionary<Object, Object>.KeyCollection.Enumerator enumerator = this.participants_.Keys.GetEnumerator();
                bool found = false;
                while (enumerator.MoveNext())
                {
                    if (this.currentTurn_ == null)
                    {
                        this.currentTurn_ = enumerator.Current;
                        found = true;
                        break;
                    }
                    else if (this.currentTurn_ == enumerator.Current)
                    {
                        break;
                    }
                }

                if (!found)
                {
                    if (enumerator.MoveNext())
                    {
                        this.currentTurn_ = enumerator.Current;
                    }
                    else
                    {
                        enumerator = this.participants_.Keys.GetEnumerator();
                        enumerator.MoveNext();
                        this.currentTurn_ = enumerator.Current;
                    }
                }

                this.currentTurn_.Send("You have " + this.TurnLength + " seconds to complete your turn.");
                this.Broadcast("It's " + this.currentTurn_.Name + "'s turn.", this.currentTurn_);
                this.turnStarted_ = DateTime.Now;
            }
        }

        [Obfuscation(Exclude = false)]
        void Broadcast(string message, params Object[] ignore)
        {
            List<Object> ignoreList = new List<Object>(ignore);
            lock (this.Participants)
            {
                foreach (Object o in this.Participants.Keys)
                {
                    if (!ignoreList.Contains(o))
                    {
                        o.Send(message);
                    }
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public void Update(Npc npc)
        {
            if (this.currentTurn_ == npc && !npc.Incapacitated && npc.CurrentAction == null)
            {
                Object target = null;
                if (!this.participants_.TryGetValue(npc, out target) || target == null)
                {
                    this.NextTurn();
                    return;
                }

                Ability ability = npc.GetRandomAggressiveAbility(target);
                if (ability != null)
                {
                    if (!ability.OnPerform(npc, target, true, npc.GetEquippedItem(ability.BodyPart)))
                        this.NextTurn();
                }
                else if (!npc.Location.Equals(target.Location))
                {
                    npc.Move(npc.GetDirection(target.Location.Point),
                        new String(null, null, null, null, "{1} leaves {dir}.", null),
                        new String(null, null, null, null, "{1} arrives from the {dir}.", null),
                        false);
                    this.NextTurn();
                }
                else
                {
                    this.NextTurn();
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public bool ConvertAlias(User user)
        {
            return true;
        }
    }
}
