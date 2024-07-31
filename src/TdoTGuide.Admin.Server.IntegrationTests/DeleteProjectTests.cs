using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TdoTGuide.Admin.Server.Data;
using TdoTGuide.Admin.Server.IntegrationTests.Utils;
using System.Net;

namespace TdoTGuide.Admin.Server.IntegrationTests;

public class DeleteProjectTests
{
    [Fact]
    public async Task CantDeleteOtherProject()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter("1234"); // TODO use real id from IUserStore?

        using var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CanDeleteOwnProject()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.Organizer.Id);

        using var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CanDeleteOtherProjectWhenAuthorized()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsAllProjectWriter("1234"); // TODO use real id from IUserStore?

        using var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
