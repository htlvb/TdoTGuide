using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TdoTGuide.Admin.Server.IntegrationTests.Utils;
using TdoTGuide.Admin.Shared;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests;

public class GetProjectListTests
{
    [Fact]
    public async Task CantGetProjectListWhenNotAuthorized()
    {
        using var host = await InMemoryServer.Start();
        using var client = host.GetTestClient()
            .AuthenticateWithoutRole("1234"); // TODO use real id from IUserStore?

        using var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CanGetProjectListWhenAuthorized()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var allProjects = FakeData.ProjectFaker.Generate(100);
        foreach (var project in allProjects)
        {
            await projectStore.Create(project);
        }
        using var client = host.GetTestClient()
            .AuthenticateAsProjectReader("1234"); // TODO use real id from IUserStore?

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.Equal(allProjects.Count, projectList!.Projects.Count);
    }

    [Fact]
    public async Task CreateNewProjectLinkIsNotEmptyIfAuthorized()
    {
        using var host = await InMemoryServer.Start();
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter("1234"); // TODO use real id from IUserStore?

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.NotNull(projectList!.Links.CreateProject);
    }

    [Fact]
    public async Task DeleteProjectLinkIsEmptyIfNotAuthorized()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter("1234"); // TODO use real id from IUserStore?

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.Null(projectList!.Projects[0].Links.Delete);
    }

    [Fact]
    public async Task DeleteProjectLinkIsNotEmptyIfAuthorized()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.Organizer.Id);

        var projectList = await client.GetFromJsonAsync<ProjectListDto>("/api/projects", host.GetJsonSerializerOptions());

        Assert.NotNull(projectList!.Projects[0].Links.Delete);
    }
}
