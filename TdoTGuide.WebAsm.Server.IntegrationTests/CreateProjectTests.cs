using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TdoTGuide.WebAsm.Server.Data;
using TdoTGuide.WebAsm.Server.IntegrationTests.Utils;
using TdoTGuide.WebAsm.Shared;
using System.Net;
using System.Net.Http.Json;

namespace TdoTGuide.WebAsm.Server.IntegrationTests;

public class CreateProjectTests
{
    [Fact]
    public async Task CanCreateProjectAsOrganizerWhenAuthorized()
    {
        using var host = await InMemoryServer.Start();
        var project = FakeData.EditingProjectDataDtoFaker.Generate();
        using var client = host.GetTestClient().AuthenticateAsProjectWriter(project.OrganizerId);
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var existingProjects = await projectStore.GetAll().ToList();

        using var response = await client.PostAsJsonAsync("/api/projects", project, host.GetJsonSerializerOptions());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var actualNewProjects = (await projectStore.GetAll().ToList()).Except(existingProjects).ToList();
        Assert.Single(actualNewProjects);
        var isProjectValid = Project.TryCreateFromEditingProjectDataDto(project, actualNewProjects[0].Id, FakeData.ProjectOrganizers.ToDictionary(v => v.Id), out var expectedNewProject, out _);
        Assert.True(isProjectValid, "Expected project to be valid");
        Assert.Equal(expectedNewProject, actualNewProjects[0], new ProjectEqualityComparer());
    }

    [Fact]
    public async Task CantCreateProjectWithOtherOrganizerWhenNotAuthorized()
    {
        using var host = await InMemoryServer.Start();
        var project = FakeData.EditingProjectDataDtoFaker.Generate();
        var writerId = FakeData.ProjectOrganizers
            .First(v => v.Id != project.OrganizerId).Id;
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(writerId);

        using var response = await client.PostAsJsonAsync("/api/projects", project, host.GetJsonSerializerOptions());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    public static IEnumerable<object[]> InvalidProjectData
    {
        get
        {
            yield return new object[] { "Title is empty", FakeData.EditingProjectDataDtoFaker.Generate() with { Title = "" } };
            yield return new object[] { "Title is white-space", FakeData.EditingProjectDataDtoFaker.Generate() with { Title = " " } };
            yield return new object[] { "Unknown organizer", FakeData.EditingProjectDataDtoFaker.Generate() with { OrganizerId = "unknown-organizer" } };
            yield return new object[] { "Unknown co-organizer", FakeData.EditingProjectDataDtoFaker.Generate() with { CoOrganizerIds = new[] { "unknown-co-organizer" } } };
            yield return new object[] { "Date is in the past", FakeData.EditingProjectDataDtoFaker.Generate() with { Date = DateOnly.FromDateTime(DateTime.Today).AddDays(-1) } };
            yield return new object[] { "Start time and end time are invalid", FakeData.EditingProjectDataDtoFaker.Generate() with { StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(9, 59) } };
        }
    }

    [Theory]
    [MemberData(nameof(InvalidProjectData))]
    public async Task CantCreateInvalidProject(
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026: Unused parameter", Justification = "Improves readability.")]string description,
        EditingProjectDataDto project
    )
    {
        using var host = await InMemoryServer.Start();
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.OrganizerId);

        using var response = await client.PostAsJsonAsync("/api/projects", project, host.GetJsonSerializerOptions());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
