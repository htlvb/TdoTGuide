using Microsoft.AspNetCore.Mvc;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Visitor.Server.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectController(
    IProjectStore projectStore,
    IBuildingStore buildingStore,
    IProjectMediaStore projectMediaStore,
    IHostEnvironment env) : ControllerBase
{
    [HttpGet("")]
    public async Task<ProjectListDto> GetProjectList()
    {
        if (env.IsDevelopment() && Random.Shared.NextDouble() > 0.9) {
            await Task.Delay(5000);
            throw new Exception("Chaos engineering exception");
        }
        var projects = await projectStore.GetAll().ToList();
        var projectTypes = await projectStore.GetProjectTypes();
        var buildings = await buildingStore.GetBuildings();
        var projectMedia = await projectMediaStore.GetAllMedia([.. projects.Select(v => v.Id)]);
        var projectDtos = new List<ProjectDto>();
        foreach (var project in projects)
        {
            if (!projectMedia.TryGetValue(project.Id, out var media))
            {
                media = [];
            }
            var projectDto = GetProjectDtoFromProject(project, media, projectTypes);
            projectDtos.Add(projectDto);
        }
        return new ProjectListDto(
            projectDtos,
            projectTypes
                .Select(projectType => projectType.Accept(new AnonymousSelectionTypeVisitor<List<ProjectTagDto>>(
                    (SimpleSelectionType selectionType) => [ new ProjectTagDto(null, selectionType.Title, selectionType.Color) ],
                    (MultiSelectSelectionType selectionType) => [.. selectionType.Choices.Select(choice => new ProjectTagDto(choice.ShortName, choice.LongName, choice.Color))]
                )))
                .ToList(),
            [.. buildings.Select(GetBuildingDtoFromDomain)]
        );
    }

    private ProjectDto GetProjectDtoFromProject(
        Project project,
        List<ProjectMedia> media,
        List<ISelectionType> projectTypes)
    {
        return new ProjectDto(
            project.Id,
            project.Title,
            project.Description,
            GetProjectTagDtos(project.Type, projectTypes),
            project.Building,
            project.Location,
            [.. media.Select(GetProjectMediaDtoFromDomain)]
        );
    }

    private static List<ProjectTagDto> GetProjectTagDtos(ISelection type, List<ISelectionType> projectTypes)
    {
        return type.Accept(new AnonymousSelectionVisitor<List<ProjectTagDto>>(
            (SimpleSelection selection) => projectTypes.Find(v => v.Id == selection.Name)?.Accept(new AnonymousSelectionTypeVisitor<List<ProjectTagDto>>(
                (SimpleSelectionType selectionType) => [new ProjectTagDto(null, selectionType.Title, selectionType.Color)],
                (MultiSelectSelectionType _) => []
            )) ?? [],
            (MultiSelectSelection selection) => projectTypes.Find(v => v.Id == selection.Name)?.Accept(new AnonymousSelectionTypeVisitor<List<ProjectTagDto>>(
                (SimpleSelectionType _) => [],
                (MultiSelectSelectionType selectionType) => selectionType.Choices
                    .Where(choice => selection.SelectedValues.Contains(choice.Id))
                    .Select(v => new ProjectTagDto(v.ShortName, v.LongName, v.Color))
                    .ToList()
            )) ?? []
        ));
    }

    private static BuildingDto GetBuildingDtoFromDomain(Building building)
    {
        return new(building.Id, building.Name);
    }

    private ProjectMediaDto GetProjectMediaDtoFromDomain(ProjectMedia media)
    {
        var type = media.Type switch
        {
            ProjectMediaType.Image => ProjectMediaTypeDto.Image,
            ProjectMediaType.Video => ProjectMediaTypeDto.Video,
            _ => throw new Exception($"Unknown project media type: {media.Type}")
        };
        return new ProjectMediaDto(type, media.Url);
    }
}
