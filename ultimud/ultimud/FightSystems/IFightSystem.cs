using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public interface IFightSystem : ICloneable
    {
        Dictionary<Object, Object> Participants
        {
            get;
        }

        [Obfuscation(Exclude = false)]
        bool CanUseCommand(User user, Command command);

        [Obfuscation(Exclude = false)]
        bool CanUseAbility(Object actor, Ability ability);

        [Obfuscation(Exclude = false)]
        bool ProcessInput(User user, string input);

        [Obfuscation(Exclude = false)]
        void Update();

        [Obfuscation(Exclude = false)]
        void Update(Npc npc);

        [Obfuscation(Exclude = false)]
        void Start();

        void Finish(string message);

        [Obfuscation(Exclude = false)]
        void Merge(IFightSystem other);

        [Obfuscation(Exclude = false)]
        void AddParticipant(Object o, Object target);

        [Obfuscation(Exclude = false)]
        void AddParticipants(Object o, Object target);

        void RemoveParticipant(Object o);

        [Obfuscation(Exclude = false)]
        bool CheckDefend(Ability ability, Object actor, Object target);

        [Obfuscation(Exclude = false)]
        bool ConvertAlias(User user);
    }
}
