using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EntityRepositoryContract
{
    public class TransactionalEntityListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic entityList = value;
            var targetList = new List<object>();
            foreach (dynamic o in entityList.List)
            {
                var id = (o == null) ? null : o.Value.Id;
                targetList.Add(id);
            }
            serializer.Serialize(writer, targetList);
        }

        public override object ReadJson(JsonReader reader, Type listType, object existingValue, JsonSerializer serializer)
        {
            var targetList = serializer.Deserialize<List<object>>(reader);
            dynamic entityList = Activator.CreateInstance(listType);
            entityList.TargetList = targetList;
            entityList.Loaded = false;
            return entityList;
        }

        public override bool CanConvert(Type objectType)
        {
            var baseType = typeof(TransactionalEntityList<Entity>);
            return baseType.IsAssignableFrom(objectType);
        }
    }
}