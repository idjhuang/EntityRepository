using System;
using System.Data.Common;
using System.Data.Odbc;
using System.Transactions;

namespace ObjectResourceManager
{
    public static class TransactionScopeUtil
    {
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
            return new TransactionScope(transactonScopeOption, transactionOptions);
        }

        public static DbConnection CreateDbConnection(string connectionStr = null)
        {
            if (string.IsNullOrEmpty(connectionStr)) connectionStr = ConnectionString;
            if (string.IsNullOrEmpty(connectionStr)) return null;
            try
            {
                return (DbConnection = new OdbcConnection(connectionStr));
            }
            catch (Exception)
            {
                return (DbConnection = null);
            }
        }

        // timeout for lock and sql command execution
        public static TimeSpan Timeout = new TimeSpan(0, 5, 0);
        // connection string for auto create database connection
        public static string ConnectionString;
        // database connection for transaction
        [ThreadStatic] public static DbConnection DbConnection;
    }
}