//Questions? Comments? go to 
//http://www.idesign.net

using System;
using System.Threading;
using System.Collections.Generic;
using System.Transactions;
using System.Diagnostics;

namespace ObjectResourceManager
{
    /// <summary>
    /// Protects a resource manager by proving exclusive transaction isolation (Serializable level only)
    /// </summary>
    public class TransactionalLock
    {
        //Transaction that tried to acquire the lock while the lock is owned by another transaction are placed in this queue
        LinkedList<KeyValuePair<Transaction,ManualResetEvent>> m_PendingTransactions = new LinkedList<KeyValuePair<Transaction,ManualResetEvent>>();
        Transaction _owningTransaction;

        Transaction OwningTransaction
        {
            get
            {
                lock (this)
                {
                    return _owningTransaction;
                }
            }
            set
            {
                lock (this)
                {
                    _owningTransaction = value;
                }
            }
        }

        public bool Locked
        {
            get
            {
                return OwningTransaction != null;
            }
        }

        /// <summary>
        /// Acquires the lock for the exclusive use of a transaction. If another transaction owns the lock, it blocks the calling transaction and places it in a queue. If the transaction owns the lock already Lock() does nothing. 
        /// </summary>
        public void Lock()
        {
            Lock(Transaction.Current);
        }

        void Lock(Transaction transaction)
        {
            if (OwningTransaction == null)
            {
                if (transaction == null)
                {
                    return;
                }
                else
                {
                    Debug.Assert(transaction.IsolationLevel == IsolationLevel.Serializable ||
                                 transaction.IsolationLevel == IsolationLevel.ReadCommitted ||
                                 transaction.IsolationLevel == IsolationLevel.ReadUncommitted);

                    //Acquire the lock
                    OwningTransaction = transaction;
                    return;
                }
            }
            else //Some transaction owns the lock
            {
                //Is it the same one?
                if (OwningTransaction == transaction)
                {
                    return;
                }
                else //Need to lock
                {
                    ManualResetEvent manualEvent = new ManualResetEvent(false);

                    KeyValuePair<Transaction, ManualResetEvent> pair;
                    pair = new KeyValuePair<Transaction, ManualResetEvent>(transaction, manualEvent);
                    lock (this)
                    {
                        m_PendingTransactions.AddLast(pair);
                    }
                    if (transaction != null)
                    {
                        Debug.Assert(transaction.TransactionInformation.Status == TransactionStatus.Active);
                        //Since the transaction can abort or just time out while blocking, unblock it when it is completed and remove from the queue
                        transaction.TransactionCompleted += delegate
                        {
                            lock (this)
                            {
                                //Note that the pair may have already been removed if unlocked
                                m_PendingTransactions.Remove(pair);
                            }
                            lock (manualEvent)//To deal with race condition of the handle closed between the check and the set
                            {
                                if (manualEvent.SafeWaitHandle.IsClosed == false)
                                {
                                    manualEvent.Set();
                                }
                            }
                        };
                    }
                    //Block the transaction or the calling thread
                    manualEvent.WaitOne();
                    lock (manualEvent)//To deal with race condition of the other threads setting the handle
                    {
                        manualEvent.Close();
                    }
                }
            }
        }

        //Releases the transaction lock and allows the next pending transaction to quire it. 
        public void Unlock()
        {
            Debug.Assert(Locked);

            OwningTransaction = null;
            LinkedListNode<KeyValuePair<Transaction, ManualResetEvent>> node = null;

            lock (this)
            {
                if (m_PendingTransactions.Count > 0)
                {
                    node = m_PendingTransactions.First;
                    m_PendingTransactions.RemoveFirst();
                }
            }
            if (node != null)
            {
                Transaction transaction = node.Value.Key;
                ManualResetEvent manualEvent = node.Value.Value;
                Lock(transaction);
                lock (manualEvent)//To deal with race condition of the handle closed between the check and the set
                {
                    if (manualEvent.SafeWaitHandle.IsClosed == false)
                    {
                        manualEvent.Set();
                    }
                }
            }
        }
    }
}
