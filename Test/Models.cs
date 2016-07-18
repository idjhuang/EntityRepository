using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectRepositoryContract;

namespace Test
{
    [Serializable]
    public class Simple : ObjectRepositoryContract.Object
    {
        public Simple(Guid id) : base(id) { }

        public int IntProperty { get; set; }
        public string StrProperty { get; set; }
    }

    [Serializable]
    public class Container : ObjectRepositoryContract.Object
    {
        public Container(Guid id) : base(id) { }

        public ObjectReference<Simple> SimpleReference;
        public string StrProperty { get; set; }
    }

    [Serializable]
    public class ObjList : ObjectRepositoryContract.Object
    {
        public ObjList(Guid id) : base(id) { }
        public ObjectList<Simple> SimpleList;
        public int IntProperty { get; set; }
    }
}
