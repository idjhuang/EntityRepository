using System;
using System.Configuration;

namespace ObjectRepositoryContract
{
    [Serializable]
    public class Object : IObject
    {
        public Object(object id)
        {
            Id = id;
        }

        public object Id { get; }
    }
}