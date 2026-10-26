using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class AutomatedFightSystem : IFightSystem
    {
        [NonSerialized]
        [Obfuscation(Exclude = false)]
        Dictionary<Object, Object> participants_;

        [NonSerialized]
        [Obfuscation(Exclude = false)]
        System.Timers.Timer updateTimer_;

        int updateFrequency_ = 3;
        Guid defaultAbilityId_;

        #region Properties
        [Browsable(false)]
        public Dictionary<Object, Object> Participants
        {
            get { return this.participants_; }
        }

        [Description("The amount of time in seconds between each update.")]
        [DefaultValue(3)]
        public int UpdateFrequency
        {
            get { return this.updateFrequency_; }
            set { this.updateFrequency_ = value; }
        }

        [DisplayName("Default Ability")]
        [Description("The ability that is automatically invoked each update. This will normally be set to a general attack ability.")]
        [TypeConverter(typeof(Server.IdToNameConverter<Ability>))]
        [Editor(typeof(Server.SingleObjectSelector<Ability>), typeof(UITypeEditor))]
        public Guid DefaultAbilityId
        {
            get { return this.defaultAbilityId_; }
            set { this.defaultAbilityId_ = value; }
        }

        [Browsable(false)]
        public Ability DefaultAbility
        {
            get { return Server.Instance.Get<Ability>(this.defaultAbilityId_); }
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
            Object target = null;
            this.participants_.TryGetValue(user.Character, out target);
            if ((command.Name == "travel" || command.Name == "follow") && target != null && target.Location.Equals(user.Character.Location))
            {
                user.Send("You can't do that, you're fighting!");
                return false;
            }
            return true;
        }

        [Obfuscation(Exclude = false)]
        public bool CanUseAbility(Object actor, Ability ability)
        {
            //if (ability.Target == Ability.Targets.None)
            //{
            //    actor.Send("This is an automated ability.");
            //    return false;
            //}
            return true;
        }

        [Obfuscation(Exclude = false)]
        public bool ProcessInput(User user, string input)
        {
            if ("truce".StartsWith(input, StringComparison.CurrentCultureIgnoreCase))
            {
                this.Finish(user.Character.Name + " has called a truce.");
                return true;
            }
            return false;
        }

        [Obfuscation(Exclude = false)]
        public void Start()
        {
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
        public void Update()
        {
            Dictionary<Object, Object> participants = new Dictionary<Object, Object>(this.participants_);
            foreach (KeyValuePair<Object, Object> pair in participants)
            {
                if (this.participants_.ContainsKey(pair.Key) && pair.Key.CurrentAction == null)
                {
                    Ability ability = this.DefaultAbility;
                    Character ch = pair.Key as Character;
                    if (ch != null)
                    {
                        foreach (Guid aid in ch.Abilities.Keys)
                        {
                            Ability a = Server.Instance.Get<Ability>(aid);
                            if (a != null && a.Auto && a.Style == Ability.Styles.Aggressive && a.CheckRange(pair.Key, pair.Value) && Server.RandomNumber.Next(4) == 1)
                            {
                                ability = a;
                                break;
                            }
                        }
                    }

                    if (ability != null)
                    {
                        if (ability.CheckRange(pair.Key, pair.Value))
                        {
                            Item item = null;
                            if (pair.Key is Character)
                                item = ((Character)pair.Key).GetEquippedItem(ability.BodyPart);
                            ability.OnPerform(pair.Key, pair.Value, true, item);
                        }
                    }
                    else
                    {
                        String str = new String(
                            "You stare aimlessly at {2}.", "You stare aimlessly at {2}.",
                            "{1} stares aimlessly at you.", "{1} stares aimlessly at you.", 
                            "{1} stares aimlessly at {2}.", null);
                        pair.Key.Area.Send(str, pair.Key, pair.Value, 0);
                    }
                }
            }

            Server.Instance.FlushOutput();
        }

        [Obfuscation(Exclude = false)]
        public void Update(Npc npc)
        {
            if (npc.CurrentAction == null && Server.RandomNumber.Next(100) < 10)
            {
                foreach (Guid id in npc.Abilities.Keys)
                {
                    Object target = this.Participants[npc];
                    Ability ability = Server.Instance.Get<Ability>(id);
                    if (ability != null
                        && ability.Style == Ability.Styles.Aggressive
                        && (ability.Target == Ability.Targets.All || ability.Target == Ability.Targets.Others)
                        && ability.CheckRange(npc, target)
                        && Server.RandomNumber.Next(100) < 50)
                    {
                        ability.OnPerform(npc, target, true, npc.GetEquippedItem(ability.BodyPart));
                        //npc.ReplaceCurrentAction(new AbilityAction(ability, npc, target, npc.GetEquippedItem(ability.BodyPart)));
                        break;
                    }
                }
            }
        }

        [Obfuscation(Exclude = false)]
        public void Merge(IFightSystem other)
        {
            if (other == null || other == this)
                return;

            AutomatedFightSystem otherAuto = other as AutomatedFightSystem;
            if (otherAuto != null && otherAuto.updateTimer_ != null)
                otherAuto.updateTimer_.Stop();

            Dictionary<Object, Object> copy = new Dictionary<Object, Object>(other.Participants);
            other.Participants.Clear();
            foreach (KeyValuePair<Object, Object> pair in copy)
            {
                pair.Key.DieEvent -= other.RemoveParticipant;
                this.AddParticipant(pair.Key, pair.Value);
            }
        }

        [Obfuscation(Exclude = false)]
        public void AddParticipant(Object o, Object target)
        {
            if (!this.participants_.ContainsKey(o))
            {
                this.participants_.Add(o, target);
                o.DieEvent += new Object.DieHandler(this.RemoveParticipant);
            }
            o.FightSystem = this;
            target.FightSystem = this;
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
                if (this.participants_.Count < 2)
                {
                    this.Finish("You have won the fight!");
                }
            }

            o.FightSystem = null;
            o.DieEvent -= this.RemoveParticipant;
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
            AutomatedFightSystem newFightSystem = this.MemberwiseClone() as AutomatedFightSystem;
            newFightSystem.participants_ = new Dictionary<Object, Object>();
            newFightSystem.updateTimer_ = new System.Timers.Timer();
            newFightSystem.updateTimer_.Elapsed += new System.Timers.ElapsedEventHandler(newFightSystem.updateTimer__Elapsed);
            newFightSystem.updateTimer_.Interval = this.UpdateFrequency * 1000;
            return newFightSystem;
        }

        [Obfuscation(Exclude = false)]
        public bool ConvertAlias(User user)
        {
            return true;
        }
    }
}
