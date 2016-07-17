using System;
using System.Runtime.Serialization;
using ObjectRepositoryContract;

namespace Test
{
    [Serializable]
    public class Drived : ObjectRepositoryContract.Object
    {
        public Drived(int id) : base(id)
        {
        }

        public int P1 { get; set; }
        public string P2 { get; set; }
    }

    [Serializable]
    public class C1 : ObjectRepositoryContract.Object
    {
        public C1(Guid id) : base(id) { }

        public int I1 { get; set; }
        public string S1 { get; set; }
    }

    [Serializable]
    public class C2 : ObjectRepositoryContract.Object
    {
        public C2(Guid id) : base(id) { }

        public ObjectReference<C1> C1Reference;
        public string S2 { get; set; }
    }

    [Serializable]
    public class C3 : ObjectRepositoryContract.Object
    {
        public C3(Guid id) : base(id) { }
        public ObjectList<C1> C1List;
    }
}