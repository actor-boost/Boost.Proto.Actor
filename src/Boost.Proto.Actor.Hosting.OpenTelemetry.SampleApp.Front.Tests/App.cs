using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Front.Tests;

public class App : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("UnitTest");
        return base.CreateHost(builder);
    }
}
