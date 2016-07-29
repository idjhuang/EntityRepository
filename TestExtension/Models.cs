using System;
using EntityRepository;

namespace TestExtension
{
    [Serializable]
    public class M1 : Entity
    {
        public M1(Guid id) : base(id) { }
        public string StrProp { get; set; }
        public int IntProp { get; set; }
    }
}
