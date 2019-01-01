using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IBlueprintManager
    {
        void AddBlueprintType(Type type);
        void AddBlueprint(IIdentifiable i);
        IIdentifiable GetBlueprint(Type type, string name, bool exact);
        IIdentifiable GetBlueprint(string typename, string name, bool exact);
        T GetBlueprint<T>(string name, bool exact) where T : class, IIdentifiable;
        T GetBlueprint<T>(Guid id) where T : class, IIdentifiable;
        bool IsBlueprintType<T>() where T : class, IIdentifiable;
        bool ContainsBlueprint<T>(Guid id) where T : class, IIdentifiable;
        List<T> GetBlueprints<T>() where T : class, IIdentifiable;
        void DeleteBlueprint(IIdentifiable i);
        //T Create<T>(string name) where T : class, IIdentifiable;
        //object Create(string typename, string name);
    }
}
