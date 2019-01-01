using System;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;

namespace Ultimud
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Obfuscation(Exclude = true)]
    public interface IFightSystem
    {
        IScript OnStart { get; set; }
        IScript OnEnd { get; set; }
        IScript OnUpdate { get; set; }
        IScript OnAbilityPerformed { get; set; }
        uint UpdateFrequency { get; set; }

        void Start(ICharacter actor, ICharacter target);
        void End();
        void Update();
        void AddFighter(ICharacter ch);
        void RemoveFighter(ICharacter ch);
        void AbilityPerformed(IAbility ability, ICharacter actor, ICharacter target, AbilityStatus status);
        bool HandleInput(IUser user, string input);
        bool IsUnderAttack(ICharacter ch);
    }
}
