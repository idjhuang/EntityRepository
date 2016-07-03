using System.Collections.Generic;

namespace ObjectRepositoryContract
{
    public class CollectionRepository
    {
        private static Dictionary<string, ICollection> Collections { get; set; }
        private static readonly System.Collections.Generic.List<IReference> _references = new System.Collections.Generic.List<IReference>();

        public static void SetCollections(Dictionary<string, ICollection> collections)
        {
            Collections = collections;
        }

        public static ICollection GetCollection(string type)
        {
            // return default collection implementation when type not found in collection table
            return Collections.ContainsKey(type) ? Collections[type] : Collections[""];
        }

        public static void RegisterReference(IReference reference)
        {
            _references.Add(reference);
        }

        public static void SetReferences()
        {
            foreach (var reference in _references)
            {
                reference.SetReference();
            }
            _references.Clear();
        }
    }
}