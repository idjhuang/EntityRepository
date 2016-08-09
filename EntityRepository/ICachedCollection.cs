using System.Collections.Generic;

namespace EntityRepository
{
    internal interface ICachedCollection
    {
        void SyncAll();
        void Sync(List<string> idList, bool delete);
    }
}