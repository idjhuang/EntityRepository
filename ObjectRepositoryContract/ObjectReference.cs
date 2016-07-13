using System.Runtime.Serialization;

namespace ObjectRepositoryContract
{
    public class ObjectReference<T> : ISerializable, IReference where T:ObjectValue
    {
        private TransactionalObject<T> _reference;
        private ObjectValue _target;
        private bool _loaded = true;

        public ObjectReference(TransactionalObject<T> reference = null)
        {
            _reference = reference;
        }

        public ObjectReference(SerializationInfo info, StreamingContext context)
        {
            _target = (ObjectValue)info.GetValue("Reference", typeof(ObjectValue));
            _loaded = false;
        }

        public TransactionalObject<T> Reference
        {
            get { if (!_loaded) SetReference(); return _reference; }
            set { _reference = value; }
        }

        public void SetReference()
        {
            _reference =
                CollectionRepository.GetCollection(_target.Type).GetObject(_target.Id) as
                    TransactionalObject<T>;
            _target = null;
            _loaded = true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var reference = new ObjectValue(_reference.Value.Type, _reference.Value.Id);
            info.AddValue("Reference", reference, typeof(ObjectValue));
        }
    }
}