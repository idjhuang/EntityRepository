using System.Runtime.Serialization;

namespace ObjectRepositoryContract
{
    public class ObjectReference<T> : ISerializable, IReference where T:Object
    {
        private TransactionalObject<T> _reference;
        private Object _target;
        private bool _loaded = true;

        public ObjectReference(TransactionalObject<T> reference = null)
        {
            _reference = reference;
        }

        public ObjectReference(SerializationInfo info, StreamingContext context)
        {
            _target = (Object)info.GetValue("Reference", typeof(Object));
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
                CollectionRepository.GetCollection(typeof(T)).GetObject(_target.Id) as
                    TransactionalObject<T>;
            _target = null;
            _loaded = true;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var reference = new Object(_reference.Value.Id);
            info.AddValue("Reference", reference, typeof(Object));
        }
    }
}