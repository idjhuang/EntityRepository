using System;
using System.Runtime.Serialization;
using System.Transactions;
using ObjectResourceManager;

namespace ObjectRepositoryContract
{
    public class TransactionalObject<T> : Transactional<T>, ISerializable where T:Object
    {
        public TransactionalObject(T value): base(value) { }

        public TransactionalObject(Object value): base(value as T) { } 

        public TransactionalObject(SerializationInfo info, StreamingContext context)
        {
            Value = info.GetValue("Value", typeof(T)) as T;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value", Value, typeof(T));
        }

        public override void Prepare(PreparingEnlistment preparingEnlistment)
        {
            CollectionRepository.GetCollection(typeof(T)).UpdateObject(this);
            base.Prepare(preparingEnlistment);
        }
    }

    public static class TransactionalObject
    {
        public static object CreateTransactionalObject(Type type, Object value)
        {
            var targetType = typeof(TransactionalObject<>).MakeGenericType(type);
            var args = new object[] { value };
            var taretObj = Activator.CreateInstance(targetType, args);
            return taretObj;
        }
    }
}