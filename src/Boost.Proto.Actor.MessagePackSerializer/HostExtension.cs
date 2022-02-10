using Microsoft.Extensions.DependencyInjection;

namespace Boost.Proto.Actor.MessagePackSerializer
{
    public static class HostExtension
    {
        public static IServiceCollection AddMessagePack(this IServiceCollection services)
        {
            services.AddSingleton(MessagePack
                .MessagePackSerializer
                .Typeless
                .DefaultOptions
                .WithAllowAssemblyVersionMismatch(true)
                //.WithCompression(MessagePackCompression.Lz4Block)
                .WithOmitAssemblyVersion(true));

            services.AddSingleton(sp => ActivatorUtilities.CreateInstance<MessagePackSerializer>(sp));

            return services;
        }
    }
}
