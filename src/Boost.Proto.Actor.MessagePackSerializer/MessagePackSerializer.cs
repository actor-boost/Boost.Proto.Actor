using Google.Protobuf;
using Proto.Remote;
using System.Runtime.InteropServices;

namespace Boost.Proto.Actor.MessagePackSerializer
{
    public record MessagePackSerializer() : ISerializer
    {
        public static object Typeless { get; internal set; }

        public bool CanSerialize(object obj) => true;
        public object Deserialize(ByteString bytes, string typeName)
        {
            return MessagePack.MessagePackSerializer
                              .Typeless
                              .Deserialize(MemoryMarshal.AsMemory(bytes.Memory));
        }
        public string GetTypeName(object message) => string.Empty;
        public ByteString Serialize(object obj)
        {
            using var ms = new MemoryStream();   
            MessagePack.MessagePackSerializer
                       .Typeless
                       .Serialize(ms, obj);
            ms.Position = 0;
            return ByteString.FromStream(ms);
        }
    }
}
