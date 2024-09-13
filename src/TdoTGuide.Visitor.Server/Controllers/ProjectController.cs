using Microsoft.AspNetCore.Mvc;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Visitor.Server.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectController(
    IProjectStore projectStore,
    IDepartmentStore departmentStore,
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
        var departments = await departmentStore.GetDepartments();
        var projectMedia = await projectMediaStore.GetAllMedia([.. projects.Select(v => v.Id)]);
        var projectDtos = new List<ProjectDto>();
        foreach (var project in projects)
        {
            if (!projectMedia.TryGetValue(project.Id, out var media))
            {
                media = [];
            }
            var projectDto = GetProjectDtoFromProject(project, media);
            projectDto.Departments.Sort((d1, d2) => departments.FindIndex(v => v.Id == d1).CompareTo(departments.FindIndex(v => v.Id == d2)));
            projectDtos.Add(projectDto);
        }
        return new ProjectListDto(
            projectDtos,
            [.. departments.Select(GetDepartmentDtoFromDomain)]
        );
    }

    private ProjectDto GetProjectDtoFromProject(
        Project project,
        List<ProjectMedia> media)
    {
        return new ProjectDto(
            project.Id,
            project.Title,
            project.Description,
            project.Group,
            [.. project.Departments],
            project.Location,
            [.. media.Select(GetProjectMediaDtoFromDomain)]
        );
    }

    private static DepartmentDto GetDepartmentDtoFromDomain(Department department)
    {
        return new(department.Id, department.Name, department.LongName, department.Color);
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
