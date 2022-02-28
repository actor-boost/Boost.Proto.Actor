using Google.Protobuf;
using Proto.Remote;
using Microsoft.Toolkit.HighPerformance;
using MessagePack;

namespace Boost.Proto.Actor.MessagePackSerializer
{
    public record MessagePackSerializer(MessagePackSerializerOptions Options) : ISerializer
    {
        public bool CanSerialize(object obj) => true;
        public object Deserialize(ByteString bytes, string typeName) =>
            MessagePack.MessagePackSerializer
                       .Typeless
                       .Deserialize(bytes.Memory.AsStream(), Options);
        public string GetTypeName(object message) => string.Empty;
        public ByteString Serialize(object obj) => 
            ByteString.FromStream(MessagePack.MessagePackSerializer
                                             .Typeless
                                             .Serialize(obj, Options)
                                             .AsMemory()
                                             .AsStream());
    }
}
