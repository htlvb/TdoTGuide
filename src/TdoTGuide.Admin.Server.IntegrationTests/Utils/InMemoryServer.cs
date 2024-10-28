using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

public static class InMemoryServer
{
    public static async Task<IHost> Start()
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services
                            .AddAuthentication(TestAuthHandler.SchemeName)
                            .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.SchemeName, options => { });
                        services.AddTdoTGuideAuthorizationRules();
                        services.AddTdoTGuideControllers();
                        services.AddSingleton<IUserStore>(new InMemoryUserStore(FakeData.ProjectOrganizers));
                        services.AddSingleton<IProjectStore, InMemoryProjectStore>();
                        services.AddSingleton<IBuildingStore, InMemoryBuildingStore>();
                        services.AddSingleton<IProjectMediaStore, InMemoryProjectMediaStore>();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();

                        app.UseAuthentication();
                        app.UseAuthorization();

                        app.UseEndpoints(config => config.MapControllers());
                    });
            })
            .StartAsync();
    }

    public static JsonSerializerOptions GetJsonSerializerOptions(this IHost host)
    {
        return host.Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
    }
}
