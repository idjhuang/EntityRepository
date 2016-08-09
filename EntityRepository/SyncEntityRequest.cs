using System;
using System.Collections.Generic;

namespace EntityRepository
{
    public class SyncEntityRequest
    {
        public string Type { get; set; }
        public bool Delete { get; set; }
        public List<string> Id { get; set; }
        public DateTime Timestamp { get; set; }
    }
}