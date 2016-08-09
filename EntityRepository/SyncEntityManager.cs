using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EntityRepository
{
    public static class SyncEntityManager
    {
        private static readonly Queue<SyncEntityRequest> SyncEntityRequestQueue = new Queue<SyncEntityRequest>();
        private static readonly List<SyncEntityRequest> IgnoreSyncEntityRequestList = new List<SyncEntityRequest>();
        private static readonly object ProcessLock = new Object();
        [ThreadStatic] private static Timer _processSyncEntityRequestTimer;

        public static void AddIgnoreSyncRequest(SyncEntityRequest ignore)
        {
            ignore.Timestamp = DateTime.Now;
            IgnoreSyncEntityRequestList.Add(ignore);
        }

        public static void AddSyncEntityRequest(SyncEntityRequest request, int delay = 100)
        {
            // put request into queue
            request.Timestamp = DateTime.Now;
            SyncEntityRequestQueue.Enqueue(request);
            _processSyncEntityRequestTimer = new Timer(ProcessSyncRequestQueue, null, delay, Timeout.Infinite);
        }

        public static void ProcessSyncRequestQueue(object state = null)
        {
            lock (ProcessLock)
            {
                // process sync request queue
                while (SyncEntityRequestQueue.Count > 0)
                {
                    var request = SyncEntityRequestQueue.Dequeue();
                    var ignore =
                        IgnoreSyncEntityRequestList.FirstOrDefault(
                            r =>
                                r.Type.Equals(request.Type) && r.Delete == request.Delete &&
                                !r.Id.Except(request.Id).Any() && (request.Timestamp - r.Timestamp).TotalMilliseconds < 1000);
                    if (ignore != null)
                    {
                        IgnoreSyncEntityRequestList.Remove(ignore);
                    }
                    else
                    {
                        var collection = CollectionRepository.GetCollection(TypeRepository.GetType(request.Type)) as ICachedCollection;
                        if (request.Id == null)
                            collection?.SyncAll();
                        else
                            collection?.Sync(request.Id, request.Delete);
                    }
                }
            }
        }

        public static void ReSyncCollection(string type)
        {
            var collection = CollectionRepository.GetCollection(TypeRepository.GetType(type)) as ICachedCollection;
            collection?.SyncAll();
        }
    }
}