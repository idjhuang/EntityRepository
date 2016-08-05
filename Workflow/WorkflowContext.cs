using EntityRepository;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Workflow
{
    public enum WorkflowStatus
    {
        Ready,
        Executing,
        Failed,
        Abort,
        Done
    }

    [Serializable]
    [JsonConverter(typeof(WorkflowContextConverter))]
    public class WorkflowContext : Entity
    {
        public WorkflowContext(Guid id, string user, string name, string abortTask = null) : base(id)
        {
            User = user;
            Name = name;
            AbortTask = abortTask;
            State = new Dictionary<string, object>();
            Status = WorkflowStatus.Ready;
            Deadline = DateTime.Now.Add(new TimeSpan(0, 10, 0)); // default 10 mins. timeout
            ExecutionRecords = new Stack<TaskExecutionRecord>();
        }

        public string User { get; private set; }
        public string Name { get; private set; }
        public string AbortTask { get; private set; }
        public Dictionary<string, object> State { get; private set; }
        public string Message { get; set; }
        public string NextTask { get; set; }
        public WorkflowStatus Status { get; set; }
        public DateTime Deadline { get; set; }
        public Stack<TaskExecutionRecord> ExecutionRecords { get; }
    }
}
