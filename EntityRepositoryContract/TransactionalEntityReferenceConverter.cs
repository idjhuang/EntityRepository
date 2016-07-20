using System;
using Newtonsoft.Json;

namespace EntityRepositoryContract
{
    public class TransactionalEntityReferenceConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic entityReferene = value;
            var id = (entityReferene.Reference == null) ? null : entityReferene.Reference.GetValue().Id;
            serializer.Serialize(writer, id);
        }

        public override object ReadJson(JsonReader reader, Type referenceType, object existingValue, JsonSerializer serializer)
        {
            var target = serializer.Deserialize<object>(reader);
            dynamic entityReference = Activator.CreateInstance(referenceType, new object[] {null});
            entityReference.Target = target;
            entityReference.Loaded = false;
            return entityReference;
        }

        public override bool CanConvert(Type objectType)
        {
            var baseType = typeof(TransactionalEntityReference<Entity>);
            return baseType.IsAssignableFrom(objectType);
        }
    }
}