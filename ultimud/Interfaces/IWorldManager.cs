using System;
using System.Collections.Generic;
using System.Text;

namespace Ultimud
{
    public interface IWorldManager
    {
        void AddObject(IObject o);
        void RemoveObject(IObject o);
        void RemoveObject(Guid id);
        IObject GetObject(Guid id);
        bool ContainsObject(Guid id);
        T FindObject<T>(string name) where T : class, IObject;
    }
}
