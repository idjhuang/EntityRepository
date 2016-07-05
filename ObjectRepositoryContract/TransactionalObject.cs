using System.Diagnostics;
using System.Runtime.Serialization;
using System.Transactions;
using ObjectResourceManager;

namespace ObjectRepositoryContract
{
    public class TransactionalObject<T> : Transactional<T>, ISerializable where T:ObjectBase
    {
        public TransactionalObject(T value): base(value) { }

        public TransactionalObject(SerializationInfo info, StreamingContext context)
        {
            Value = (T)info.GetValue("Value", typeof(T));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value", Value, typeof(T));
        }

        public override void Prepare(PreparingEnlistment preparingEnlistment)
        {
            Debug.WriteLine("Prepared.");
            base.Prepare(preparingEnlistment);
        }
    }
}