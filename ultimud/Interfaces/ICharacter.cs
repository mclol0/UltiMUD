using System;
using System.Collections.Generic;
using System.Text;
using Damdai;

namespace Ultimud
{
    public interface ICharacter : IObject
    {
        bool IsRecovering { get; }
        bool IsInventoryFull { get; }
        IRace Race { get; set; }
        IJob Job { get; set; }
        Guid RaceId { get; set; }
        Guid JobId { get; set; }
        IFightSystem Fight { get; set; }
        IUser User { get; set; }
        DateTime LastAggressiveAct { get; set; }
        List<IItem> Inventory { get; }
        Dictionary<BodyLocation, IItem> Equipment { get; set; }
        Dictionary<Guid, int> Abilities { get; set; }
        Gender Sex { get; set; }
        ICharacter Fighting { get; set; }

        string GenderFilter(string neutral, string male, string female);
        string GenderFilter(string[] vars);
        bool CanLearnAbility(IAbility ability, bool checkrequirements);
        bool CanDoAbility(IAbility ability, IObject target, bool quiet);
        bool HasAbility(Guid id);
        bool HasAbility(IAbility ability);
        bool HasAbility(string abilityname);
        void AddAbility(Guid id);
        void AddAbility(IAbility ability);
        void RaiseAbilityLevel(IAbility ability);
        void Use(IItem item, bool quiet);
        int GetAbilityLevel(IAbility ability);
        IItem GetEquippedItem(BodyLocation location);
        IItem GetEquippedItem(string name);
        IAbility GetRandomAggressiveAbility(IObject target);
        void DoAbility(IAbility ability, IObject target, bool prepare, IItem item);

        //event Function_void<IAbility, ICharacter, IObject, AbilityStatus> AbilityPerformed;
    }
}
