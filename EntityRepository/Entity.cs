using System;

namespace EntityRepository
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