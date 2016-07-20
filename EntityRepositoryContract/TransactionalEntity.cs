using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using ObjectResourceManager;

namespace EntityRepositoryContract
{
    [Serializable]
    public class TransactionalEntity<T> : Transactional<T>, ISerializable where T : Entity
    {
        private readonly object _id;
        public TransactionalEntity(T value): base(value)
        {
            _id = value.Id;
        }

        public TransactionalEntity(SerializationInfo info, StreamingContext context)
        {
            _id = info.GetValue("Id", typeof (object));
            Value = (T) info.GetValue("Value", typeof (T));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", _id, typeof (object));
            info.AddValue("Value", Value, typeof(T));
        }

        protected override void SetValue(T t)
        {
            // check value's id
            if (!_id.Equals(t.Id)) throw new ArgumentException("object's id mismatch.");
            // track updated bojects
            TransactionScopeUtil.MarkForUpdate(this);
            base.SetValue(t);
        }

        public override T GetValue(LockMode lockMode = LockMode.Normal)
        {
            // return a clone of value to prevent indirect update to object
            var value = base.GetValue(lockMode);
            Debug.Assert(value != null);
            var retVal = ResourceManager.Clone(value);
            Debug.Assert(retVal != null);
            return retVal;
        }

        public void Update(T t)
        {
            // check value's id
            if (!_id.Equals(t.Id)) throw new ArgumentException("object's id mismatch.");
            base.SetValue(t);
            Update();
        }

        public void Update()
        {
            CollectionRepository.GetCollection(typeof(T)).UpdateEntity(this);
            TransactionScopeUtil.UnmarkForUpdate(this);
        }
    }

    public static class TransactionalEntity
    {
        public static object CreateTransactionalEntity(Type entityType, object value)
        {
            if (value.GetType() != entityType) throw new ArgumentException("Value's type mismatch.");
            var type = typeof(TransactionalEntity<>).MakeGenericType(entityType);
            var args = new[] { value };
            var taretObj = Activator.CreateInstance(type, args);
            return taretObj;
        }
    }
}