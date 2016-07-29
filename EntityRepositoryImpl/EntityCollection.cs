using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using EntityRepository;
using EntityRepositoryImpl.EntityRepositoryTableAdapters;
using Newtonsoft.Json;

namespace EntityRepositoryImpl
{
    public class EntityCollection : ICollection
    {
        private readonly Dictionary<Guid, WeakReference>  _objectTable = new Dictionary<Guid, WeakReference>();
        private readonly EventLog _log = new EventLog("Application", ".", "Entity Repository");

        public IList<Type> SupportedTypes()
        {
            // use ObjectValue indicate default collection
            return new[] {typeof (object)};
        }

        public object GetEntity(object id, bool reload = false)
        {
            if (id == null) throw new ArgumentException("Object's Id is null.");
            var guid = Guid.Empty;
            if (id is string) guid = new Guid((string)id);
            if (id is Guid) guid = (Guid)id;
            if (guid == Guid.Empty) throw new ArgumentException("Object's Id is empty.");
            if (_objectTable.ContainsKey(guid))
            {
                var reference = _objectTable[guid];
                if (reference.Target != null && !reload) return reference.Target;
                _objectTable.Remove(guid);
            }
            try
            {
                var adapter = new EntityRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                var tbl = adapter.GetEntity(guid);
                if (tbl.Rows.Count == 0) throw new Exception($"Object Id: {id} not found.");
                var row = tbl.Rows[0] as EntityRepository.EntityRepositoryRow;
                if (row == null) throw new ArgumentException($"Object Id: {id} not found.");
                var entityType = Type.GetType(row.Type);
                if (entityType == null) throw new TypeAccessException($"Type {row.Type} not found.");
                // Deserialize object value
                var entity = JsonConvert.DeserializeObject(row.JSON, entityType);
                var obj = TransactionalEntity.CreateTransactionalEntity(entityType, entity);
                var reference = new WeakReference(obj);
                _objectTable.Add(guid, reference);
                return obj;
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Retrieve object failure ({guid}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public void InsertEntity(object obj)
        {
            dynamic o = obj;
            var entity = o.GetEntity();
            if (!(entity is Entity)) throw new ArgumentException("object's value must be subclass of Entity.");
            var id = entity.Id;
            if (id == null) return;
            var guid = Guid.Empty;
            if (id is string) guid = new Guid((string)id);
            if (id is Guid) guid = (Guid)id;
            if (guid == Guid.Empty) return;
            if (_objectTable.ContainsKey(guid)) throw new ArgumentException($"Duplicate object Id: {guid}.");
            var reference = new WeakReference(obj);
            _objectTable.Add(guid, reference);
            try
            {
                var json = JsonConvert.SerializeObject(entity);
                var adapter = new EntityRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                adapter.AddEntity(guid, entity.GetType().AssemblyQualifiedName, json);
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Insert object failure ({guid}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public void UpdateEntity(object obj)
        {
            dynamic o = obj;
            var entity = o.GetEntity();
            if (!(entity is Entity)) throw new ArgumentException("object's value must be subclass of Entity.");
            var id = entity.Id;
            if (id == null) return;
            var guid = Guid.Empty;
            if (id is string) guid = new Guid((string)id);
            if (id is Guid) guid = (Guid)id;
            if (guid == Guid.Empty) return;
            if (!_objectTable.ContainsKey(guid))
            {
                var reference = new WeakReference(obj);
                _objectTable.Add(guid, reference);
            }
            try
            {
                var json = JsonConvert.SerializeObject(entity);
                var adapter = new EntityRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                adapter.AddEntity(guid, entity.GetType().AssemblyQualifiedName, json);
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Update object failure ({guid}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public void DeleteEntity(object obj)
        {
            dynamic o = obj;
            var entity = o.GetEntity();
            if (!(entity is Entity)) throw new ArgumentException("object's value must be subclass of Entity.");
            var id = entity.Id;
            if (id == null) return;
            var guid = Guid.Empty;
            if (id is string) guid = new Guid((string)id);
            if (id is Guid) guid = (Guid)id;
            if (guid == Guid.Empty) return;
            if (_objectTable.ContainsKey(guid)) _objectTable.Remove(guid);
            try
            {
                var adapter = new EntityRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                adapter.DeleteEntity(guid);
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Delete object failure ({guid}): {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public void RemoveReclaimedObjects()
        {
            var toRemove = (from item in _objectTable where item.Value.Target == null select item.Key).ToList();
            foreach (var guid in toRemove)
            {
                _objectTable.Remove(guid);
            }
        }

        public void RegisterReference(IReference reference)
        {
        }

        public IEnumerable<Type> GetAllTypes()
        {
            var typeList = new List<Type>();
            var typeNameList = new List<string>();
            try
            {
                var adapter = new EntityRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                var typeTbl = adapter.GetAllTypes();
                typeNameList.AddRange(from EntityRepository.EntityRepositoryRow row in typeTbl.Rows select row.Type);
                typeList.AddRange(typeNameList.Select(Type.GetType));
                return typeList;
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Get types failure: {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public IList<object> GetAllEntities(Type entityType)
        {
            try
            {
                RemoveReclaimedObjects();
                if(!typeof(Entity).IsAssignableFrom(entityType)) throw new ArgumentException("object's value must be subclass of Entity.");
                var objList = new List<object>();
                var adapter = new EntityRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null && TransactionScopeUtil.DbConnection.State == ConnectionState.Open)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                var tbl = adapter.GetEntitiesByType(entityType.AssemblyQualifiedName);
                foreach (EntityRepository.EntityRepositoryRow row in tbl.Rows)
                {
                    var entity = JsonConvert.DeserializeObject(row.JSON, entityType);
                    var obj = TransactionalEntity.CreateTransactionalEntity(entityType, entity);
                    var e = entity as Entity;
                    var guid = Guid.Empty;
                    if (e != null)
                    {
                        var id = e.Id;
                        if (id is string) guid = new Guid((string) id);
                        if (id is Guid) guid = (Guid) id;
                    }
                    if (_objectTable.ContainsKey(guid))
                        objList.Add(_objectTable[guid].Target);
                    else
                    {
                        var reference = new WeakReference(obj);
                        _objectTable.Add(guid, reference);
                        objList.Add(obj);
                    }
                }
                return objList;
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Get all objects of type {entityType} failure: {e.Message}", EventLogEntryType.Error);
                throw;
            }
        }
    }
}
