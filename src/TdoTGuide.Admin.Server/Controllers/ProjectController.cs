﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using TdoTGuide.Admin.Shared;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects")]
    public class ProjectController(
        IProjectStore projectStore,
        IDepartmentStore departmentStore,
        IBuildingStore buildingStore,
        IProjectMediaStore projectMediaStore,
        IUserStore userStore,
        IAuthorizationService authService) : ControllerBase
    {
        private string UserId => HttpContext.User.GetObjectId()!;

        [HttpGet("")]
        [Authorize("ListProjects")]
        public async Task<ProjectListDto> GetProjectList()
        {
            var projects = await projectStore.GetAll().ToList();
            var projectMedia = await projectMediaStore.GetAllMedia([.. projects.Select(v => v.Id)]);
            var projectDtos = new List<ProjectDto>();
            foreach (var project in projects)
            {
                if (!projectMedia.TryGetValue(project.Id, out var media))
                {
                    media = [];
                }
                var projectDto = await GetProjectDtoFromProject(project, media);
                projectDtos.Add(projectDto);
            }
            var canCreateProject = (await authService.AuthorizeAsync(HttpContext.User, "CreateProject")).Succeeded;
            var departments = await departmentStore.GetDepartments();
            var buildings = await buildingStore.GetBuildings();
            return new ProjectListDto(
                projectDtos,
                [.. departments.Select(GetDepartmentDtoFromDomain)],
                [.. buildings.Select(GetBuildingDtoFromDomain)],
                new ProjectListLinksDto(
                    canCreateProject ? "projects/new" : default
                )
            );
        }

        [HttpGet("edit/{projectId}")]
        public async Task<IActionResult> GetProject(string projectId)
        {
            IReadOnlyList<ProjectOrganizerDto> coOrganizerCandidates = (await userStore.GetOrganizerCandidates().ToList())
                .OrderBy(v => v.LastName)
                .ThenBy(v => v.FirstName)
                .Select(v => new ProjectOrganizerDto(v.Id, $"{v.LastName} {v.FirstName} ({v.ShortName})"))
                .ToList();
            IReadOnlyList<ProjectOrganizerDto> organizerCandidates;
            if ((await authService.AuthorizeAsync(HttpContext.User, "ChangeProjectOrganizer")).Succeeded)
            {
                organizerCandidates = coOrganizerCandidates;
            }
            else
            {
                organizerCandidates = coOrganizerCandidates
                    .Where(v => v.Id == UserId)
                    .ToList();
            }

            if (projectId == "new")
            {
                if (!(await authService.AuthorizeAsync(HttpContext.User, "CreateProject")).Succeeded)
                {
                    return Forbid();
                }

                var projectGroups = await projectStore.GetProjectGroups().ToList();
                var departments = await departmentStore.GetDepartments();
                var buildings = await buildingStore.GetBuildings();

                var result = new EditingProjectDto(
                    new EditingProjectDataDto(
                        Title: "",
                        Description: "",
                        Groups: [],
                        Departments: [],
                        MediaFileNames: [],
                        MediaFileNamesToRemove: [],
                        Building: null,
                        Location: "",
                        OrganizerId: UserId,
                        CoOrganizerIds: Array.Empty<string>()
                    ),
                    projectGroups,
                    [.. departments.Select(GetDepartmentDtoFromDomain)],
                    [.. buildings.Select(GetBuildingDtoFromDomain)],
                    organizerCandidates,
                    coOrganizerCandidates,
                    new EditingProjectLinksDto(
                        Url.Action(nameof(CreateProject))
                    )
                );
                return base.Ok(result);
            }
            else
            {
                var project = await projectStore.Get(projectId);
                if (project == null)
                {
                    return NotFound("Project doesn't exist.");
                }
                if (!(await authService.AuthorizeAsync(HttpContext.User, project, "UpdateProject")).Succeeded)
                {
                    return Forbid();
                }
                var projectMediaNames = await projectMediaStore.GetAllMediaNames(project.Id).ToList();
                var projectGroups = await projectStore.GetProjectGroups().ToList();
                var departments = await departmentStore.GetDepartments();
                var buildings = await buildingStore.GetBuildings();
                var result = new EditingProjectDto(
                    new EditingProjectDataDto(
                        project.Title,
                        project.Description,
                        project.Groups,
                        project.Departments,
                        projectMediaNames,
                        [],
                        project.Building,
                        project.Location,
                        project.Organizer.Id,
                        project.CoOrganizers.Select(v => v.Id).ToArray()
                    ),
                    projectGroups,
                    [.. departments.Select(GetDepartmentDtoFromDomain)],
                    [.. buildings.Select(GetBuildingDtoFromDomain)],
                    organizerCandidates,
                    coOrganizerCandidates,
                    new EditingProjectLinksDto(
                        Url.Action(nameof(UpdateProject), new { projectId = project.Id })
                    )
                );
                return Ok(result);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateProject([FromBody] EditingProjectDataDto projectData)
        {
            var organizerCandidates = await GetOrganizerCandidatesDictionary();
            if (!Mapper.TryMapEditingProjectDataDtoToProject(projectData, Guid.NewGuid().ToString(), organizerCandidates, out var project, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }
            if (!(await authService.AuthorizeAsync(HttpContext.User, project, "CreateProject")).Succeeded)
            {
                return Forbid();
            }
            await projectStore.Create(project);
            var uniqueMediaFileNames = projectData.MediaFileNames.Select(GetUniqueFileName);
            var fileUploadUrls = await projectMediaStore.GetNewMediaUploadUrls(project.Id, uniqueMediaFileNames).ToList();
            return Ok(fileUploadUrls);
        }

        [HttpPost("{projectId}")]
        public async Task<IActionResult> UpdateProject(string projectId, [FromBody] EditingProjectDataDto projectData)
        {
            var existingProject = await projectStore.Get(projectId);
            if (existingProject == null)
            {
                return NotFound("Project doesn't exist.");
            }

            var organizerCandidates = await GetOrganizerCandidatesDictionary();
            if (!Mapper.TryMapEditingProjectDataDtoToProject(projectData, projectId, organizerCandidates, out var project, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }
            if (!(await authService.AuthorizeAsync(HttpContext.User, project, "UpdateProject")).Succeeded)
            {
                return Forbid();
            }
            await projectStore.Update(project);
            await projectMediaStore.RemoveMedia(project.Id, projectData.MediaFileNamesToRemove);
            var uniqueMediaFileNames = projectData.MediaFileNames.Select(GetUniqueFileName);
            var fileUploadUrls = await projectMediaStore.GetNewMediaUploadUrls(project.Id, uniqueMediaFileNames).ToList();
            return Ok(fileUploadUrls);
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(string projectId)
        {
            var existingProject = await projectStore.Get(projectId);
            if (existingProject == null)
            {
                return NotFound("Project doesn't exist.");
            }

            if (!(await authService.AuthorizeAsync(HttpContext.User, existingProject, "DeleteProject")).Succeeded)
            {
                return Forbid();
            }
            await projectStore.Delete(projectId);
            var mediaNames = await projectMediaStore.GetAllMediaNames(projectId).ToList();
            await projectMediaStore.RemoveMedia(projectId, mediaNames);
            return Ok();
        }

        private static string GetUniqueFileName(string fileName)
        {
            return $"{Path.GetFileNameWithoutExtension(fileName)}-{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        }

        private async Task<Dictionary<string, ProjectOrganizer>> GetOrganizerCandidatesDictionary()
        {
            var organizerCandidates = await userStore.GetOrganizerCandidates().ToList();
            return organizerCandidates.ToDictionary(v => v.Id);
        }

        private async Task<ProjectDto> GetProjectDtoFromProject(Project project, List<ProjectMedia> media)
        {
            ProjectOrganizerDto mapOrganizer(ProjectOrganizer organizer)
            {
                return new ProjectOrganizerDto(organizer.Id, $"{organizer.LastName} {organizer.FirstName} ({organizer.ShortName})");
            }

            UserRoleForProjectDto getCurrentUserRole(Project project)
            {
                if (project.Organizer.Id == UserId)
                {
                    return UserRoleForProjectDto.Organizer;
                }
                if (project.CoOrganizers.Any(v => v.Id == UserId))
                {
                    return UserRoleForProjectDto.CoOrganizer;
                }
                return UserRoleForProjectDto.NotRelated;
            }

            var userRole = getCurrentUserRole(project);
            var canUpdate = (await authService.AuthorizeAsync(HttpContext.User, project, "UpdateProject")).Succeeded;
            var canDelete = (await authService.AuthorizeAsync(HttpContext.User, project, "DeleteProject")).Succeeded;
            return new ProjectDto(
                project.Title,
                project.Description,
                project.Groups,
                project.Departments,
                project.Building,
                project.Location,
                mapOrganizer(project.Organizer),
                project.CoOrganizers.Select(mapOrganizer).ToArray(),
                getCurrentUserRole(project),
                [.. media.Select(GetProjectMediaDtoFromDomain)],
                new ProjectLinksDto(
                    canUpdate ? $"projects/edit/{project.Id}" : default,
                    canDelete ? Url.Action(nameof(DeleteProject), new { projectId = project.Id }) : default
                )
            );
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

        private static DepartmentDto GetDepartmentDtoFromDomain(Department department)
        {
            return new(department.Id, department.Name, department.Color);
        }

        private static BuildingDto GetBuildingDtoFromDomain(Building building)
        {
            return new(building.Id, building.Name);
        }
    }
}
