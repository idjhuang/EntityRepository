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
            public Dictionary<string, object> State { get; set; } 
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
                State = context.State,
                Message = context.Message,
                NextTask = context.NextTask,
                Status = (int) context.Status,
                Deadline = context.Deadline,
                ExecutionRecords = context.ExecutionRecords.ToList()
            };
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
            foreach (var key in package.State.Keys)
            {
                context.State.Add(key, package.State[key]);
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