using System;
using System.Runtime.Serialization;
using ObjectResourceManager;

namespace EntityRepository
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
            SetValue((T) info.GetValue("Value", typeof (T)));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", _id, typeof (object));
            info.AddValue("Value", GetValue(), typeof(T));
        }

        internal T GetValue()
        {
            return base.GetValue();
        }

        public void SetEntity(T t)
        {
            // check value's id
            if (!_id.Equals(t.Id)) throw new ArgumentException("object's id mismatch.");
            // track updated bojects
            TransactionScopeUtil.MarkForUpdate(this);
            SetValue(t);
        }

        public T GetEntity(LockMode lockMode = LockMode.Normal)
        {
            // return a clone of value to prevent indirect update to object
            var value = base.GetValue(lockMode);
            return ResourceManager.Clone(value);
        }

        public void Update(T t)
        {
            // check value's id
            if (!_id.Equals(t.Id)) throw new ArgumentException("object's id mismatch.");
            SetValue(t);
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