using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using TdoTGuide.Admin.Server.IntegrationTests.Utils;
using TdoTGuide.Admin.Shared;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests;

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
        var isProjectValid = Mapper.TryMapEditingProjectDataDtoToProject(project, actualNewProjects[0].Id, FakeData.ProjectOrganizers.ToDictionary(v => v.Id), out var expectedNewProject, out _);
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
        }
    }

    [Theory]
    [MemberData(nameof(InvalidProjectData))]
    public async Task CantCreateInvalidProject(
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1026: Unused parameter", Justification = "Improves readability.")] string description,
        EditingProjectDataDto project
    )
    {
        using var host = await InMemoryServer.Start();
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.OrganizerId);

        using var response = await client.PostAsJsonAsync("/api/projects", project, host.GetJsonSerializerOptions());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CanGetUploadUrlsWhenCreatingProjectWithMedia()
    {
        using var host = await InMemoryServer.Start();
        var project = FakeData.EditingProjectDataDtoFaker.Generate() with { MediaFileNames = ["1.jpg", "2.png", "3.mp4"] };
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.OrganizerId);

        using var response = await client.PostAsJsonAsync("/api/projects", project, host.GetJsonSerializerOptions());
        response.EnsureSuccessStatusCode();
        var uploadUrls = (await response.Content.ReadFromJsonAsync<string[]>())!;
        Assert.Equal(project.MediaFileNames.Count, uploadUrls.Length);
    }

    [Fact]
    public async Task CanGetUploadUrlsWhenUpdatingProjectWithMedia()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.Organizer.Id);
        var editProject = new EditingProjectDataDto(
            project.Title,
            project.Description,
            project.Type.Accept(new AnonymousSelectionVisitor<SelectionReferenceDto>(
                (SimpleSelection selection) => new SimpleSelectionReferenceDto(selection.Name),
                (MultiSelectSelection selection) => new MultiSelectSelectionReferenceDto(selection.Name, [.. selection.SelectedValues])
            )),
            ["1.jpg", "2.mp4"],
            [],
            project.Building,
            project.Location,
            project.Organizer.Id,
            [.. project.CoOrganizers.Select(v => v.Id)]
        );

        using var response = await client.PostAsJsonAsync($"/api/projects/{project.Id}", editProject, host.GetJsonSerializerOptions());
        response.EnsureSuccessStatusCode();
        var uploadUrls = (await response.Content.ReadFromJsonAsync<string[]>())!;
        Assert.Equal(editProject.MediaFileNames.Count, uploadUrls.Length);
    }

    [Fact]
    public async Task CanUpdateMedia()
    {
        using var host = await InMemoryServer.Start();
        var projectStore = host.Services.GetRequiredService<IProjectStore>();
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        var projectMediaStore = (InMemoryProjectMediaStore)host.Services.GetRequiredService<IProjectMediaStore>();
        var projectMedia = new[] { "1.jpg", "2.png", "3.mp4" };
        await projectMediaStore.Add(project.Id, projectMedia);
        using var client = host.GetTestClient()
            .AuthenticateAsProjectWriter(project.Organizer.Id);
        var projectUpdate = new EditingProjectDataDto(
            project.Title,
            project.Description,
            project.Type.Accept(new AnonymousSelectionVisitor<SelectionReferenceDto>(
                (SimpleSelection selection) => new SimpleSelectionReferenceDto(selection.Name),
                (MultiSelectSelection selection) => new MultiSelectSelectionReferenceDto(selection.Name, [.. selection.SelectedValues])
            )),
            ["4.png", "5.mp4"],
            ["1.jpg", "3.mp4"],
            project.Building,
            project.Location,
            project.Organizer.Id,
            [.. project.CoOrganizers.Select(v => v.Id)]
        );

        using var response = await client.PostAsJsonAsync($"/api/projects/{project.Id}", projectUpdate, host.GetJsonSerializerOptions());
        response.EnsureSuccessStatusCode();
        var uploadUrls = (await response.Content.ReadFromJsonAsync<string[]>())!;
        await projectMediaStore.Add(project.Id, uploadUrls.Select(v => new Uri(v).Segments.Last()));

        var editProject = (await client.GetFromJsonAsync<EditingProjectDto>($"/api/projects/edit/{project.Id}"))!;
        Assert.Equal(projectMedia.Length + projectUpdate.MediaFileNames.Count - projectUpdate.MediaFileNamesToRemove.Count, editProject.Data.MediaFileNames.Count);
    }
}
