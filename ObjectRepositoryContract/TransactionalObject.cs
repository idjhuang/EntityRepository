using System;
using System.Runtime.Serialization;
using System.Transactions;
using ObjectResourceManager;

namespace ObjectRepositoryContract
{
    public class TransactionalObject<T> : Transactional<T>, ISerializable where T:ObjectValue
    {
        public static object CreateTransactionalObject(Type type, ObjectValue value)
        {
            var targetType = typeof (TransactionalObject<>).MakeGenericType(type);
            var args = new object[] {value};
            return Activator.CreateInstance(targetType, args);
        } 

        public TransactionalObject(T value): base(value) { }

        public TransactionalObject(ObjectValue value): base(value as T) { } 

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
            CollectionRepository.GetCollection(Value.Type).UpdateObject(this);
            base.Prepare(preparingEnlistment);
        }
    }
}