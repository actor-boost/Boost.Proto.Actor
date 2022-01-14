using Boost.Proto.Actor.Decorators;
using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting.Logging;

public static class HostExtensions
{
    public static IHostBuilder UseProtoActorLogging(this IHostBuilder host,
                                                    Action<IServiceProvider, HostOption> config)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton(sp =>
            {
                var ret = ActivatorUtilities.CreateInstance<HostOption>(sp);
                config?.Invoke(sp, ret);
                return ret;
            });

            services.AddSingleton<FuncProps>(sp => x =>
                x.WithContextDecorator(ctx =>
                    ActivatorUtilities.CreateInstance<LoggerActorContextDecorator>(sp, ctx)));
        });

        return host;
    }
}
