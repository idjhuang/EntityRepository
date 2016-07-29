using System;
using Newtonsoft.Json;

namespace Workflow
{
    public class TaskExecutionRecordConvertor : JsonConverter
    {
        public class Package
        {
            public string Task { get; set; }
            public string ParamsType { get; set; }
            public string ParamJson { get; set; }
            public string CompensationDataType { get; set; }
            public string CompensationDataJson { get; set; }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var record = value as TaskExecutionRecord;
            var package = new Package
            {
                Task = record.Task,
                ParamsType = null,
                ParamJson = null,
                CompensationDataType = null,
                CompensationDataJson = null
            };
            if (record.Parameters != null)
            {
                package.ParamsType = record.Parameters.GetType().AssemblyQualifiedName;
                package.ParamJson = JsonConvert.SerializeObject(record.Parameters);
            }
            if (record.CompensationData != null)
            {
                package.CompensationDataType = record.CompensationData.GetType().AssemblyQualifiedName;
                package.CompensationDataJson = JsonConvert.SerializeObject(record.CompensationData);
            }
            serializer.Serialize(writer, package);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var package = serializer.Deserialize<Package>(reader);
            var record = new TaskExecutionRecord
            {
                Task = package.Task,
                Parameters = null,
                CompensationData = null
            };
            if (package.ParamsType != null)
            {
                record.Parameters = JsonConvert.DeserializeObject(package.ParamJson, Type.GetType(package.ParamsType));
            }
            if (package.CompensationDataType != null)
            {
                record.CompensationData = JsonConvert.DeserializeObject(package.CompensationDataJson,
                    Type.GetType(package.CompensationDataType));
            }
            return record;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TaskExecutionRecord);
        }
    }
}