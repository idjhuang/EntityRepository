using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Workflow
{
    public class WorkflowContextConverter : JsonConverter
    {
        public class Package
        {
            public Guid Id { get; set; }
            public string User { get; set; }
            public string Name { get; set; }
            public List<string> StateKeys { get; set; }
            public List<string> StateValuesType { get; set; }
            public List<string> StateValuesJson { get; set; }
            public string Message { get; set; }
            public string NextTask { get; set; }
            public int Status { get; set; }
            public DateTime Deadline { get; set; }
            public List<TaskExecutionRecord> ExecutionRecords { get; set; } 
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var context = value as WorkflowContext;
            var package = new Package
            {
                Id = (Guid) context.Id,
                User = context.User,
                Name = context.Name,
                StateKeys = new List<string>(),
                StateValuesType = new List<string>(),
                StateValuesJson = new List<string>(),
                Message = context.Message,
                NextTask = context.NextTask,
                Status = (int) context.Status,
                Deadline = context.Deadline,
                ExecutionRecords = context.ExecutionRecords.ToList()
            };
            foreach (var keyValue in context.State)
            {
                package.StateKeys.Add(keyValue.Key);
                package.StateValuesType.Add(keyValue.Value.GetType().AssemblyQualifiedName);
                package.StateValuesJson.Add(JsonConvert.SerializeObject(keyValue.Value));
            }
            serializer.Serialize(writer, package);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var package = serializer.Deserialize<Package>(reader);
            var context = new WorkflowContext(package.Id, package.User, package.Name)
            {
                Message = package.Message,
                NextTask = package.NextTask,
                Status = (WorkflowStatus) package.Status,
                Deadline = package.Deadline
            };
            for (var i = 0; i < package.StateKeys.Count; i++)
            {
                var key = package.StateKeys[i];
                var json = package.StateValuesJson[i];
                var type = Type.GetType(package.StateValuesType[i]);
                context.State.Add(key,
                    JsonConvert.DeserializeObject(json, type));
            }
            package.ExecutionRecords.Reverse();
            foreach (var record in package.ExecutionRecords)
            {
                context.ExecutionRecords.Push(record);
            }
            return context;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (WorkflowContext);
        }
    }
}