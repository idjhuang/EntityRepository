using System;

namespace ObjectRepositoryContract
{
    [Serializable]
    public class ObjectBase : IObject
    {
        public ObjectBase(string type, object id)
        {
            Type = type;
            Id = id;
        }

        public string Type { get; }
        public object Id { get; }
    }
}