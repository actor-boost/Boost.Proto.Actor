using Google.Protobuf;
using Proto.Remote;
using Microsoft.Toolkit.HighPerformance;
using MessagePack;

namespace Boost.Proto.Actor.MessagePackSerializer
{
    public record MessagePackSerializer(MessagePackSerializerOptions Options) : ISerializer
    {
        public bool CanSerialize(object obj) => true;
        public object Deserialize(ByteString bytes, string typeName)
        {
            using var stream = bytes.Memory.AsStream();
            return MessagePack.MessagePackSerializer
                              .Typeless
                              .Deserialize(stream, Options);
        }
        public string GetTypeName(object message) => string.Empty;
        public ByteString Serialize(object obj)
        {
            using var stream = MessagePack.MessagePackSerializer
                                             .Typeless
                                             .Serialize(obj, Options)
                                             .AsMemory()
                                             .AsStream();
            return ByteString.FromStream(stream);
        }
    }
}
