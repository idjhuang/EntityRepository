using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ObjectRepositoryContract;
using ObjectRepositoryImpl.ObjectRepositoryTableAdapters;
using ObjectResourceManager;

namespace ObjectRepositoryImpl
{
    public class ObjectCollection : ICollection
    {
        private readonly Dictionary<Guid, WeakReference>  _objectTable = new Dictionary<Guid, WeakReference>();
        private readonly EventLog _log = new EventLog("Application", ".", "Object Repository");

        public IList<Type> SupportedTypes()
        {
            // use ObjectValue indicate default collection
            return new[] {typeof (ObjectRepositoryContract.Object)};
        }

        public object GetObject(object id, bool reload = false)
        {
            if (id == null) return null;
            var guid = Guid.Empty;
            if (id is string) guid = new Guid((string)id);
            if (id is Guid) guid = (Guid)id;
            if (guid == Guid.Empty) return null;
            if (_objectTable.ContainsKey(guid))
            {
                var reference = _objectTable[guid];
                if (reference.Target != null && !reload) return reference.Target;
                _objectTable.Remove(guid);
            }
            try
            {
                var adapter = new ObjectRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                var objTbl = adapter.GetObject(guid);
                if (objTbl.Rows.Count == 0) return null;
                var objRow = objTbl.Rows[0] as ObjectRepository.ObjectRepositoryRow;
                var type = TypeRepository.GetType(objRow.Type);
                if (type == null) throw new TypeAccessException($"Type {objRow.Type} not found.");
                // Deserialize object value
                var objVal = JsonConvert.DeserializeObject(objRow.JSON, type);
                var obj = TransactionalObject.CreateTransactionalObject(type, objVal as ObjectRepositoryContract.Object);
                var reference = new WeakReference(obj);
                _objectTable.Add(guid, reference);
                return obj;
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Retrieve object failure ({guid}): {e.Message}");
                throw;
            }
        }

        public void InsertObject(object obj)
        {
            dynamic o = obj;
            var objVal = o.Value;
            if (!(objVal is ObjectRepositoryContract.Object)) return;
            var id = objVal.Id;
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
                var json = JsonConvert.SerializeObject(objVal);
                var adapter = new ObjectRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                adapter.AddObject(guid, objVal.GetType().FullName, json);
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Insert object failure ({guid}): {e.Message}");
                throw;
            }
        }

        public void UpdateObject(object obj)
        {
            dynamic o = obj;
            var ojbVal = o.Value;
            if (!(ojbVal is ObjectRepositoryContract.Object)) return;
            var id = ojbVal.Id;
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
                var json = JsonConvert.SerializeObject(ojbVal);
                var adapter = new ObjectRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                adapter.AddObject(guid, ojbVal.Type, json);
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Update object failure ({guid}): {e.Message}");
                throw;
            }
        }

        public void DeleteObject(object obj)
        {
            dynamic o = obj;
            var ojbVal = o.Value;
            if (!(ojbVal is ObjectRepositoryContract.Object)) return;
            var id = ojbVal.Id;
            if (id == null) return;
            var guid = Guid.Empty;
            if (id is string) guid = new Guid((string)id);
            if (id is Guid) guid = (Guid)id;
            if (guid == Guid.Empty) return;
            if (_objectTable.ContainsKey(guid)) _objectTable.Remove(guid);
            try
            {
                var adapter = new ObjectRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                adapter.DeleteObject(guid);
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Delete object failure ({guid}): {e.Message}");
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
                var adapter = new ObjectRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                var typeTbl = adapter.GetAllTypes();
                typeNameList.AddRange(from ObjectRepository.ObjectRepositoryRow row in typeTbl.Rows select row.Type);
                return typeList;
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Get types failure: {e.Message}");
                throw;
            }
        }

        public IList<object> GetAllObjects(Type type)
        {
            try
            {
                var objList = new List<object>();
                var adapter = new ObjectRepositoryTableAdapter();
                if (TransactionScopeUtil.DbConnection != null)
                    adapter.Connection = TransactionScopeUtil.DbConnection as SqlConnection;
                var objTbl = adapter.GetObjectsByType(type.FullName);
                foreach (ObjectRepository.ObjectRepositoryRow row in objTbl.Rows)
                {
                    var objVal = JsonConvert.DeserializeObject(row.JSON, type) as ObjectRepositoryContract.Object;
                    var guid = Guid.Empty;
                    if (objVal.Id is string) guid = new Guid((string)objVal.Id);
                    if (objVal.Id is Guid) guid = (Guid)objVal.Id;
                    if (_objectTable.ContainsKey(guid))
                        objList.Add(_objectTable[guid].Target);
                    else
                    {
                        var obj = TransactionalObject.CreateTransactionalObject(type, objVal);
                        var reference = new WeakReference(obj);
                        _objectTable.Add(guid, reference);
                        objList.Add(obj);
                    }
                }
                return objList;
            }
            catch (Exception e)
            {
                _log.WriteEntry($"Get all objects of type {type} failure: {e.Message}");
                throw;
            }
        }
    }
}
