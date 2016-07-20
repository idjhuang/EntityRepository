using System;
using EntityRepositoryContract;

namespace Test
{
    [Serializable]
    public class Simple : Entity
    {
        public Simple(Guid id) : base(id) { }
        public int IntProperty { get; set; }
        public string StrProperty { get; set; }
    }

    [Serializable]
    public class Container : Entity
    {
        public Container(Guid id) : base(id) { }
        public TransactionalEntityReference<Simple> SimpleReference;
        public string StrProperty { get; set; }
    }

    [Serializable]
    public class ObjList : Entity
    {
        public ObjList(Guid id) : base(id) { }
        public TransactionalEntityList<Simple> SimpleList;
        public int IntProperty { get; set; }
    }
}
