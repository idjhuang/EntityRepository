using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectRepositoryContract
{
    public class ObjectListConveter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic objectList = value;
            var targetList = new List<ObjectValue>();
            foreach (dynamic o in objectList.List)
            {
                targetList.Add(new ObjectValue(o.Value.Type, o.Value.Id));
            }
            serializer.Serialize(writer, targetList);
        }

        public override bool CanConvert(Type objectType)
        {
            var baseType = typeof (ObjectList<ObjectValue>);
            return baseType.IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var targetList = serializer.Deserialize<List<ObjectValue>>(reader);
            dynamic objList = Activator.CreateInstance(objectType);
            objList.TargetList = targetList;
            objList.Loaded = false;
            return objList;
        }
    }
}