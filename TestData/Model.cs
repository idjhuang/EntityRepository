using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestData
{
    [Serializable]
    public class M1 : ObjectRepositoryContract.Object
    {
        public M1(Guid id) : base(id) {}
        public string M1S1 { get; set; }
        public int M1I1 { get; set; }
    }
}
