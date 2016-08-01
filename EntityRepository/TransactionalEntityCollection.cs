using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EntityRepository
{
    public abstract class TransactionalEntityCollection<T, TId> : ICollection where T : Entity
    {
        protected readonly Dictionary<TId, WeakReference> ObjectTable = new Dictionary<TId, WeakReference>();
        protected readonly EventLog Log = new EventLog("Application", ".", "Entity Repository");

        public virtual IList<Type> SupportedTypes()
        {
            return new List<Type> {typeof (T)};
        }

        public virtual object GetEntity(object id, bool reload = false)
        {
            if (id == null) throw new ArgumentException("Object's Id is null.");
            if (id.GetType() != typeof(TId)) throw new ArgumentException("Object's Id's type mismatch.");
            var objId = (TId) id;
            if (ObjectTable.ContainsKey(objId))
            {
                var reference = ObjectTable[objId];
                if (reference.Target != null && !reload) return reference.Target;
                ObjectTable.Remove(objId);
            }
            try
            {
                var obj = GetEntityImpl(objId);
                var reference = new WeakReference(obj);
                ObjectTable.Add(objId, reference);
                return obj;
            }
            catch (Exception e)
            {
                Log.WriteEntry($"Retrieve object failure ({objId}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected abstract TransactionalEntity<T> GetEntityImpl(TId id);

        public virtual void InsertEntity(object obj)
        {
            if (obj.GetType() != typeof(TransactionalEntity<T>)) throw new ArgumentException("Object's type mismatch.");
            var entity = ((TransactionalEntity<T>) obj).GetValue();
            var id = (TId) entity.Id;
            if (ObjectTable.ContainsKey(id)) throw new ArgumentException($"Duplicate object Id: {id}.");
            var reference = new WeakReference(obj);
            ObjectTable.Add(id, reference);
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
            if (!ObjectTable.ContainsKey(id))
            {
                var reference = new WeakReference(obj);
                ObjectTable.Add(id, reference);
            }
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
            var toRemove = (from item in ObjectTable where item.Value.Target == null select item.Key).ToList();
            foreach (var guid in toRemove)
            {
                ObjectTable.Remove(guid);
            }
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
                RemoveReclaimedObjects();
                return GetAllEntitiesImpl();
            }
            catch (Exception e)
            {
                Log.WriteEntry($"Get all objects of type {typeof(T)} failure: {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected abstract IList<object> GetAllEntitiesImpl();
    }
}