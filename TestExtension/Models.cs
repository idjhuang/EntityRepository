using System;

namespace TestExtension
{
    [Serializable]
    public class M1 : ObjectRepositoryContract.Object
    {
        public M1(Guid id) : base(id) {}
        public string StrProp { get; set; }
        public int IntProp { get; set; }
    }
}
