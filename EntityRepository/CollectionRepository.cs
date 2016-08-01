using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityRepository
{
    public static class CollectionRepository
    {
        private static readonly Dictionary<Type, ICollection> Collections = new Dictionary<Type, ICollection>();
        private static readonly List<IReference> References = new List<IReference>();

        public static Dictionary<Type, DateTime> LastUpdateTime { get; }    // track update time of each entity type

        static CollectionRepository()
        {
            LastUpdateTime = new Dictionary<Type, DateTime>();
        }

        public static void Init()
        {
            // explict init the CollectionRepository
            // init type repository
            TypeRepository.Init();
            // load all entity modules
            TypeRepository.LoadModules(typeof(Entity));
            // load all collection modules
            var collectionTypes = TypeRepository.LoadModules(typeof(ICollection));
            foreach (var collection in from collectionType in collectionTypes where collectionType.IsClass select Activator.CreateInstance(collectionType))
            {
                AddCollection(collection as ICollection);
            }
        }

        public static void AddCollection(ICollection collection)
        {
            foreach (var type in collection.SupportedTypes().Where(type => !Collections.ContainsKey(type)))
            {
                Collections.Add(type, collection);
                LastUpdateTime.Add(type, DateTime.MinValue);
            }
        }

        public static ICollection GetCollection(Type type)
        {
            // return default collection implementation when type not found in collection table
            return Collections.ContainsKey(type) ? Collections[type] : Collections[typeof(object)];
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