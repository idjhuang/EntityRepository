using System;
using System.Collections.Generic;

namespace ObjectRepositoryContract
{
    public interface ICollection
    {
        IList<Type> SupportedTypes();
        object GetObject(object id, bool reload = false);
        void InsertObject(object obj);
        void UpdateObject(object obj);
        void DeleteObject(object obj);
        void RemoveReclaimedObjects();
        void RegisterReference(IReference reference);
        IEnumerable<Type> GetAllTypes();
        IList<object> GetAllObjects(Type type);
    }
}