using System;
using System.Collections.Generic;

namespace EntityRepositoryContract
{
    public interface ICollection
    {
        IList<Type> SupportedTypes();
        object GetEntity(object id, bool reload = false);
        void InsertEntity(object obj);
        void UpdateEntity(object obj);
        void DeleteEntity(object obj);
        void RemoveReclaimedObjects();
        void RegisterReference(IReference reference);
        IEnumerable<Type> GetAllTypes();
        IList<object> GetAllEntities(Type type);
    }
}