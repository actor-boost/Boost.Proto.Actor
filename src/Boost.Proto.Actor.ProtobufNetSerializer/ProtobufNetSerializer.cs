using System.Reflection;
using Google.Protobuf;
using Proto.Remote;
using ProtoBuf;

namespace Boost.Proto.Actor.ProtobufNetSerializer;

public class ProtobufNetSerializer : ISerializer
{
    public ByteString Serialize(object obj)
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, obj);
        stream.Position = 0;
        var ret = ByteString.FromStream(stream);
        return ret;
    }

    public object Deserialize(ByteString bytes, string typeName)
    {
        var typeNames = typeName.Split('/');
        var assemblyName = typeNames[0];
        var fullName = typeNames[1];
        var type = Assembly.Load(assemblyName).GetType(fullName);

        using var stream = new MemoryStream();
        stream.Write(bytes.Span);
        stream.Position = 0;

        return Serializer.Deserialize(type, stream);
    }

    public string GetTypeName(object obj)
    {
        var assemblyName = obj.GetType().Assembly.GetName().Name;
        var fullName = obj.GetType().FullName;
        return $"{assemblyName}/{fullName}";
    }

    public bool CanSerialize(object obj) => true;
}
