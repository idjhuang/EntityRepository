using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EntityRepository
{
    public abstract class CachedTransactionalEntityCollection<T, TId> : ICollection where T : Entity
    {
        protected readonly Dictionary<TId, TransactionalEntity<T>> ObjectTable = new Dictionary<TId, TransactionalEntity<T>>();
        protected readonly EventLog Log = new EventLog("Application", ".", "Entity Repository");

        // constructor of implemention must load all entities

        public IList<T> GetEntityList()
        {
            return ObjectTable.Values.Select(v => v.GetEntity()).ToList();
        }

        public void Sync(TId id, bool delete = false)
        {
            if (delete)
            {
                if (ObjectTable.ContainsKey(id)) ObjectTable.Remove(id);
            }
            else
            {
                GetEntity(id, true);
            }
        }

        public virtual IList<Type> SupportedTypes()
        {
            return new List<Type> { typeof(T) };
        }

        public virtual object GetEntity(object id, bool reload = false)
        {
            if (id == null) throw new ArgumentException("Object's Id is null.");
            if (id.GetType() != typeof(TId)) throw new ArgumentException("Object's Id's type mismatch.");
            var objId = (TId)id;
            try
            {
                if (ObjectTable.ContainsKey(objId))
                {
                    var obj = ObjectTable[objId];
                    if (!reload) return obj;
                    // reload entity
                    var entity = GetEntityImpl(objId);
                    // if cached then update entity, else remove from cache
                    if (IsCached(entity))
                        obj.SetEntity(entity);
                    else
                        ObjectTable.Remove(objId);
                    return obj;
                }
                else
                {
                    var entity = GetEntityImpl(objId);
                    var obj = new TransactionalEntity<T>(entity);
                    if (IsCached(entity)) ObjectTable.Add(objId, obj);
                    return obj;
                }
            }
            catch (Exception e)
            {
                Log.WriteEntry($"Retrieve object failure ({objId}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected abstract T GetEntityImpl(TId id);

        public virtual void InsertEntity(object obj)
        {
            if (obj.GetType() != typeof(TransactionalEntity<T>)) throw new ArgumentException("Object's type mismatch.");
            var entity = ((TransactionalEntity<T>) obj).GetValue();
            var id = (TId)entity.Id;
            var isCached = IsCached(entity);
            if (ObjectTable.ContainsKey(id)) throw new ArgumentException($"Duplicate object Id: {id}.");
            if (isCached) ObjectTable.Add(id, (TransactionalEntity<T>) obj);
            try
            {
                InsertEntityImpl(entity);
                CollectionRepository.LastUpdateTime[typeof(T)] = DateTime.Now;
            }
            catch (Exception e)
            {
                Log.WriteEntry($"Insert object failure ({id}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected abstract void InsertEntityImpl(T entity);

        public virtual void UpdateEntity(object obj)
        {
            if (obj.GetType() != typeof(TransactionalEntity<T>)) throw new ArgumentException("Object's type mismatch.");
            var entity = ((TransactionalEntity<T>)obj).GetValue();
            var id = (TId)entity.Id;
            var isCached = IsCached(entity);
            // if entity is valid and not in colection, add it
            if (!ObjectTable.ContainsKey(id) && isCached)
            {
                ObjectTable.Add(id, (TransactionalEntity<T>) obj);
            }
            // if entity is not valid remove it from collection
            if (!isCached) ObjectTable.Remove(id);
            try
            {
                UpdateEntityImpl(entity);
                CollectionRepository.LastUpdateTime[typeof(T)] = DateTime.Now;
            }
            catch (Exception e)
            {
                Log.WriteEntry($"Update object failure ({id}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        // check entity should keep in collection or not
        protected abstract bool IsCached(T entity);
        protected abstract void UpdateEntityImpl(T entity);

        public virtual void DeleteEntity(object obj)
        {
            if (obj.GetType() != typeof(TransactionalEntity<T>)) throw new ArgumentException("Object's type mismatch.");
            var entity = ((TransactionalEntity<T>)obj).GetValue();
            var id = (TId)entity.Id;
            if (ObjectTable.ContainsKey(id)) ObjectTable.Remove(id);
            try
            {
                DeleteEntityImpl(entity);
                CollectionRepository.LastUpdateTime[typeof(T)] = DateTime.Now;
            }
            catch (Exception e)
            {
                Log.WriteEntry($"Delete object failure ({id}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected abstract void DeleteEntityImpl(T entity);

        public virtual void RemoveReclaimedObjects()
        {
        }

        public virtual void RegisterReference(IReference reference) { }

        public virtual IEnumerable<Type> GetAllTypes()
        {
            return new List<Type> { typeof(T) };
        }

        public virtual IList<object> GetAllEntities(Type type = null)
        {
            try
            {
                return (IList<object>) ObjectTable.Values.ToList();
            }
            catch (Exception e)
            {
                Log.WriteEntry($"Get all objects of type {typeof(T)} failure: {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }
    }
}