using System;
using Newtonsoft.Json;

namespace EntityRepository
{
    [Serializable]
    [JsonConverter(typeof(TransactionalEntityReferenceConverter))]
    public class TransactionalEntityReference<T> : IReference where T : Entity
    {
        private TransactionalEntity<T> _reference;
        internal object Target;
        internal bool Loaded;

        public TransactionalEntityReference(TransactionalEntity<T> reference = null)
        {
            _reference = reference;
            Loaded = true;
        }

        public TransactionalEntityReference(object target)
        {
            Target = target;
            Loaded = false;
        }

        public TransactionalEntity<T> Reference
        {
            get { if (!Loaded) SetReference(); return _reference; }
            set { _reference = value; }
        }

        public void SetReference()
        {
            _reference =
                CollectionRepository.GetCollection(typeof(T)).GetEntity(Target) as
                    TransactionalEntity<T>;
            Target = null;
            Loaded = true;
        }
    }
}