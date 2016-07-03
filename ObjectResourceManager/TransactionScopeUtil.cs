using System;
using System.Transactions;

namespace ObjectResourceManager
{
    public class TransactionScopeUtil
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

        // timeout for lock and sql command execution
        public static TimeSpan Timeout { get; set; } = new TimeSpan(0, 5, 0);
    }
}