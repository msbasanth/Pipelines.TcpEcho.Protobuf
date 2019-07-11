using ProtoBuf;

namespace Common
{
    [ProtoContract]
    public class Person
    {
        [ProtoMember(1)]
        public string Name { get; set; }
    }
}
