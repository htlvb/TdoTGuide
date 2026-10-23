using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TdoTGuide.Admin.Server.IntegrationTests.Utils;
using TdoTGuide.Admin.Shared;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests;

public class DisableProjectTests
{
    [Fact]
    public async Task CantDisableOtherProject()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter("1234"); // TODO use real id from IUserStore?

        using var response = await client.PostAsync($"/api/projects/{project.Id}/disable", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CanDisableOwnProject()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.Organizer.Id);

        using var response = await client.PostAsync($"/api/projects/{project.Id}/disable", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True((await projectStore.Get(project.Id))!.IsDisabled);
    }

    [Fact]
    public async Task CanEnableOwnProject()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate() with { IsDisabled = true };
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.Organizer.Id);

        using var response = await client.PostAsync($"/api/projects/{project.Id}/enable", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False((await projectStore.Get(project.Id))!.IsDisabled);
    }

    [Fact]
    public async Task DisabledProjectIsHiddenFromOthers()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate() with { IsDisabled = true };
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter("1234"); // TODO use real id from IUserStore?

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.Empty(projectList!.Projects);
    }

    [Fact]
    public async Task DisabledProjectIsVisibleToOrganizers()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate() with { IsDisabled = true };
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectReader(project.Organizer.Id);

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.Single(projectList!.Projects);
    }

    [Fact]
    public async Task DisabledProjectIsVisibleToCoOrganizers()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate() with { IsDisabled = true, CoOrganizers = [FakeData.ProjectOrganizers[0]] };
        await projectStore.Create(project with { Organizer = FakeData.ProjectOrganizers[1] });
        using var client = host.GetTestClient()
            .AuthenticateAsProjectReader(FakeData.ProjectOrganizers[0].Id);

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.Single(projectList!.Projects);
    }

    [Fact]
    public async Task DisabledProjectIsVisibleWhenAuthorized()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate() with { IsDisabled = true };
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsAllProjectWriter("1234"); // TODO use real id from IUserStore?

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.Single(projectList!.Projects);
    }
}
