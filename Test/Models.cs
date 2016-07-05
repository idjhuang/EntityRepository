using System;
using System.Runtime.Serialization;
using ObjectRepositoryContract;

namespace Test
{
    [Serializable]
    public class Drived : ObjectBase
    {
        public Drived(string type, int id) : base(type, id)
        {
        }

        public int P1 { get; set; }
        public string P2 { get; set; }
    }

    public class C1 : ISerializable
    {
        public C1(int i1)
        {
            I1 = i1;
        }

        public C1(SerializationInfo info, StreamingContext context)
        {
            S1 = (string)info.GetValue("S1", typeof(string));
        }

        public int I1 { get; set; }
        public string S1 { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("S1", S1, typeof(string));
        }
    }
}