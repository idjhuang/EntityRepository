using System;

namespace EntityRepositoryContract
{
    [Serializable]
    public class Entity
    {
        public Entity(object id)
        {
            Id = id;
        }

        public object Id { get; private set; }
    }
}