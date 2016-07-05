using System;
using System.Transactions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ObjectResourceManager
{
    public class Transactional<T> : IEnlistmentNotification
    {
        private T _value;
        private T _temporaryValue;
        private readonly List<Transaction> _transactionList = new List<Transaction>();
        private ReaderWriterLockSlim ReaderWriterLock { get; }
        protected bool IsDirty;

        public Transactional(T value)
        {
            ReaderWriterLock = new ReaderWriterLockSlim();
            _value = value;
            IsDirty = false;
        }

        public Transactional(Transactional<T> transactional) : this(transactional.Value)
        { }

        public Transactional() : this(default(T))
        { }

        static Transactional()
        {
            ResourceManager.ConstrainType(typeof(T));
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            IDisposable disposable = _value as IDisposable;
            disposable?.Dispose();
            _value = _temporaryValue;
            _temporaryValue = default(T);
            RemoveTransaction();
            Unlock();
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            RemoveTransaction();
            Unlock();
            enlistment.Done();
        }

        public virtual void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            IDisposable disposable = _temporaryValue as IDisposable;
            disposable?.Dispose();
            _temporaryValue = default(T);
            RemoveTransaction();
            Unlock();
            enlistment.Done();
        }

        private void Enlist(T t)
        {
            var currentTransaction = Transaction.Current;
            // if already join in the transaction, then exit
            if (_transactionList.Contains(currentTransaction)) return;
            _transactionList.Add(currentTransaction);
            Debug.Assert(currentTransaction.TransactionInformation.Status == TransactionStatus.Active);
            currentTransaction.EnlistVolatile(this, EnlistmentOptions.None);
            _temporaryValue = ResourceManager.Clone(t);
        }

        private void RemoveTransaction()
        {
            var toBeRemoved =
                _transactionList.Where(
                    transaction => transaction.TransactionInformation.Status != TransactionStatus.Active).ToList();
            foreach (var transaction in toBeRemoved)
            {
                _transactionList.Remove(transaction);
            }
        }

        private void Unlock()
        {
            Debug.Assert(ReaderWriterLock.IsReadLockHeld || ReaderWriterLock.IsWriteLockHeld);
            if (ReaderWriterLock.IsReadLockHeld) ReaderWriterLock.ExitReadLock();
            if (ReaderWriterLock.IsWriteLockHeld) ReaderWriterLock.ExitWriteLock();
        }

        private void Unlock(bool writeLock)
        {
            if (writeLock && ReaderWriterLock.IsWriteLockHeld) ReaderWriterLock.ExitWriteLock();
            if (!writeLock && ReaderWriterLock.IsReadLockHeld) ReaderWriterLock.ExitReadLock();
        }

        private void Lock(bool writeLock)
        {
            if (writeLock)
            {
                if (ReaderWriterLock.IsReadLockHeld) ReaderWriterLock.ExitReadLock();
                if (ReaderWriterLock.IsWriteLockHeld) return;
                if (!ReaderWriterLock.TryEnterWriteLock(TransactionScopeUtil.Timeout))
                    throw new TimeoutException("Acquire write lock timeout.");
            }
            else
            {
                if (ReaderWriterLock.IsWriteLockHeld) return;
                if (ReaderWriterLock.IsReadLockHeld) return;
                if (!ReaderWriterLock.TryEnterReadLock(TransactionScopeUtil.Timeout))
                    throw new TimeoutException("Acquire read lock timeout.");
            }
        }

        private void SetValue(T t)
        {
            Lock(true);
            IsDirty = true;
            if (Transaction.Current == null)
            {
                _value = t;
                Unlock(true);
            }
            else
            {
                Enlist(t);
                _temporaryValue = t;
            }
        }

        public T GetValue(LockMode lockMode = LockMode.Normal)
        {
            if (Transaction.Current == null)
            {
                switch (lockMode)
                {
                    case LockMode.NoLock:
                        return _temporaryValue;
                    case LockMode.Normal:
                        Lock(false);
                        Unlock(false);
                        return _value;
                    case LockMode.Exclusive:
                        Lock(true);
                        Unlock(true);
                        return _value;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if ((Transaction.Current.IsolationLevel == IsolationLevel.ReadUncommitted && lockMode != LockMode.Exclusive) ||
                (lockMode == LockMode.NoLock)) return _temporaryValue;
            Lock(Transaction.Current.IsolationLevel == IsolationLevel.Serializable || lockMode == LockMode.Exclusive);
            Enlist(_value);
            return _temporaryValue;
        }

        public T Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        public static implicit operator T(Transactional<T> transactional)
        {
            return transactional.Value;
        }

        public static bool operator ==(Transactional<T> t1, Transactional<T> t2)
        {
            // Is t1 and t2 null (check the value as well).
            bool t1Null = (ReferenceEquals(t1, null) || t1.Value == null);
            bool t2Null = (ReferenceEquals(t2, null) || t2.Value == null);

            // If they are both null, return true.
            if (t1Null && t2Null)
            {
                return true;
            }

            // If one is null, return false.
            if (t1Null || t2Null)
            {
                return false;
            }
            return EqualityComparer<T>.Default.Equals(t1.Value, t2.Value);
        }

        public static bool operator ==(Transactional<T> t1, T t2)
        {
            // Is t1 and t2 null (check the value as well).
            bool t1Null = (ReferenceEquals(t1, null) || t1.Value == null);
            bool t2Null = t2 == null;

            // If they are both null, return true.
            if (t1Null && t2Null)
            {
                return true;
            }

            // If one is null, return false.
            if (t1Null || t2Null)
            {
                return false;
            }
            return EqualityComparer<T>.Default.Equals(t1.Value, t2);
        }

        public static bool operator ==(T t1, Transactional<T> t2)
        {
            // Is t1 and t2 null (check the value as well)
            bool t1Null = t1 == null;
            bool t2Null = (ReferenceEquals(t2, null) || t2.Value == null);

            // If they are both null, return true.
            if (t1Null && t2Null)
            {
                return true;
            }

            // If one is null, return false.
            if (t1Null || t2Null)
            {
                return false;
            }
            return EqualityComparer<T>.Default.Equals(t1, t2.Value);
        }

        public static bool operator !=(T t1, Transactional<T> t2)
        {
            return !(t1 == t2);
        }

        public static bool operator !=(Transactional<T> t1, T t2)
        {
            return !(t1 == t2);
        }

        public static bool operator !=(Transactional<T> t1, Transactional<T> t2)
        {
            return !(t1 == t2);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }
    }
}