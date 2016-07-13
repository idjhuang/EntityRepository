using System;
using System.Configuration;

namespace ObjectRepositoryContract
{
    [Serializable]
    public class ObjectValue : IObject
    {
        public ObjectValue(string type, object id)
        {
            Type = type;
            Id = id;
        }

        public string Type { get; }
        public object Id { get; }
    }
}