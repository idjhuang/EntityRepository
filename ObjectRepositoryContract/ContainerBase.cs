using System.Runtime.Serialization;

namespace ObjectRepositoryContract
{
    public class ContainerBase: ObjectValue, IReference
    {
        [OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            // replace all references to simple object with only type and id
            object obj = this;
            var properties = obj.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.PropertyType.IsSubclassOf(typeof (IObject))) continue;
                var target = (IObject)propertyInfo.GetValue(this);
                propertyInfo.SetValue(this, new ObjectValue(target.Type, target.Id));
            }
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // register to CollectionRepository for later process
            CollectionRepository.RegisterReference(this);
        }

        public ContainerBase(string type, object id) : base(type, id)
        {
        }

        public void SetReference()
        {
            // replase all references with actual object reference
            object obj = this;
            var properties = obj.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.PropertyType.IsSubclassOf(typeof(IObject))) continue;
                var target = (IObject)propertyInfo.GetValue(this);
                propertyInfo.SetValue(this, CollectionRepository.GetCollection(target.Type).GetObject(target.Id));
            }
        }
    }
}