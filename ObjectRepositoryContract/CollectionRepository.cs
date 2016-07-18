using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ObjectRepositoryContract
{
    public static class CollectionRepository
    {
        private static readonly Dictionary<Type, ICollection> Collections = new Dictionary<Type, ICollection>();
        private static readonly List<IReference> References = new List<IReference>();

        static CollectionRepository()
        {
        }

        public static void Init()
        {
            // explict init the CollectionRepository
            // init type repository
            TypeRepository.Init();
            // load all entity modules
            TypeRepository.LoadModules(typeof(Object));
            // load all collection modules
            var collectionTypes = TypeRepository.LoadModules(typeof(ICollection));
            foreach (var collection in from collectionType in collectionTypes where collectionType.IsClass select Activator.CreateInstance(collectionType))
            {
                AddCollection(collection as ICollection);
            }
        }

        public static void AddCollection(ICollection collection)
        {
            foreach (var type in collection.SupportedTypes())
            {
                Collections.Add(type, collection);
            }
        }

        public static ICollection GetCollection(Type type)
        {
            // return default collection implementation when type not found in collection table
            return Collections.ContainsKey(type) ? Collections[type] : Collections[typeof(Object)];
        }

        public static void RegisterReference(IReference reference)
        {
            References.Add(reference);
        }

        public static void SetReferences()
        {
            foreach (var reference in References)
            {
                reference.SetReference();
            }
            References.Clear();
        }
    }
}