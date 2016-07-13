﻿using System;
using System.Configuration;

namespace ObjectRepositoryContract
{
    [Serializable]
    public class ObjectBase : IObject
    {
        public ObjectBase(string type, object id, bool loaded = true)
        {
            Type = type;
            Id = id;
            Loaded = loaded;
        }

        public string Type { get; }
        public object Id { get; }
        public bool Loaded { get; }
    }
}