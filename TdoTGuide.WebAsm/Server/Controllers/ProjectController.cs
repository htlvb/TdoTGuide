using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using TdoTGuide.WebAsm.Server.Data;
using TdoTGuide.WebAsm.Shared;

namespace TdoTGuide.WebAsm.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectStore projectStore;
        private readonly IUserStore userStore;
        private readonly IAuthorizationService authService;

        private string UserId => HttpContext.User.GetObjectId()!;

        public ProjectController(
            IProjectStore projectStore,
            IUserStore userStore,
            IAuthorizationService authService)
        {
            this.projectStore = projectStore;
            this.userStore = userStore;
            this.authService = authService;
        }

        [HttpGet("")]
        [Authorize("ListProjects")]
        public async Task<ProjectListDto> GetProjectList()
        {
            var projects = (await projectStore.GetAll().ToList())
                .GroupBy(v => v.Date).OrderBy(v => v.Key).SelectMany(v => v); // Sort by date, but don't change order of projects with same date;

            var projectDtos = new List<ProjectDto>();
            foreach (var project in projects)
            {
                var projectDto = await GetProjectDtoFromProject(project);
                projectDtos.Add(projectDto);
            }
            var canCreateProject = (await authService.AuthorizeAsync(HttpContext.User, "CreateProject")).Succeeded;
            return new ProjectListDto(
                projectDtos,
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

                var result = new EditingProjectDto(
                    new EditingProjectDataDto(
                        "",
                        "",
                        "",
                        UserId,
                        Array.Empty<string>(),
                        DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
                        TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)),
                        TimeOnly.FromTimeSpan(TimeSpan.FromHours(13))
                    ),
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
                var result = new EditingProjectDto(
                    new EditingProjectDataDto(
                        project.Title,
                        project.Description,
                        project.Location,
                        project.Organizer.Id,
                        project.CoOrganizers.Select(v => v.Id).ToArray(),
                        project.Date,
                        project.StartTime,
                        project.EndTime
                    ),
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
        public async Task<IActionResult> CreateProject([FromBody]EditingProjectDataDto projectData)
        {
            var organizerCandidates = await GetOrganizerCandidatesDictionary();
            if (!Project.TryCreateFromEditingProjectDataDto(projectData, Guid.NewGuid().ToString(), organizerCandidates, out var project, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }
            if (!(await authService.AuthorizeAsync(HttpContext.User, project, "CreateProject")).Succeeded)
            {
                return Forbid();
            }
            await projectStore.Create(project);
            return Ok();
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
            if (!Project.TryCreateFromEditingProjectDataDto(projectData, projectId, organizerCandidates, out var project, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }
            if (!(await authService.AuthorizeAsync(HttpContext.User, project, "UpdateProject")).Succeeded)
            {
                return Forbid();
            }
            await projectStore.Update(project);
            return Ok();
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
            return Ok();
        }

        private async Task<Dictionary<string, ProjectOrganizer>> GetOrganizerCandidatesDictionary()
        {
            var organizerCandidates = await userStore.GetOrganizerCandidates().ToList();
            return organizerCandidates.ToDictionary(v => v.Id);
        }

        private async Task<ProjectDto> GetProjectDtoFromProject(Project project)
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
                project.Location,
                mapOrganizer(project.Organizer),
                project.CoOrganizers.Select(mapOrganizer).ToArray(),
                project.Date,
                project.StartTime,
                project.EndTime,
                getCurrentUserRole(project),
                new ProjectLinksDto(
                    canUpdate ? $"projects/edit/{project.Id}" : default,
                    canDelete ? Url.Action(nameof(DeleteProject), new { projectId = project.Id }) : default
                )
            );
        }
    }
}
