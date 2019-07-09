using ProtoBuf;

namespace Common
{
    class ProtoContracts
    {
        [ProtoContract]
        public class Person
        {
            [ProtoMember(1)]
            public string Name { get; set; }
        }
    }
}
