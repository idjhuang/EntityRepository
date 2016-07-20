using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using ObjectResourceManager;

namespace EntityRepositoryContract
{
    public static class TransactionScopeUtil
    {
        [ThreadStatic] private static List<object> _markedForUpdateObjects;

        // connection string for auto create database connection
        public static string ConnectionString;
        // database connection for transaction
        [ThreadStatic]
        public static DbConnection DbConnection;

        public static TransactionScope GetTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return GetTransactionScope(TransactionScopeOption.Required,
                new TransactionOptions {Timeout = TimeSpan.MaxValue, IsolationLevel = isolationLevel});
        }

        public static TransactionScope GetTransactionScope(TransactionScopeOption transactonScopeOption, TransactionOptions transactionOptions)
        {
            if (transactionOptions.IsolationLevel != IsolationLevel.ReadUncommitted &&
                         transactionOptions.IsolationLevel != IsolationLevel.ReadCommitted &&
                         transactionOptions.IsolationLevel != IsolationLevel.Serializable)
                throw new TransactionException("Unsupported Isolation Level");
            Transactional.Timeout = new TimeSpan(0, 5, 0);
            if (_markedForUpdateObjects == null)
                _markedForUpdateObjects = new List<object>();
            else
                _markedForUpdateObjects.Clear();
            return new TransactionScope(transactonScopeOption, transactionOptions);
        }

        public static DbConnection CreateDbConnection(string connectionStr = null)
        {
            if (string.IsNullOrEmpty(connectionStr)) connectionStr = ConnectionString;
            if (string.IsNullOrEmpty(connectionStr)) return null;
            try
            {
                return (DbConnection = new SqlConnection(connectionStr));
            }
            catch (Exception)
            {
                return (DbConnection = null);
            }
        }

        public static void UpdateAll()
        {
            foreach (var obj in _markedForUpdateObjects)
            {
                CollectionRepository.GetCollection(obj.GetType()).UpdateEntity(obj);
            }
            _markedForUpdateObjects.Clear();
            DbConnection = null;
        }

        internal static void MarkForUpdate(object target)
        {
            if (_markedForUpdateObjects == null) _markedForUpdateObjects = new List<object>();
            if (!_markedForUpdateObjects.Contains(target)) _markedForUpdateObjects.Add(target);
        }

        internal static void UnmarkForUpdate(object target)
        {
            if (_markedForUpdateObjects == null) return;
            if (_markedForUpdateObjects.Contains(target)) _markedForUpdateObjects.Remove(target);
        }
    }
}