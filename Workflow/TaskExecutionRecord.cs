using Newtonsoft.Json.Linq;
using System;
using Newtonsoft.Json;

namespace Workflow
{
    [Serializable]
    [JsonConverter(typeof(TaskExecutionRecordConvertor))]
    public class TaskExecutionRecord
    {
        public string Task { get; set; }
        public object Parameters { get; set; }
        public object CompensationData { get; set; }
    }
}
