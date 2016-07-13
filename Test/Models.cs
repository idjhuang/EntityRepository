using System;
using System.Runtime.Serialization;
using ObjectRepositoryContract;

namespace Test
{
    [Serializable]
    public class Drived : ObjectValue
    {
        public Drived(string type, int id) : base(type, id)
        {
        }

        public int P1 { get; set; }
        public string P2 { get; set; }
    }

    [Serializable]
    public class C1 : ObjectValue
    {
        public C1(string type, Guid id) : base(type, id) { }

        public int I1 { get; set; }
        public string S1 { get; set; }
    }

    [Serializable]
    public class C2 : ObjectValue
    {
        public C2(string type, object id) : base(type, id) { }

        public ObjectReference<C1> C1Reference;
        public string S2 { get; set; }
    }

    [Serializable]
    public class C3 : ObjectValue
    {
        public C3(string type, object id) : base(type, id) { }
        public ObjectList<C1> C1List;
    }
}