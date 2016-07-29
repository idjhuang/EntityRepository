using Newtonsoft.Json.Linq;
using System;

namespace Workflow
{
    [Serializable]
    public class TaskExecutionRecord
    {
        public string Task { get; set; }
        public object Parameters { get; set; }
        public object CompensationData { get; set; }
    }
}
