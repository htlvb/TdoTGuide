using Microsoft.AspNetCore.Mvc;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Visitor.Server.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectController(
    IProjectStore projectStore,
    IDepartmentStore departmentStore,
    IProjectMediaStore projectMediaStore) : ControllerBase
{
    [HttpGet("")]
    public async Task<List<ProjectDto>> GetProjectList()
    {
        var projects = await projectStore.GetAll().ToList();
        var departments = await departmentStore.GetDepartments();
        var departmentDtoLookup = departments.ToDictionary(v => v.Id, GetDepartmentDtoFromDomain);
        var projectMedia = await projectMediaStore.GetAllMedia([.. projects.Select(v => v.Id)]);
        var projectDtos = new List<ProjectDto>();
        foreach (var project in projects)
        {
            if (!projectMedia.TryGetValue(project.Id, out var media))
            {
                media = [];
            }
            var projectDto = GetProjectDtoFromProject(project, departmentDtoLookup, media);
            projectDtos.Add(projectDto);
        }
        return projectDtos;
    }

    private ProjectDto GetProjectDtoFromProject(
        Project project,
        IReadOnlyDictionary<string, DepartmentDto> departmentLookup,
        List<ProjectMedia> media)
    {
        return new ProjectDto(
            project.Title,
            project.Description,
            project.Group,
            [.. project.Departments.Select(v => departmentLookup.TryGetValue(v, out var result) ? result : null).OfType<DepartmentDto>()],
            project.Location,
            project.TimeSelection.Accept(new TimeSelectionToDtoVisitor()),
            [.. media.Select(GetProjectMediaDtoFromDomain)]
        );
    }

    private static DepartmentDto GetDepartmentDtoFromDomain(Department department)
    {
        return new(department.Name, department.Color);
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

    private class TimeSelectionToDtoVisitor : ITimeSelectionVisitor<TimeSelectionDto>
    {
        public TimeSelectionDto VisitContinuousTimeSelection(ContinuousTimeSelection v)
        {
            return new(TimeSelectionTypeDto.Continuous, default, []);
        }

        public TimeSelectionDto VisitRegularTimeSelection(RegularTimeSelection v)
        {
            return new TimeSelectionDto(TimeSelectionTypeDto.Regular, v.IntervalMinutes, []);
        }

        public TimeSelectionDto VisitIndividualTimeSelection(IndividualTimeSelection v)
        {
            return new(TimeSelectionTypeDto.Individual, default, [.. v.Times]);
        }
    }
}
