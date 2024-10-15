using System.Text.Json.Serialization;

namespace TdoTGuide.Admin.Shared
{
    public record EditingProjectDto(
        EditingProjectDataDto Data,
        IReadOnlyList<string> AllGroups,
        IReadOnlyList<DepartmentDto> AllDepartments,
        IReadOnlyList<BuildingDto> AllBuildings,
        IReadOnlyList<ProjectOrganizerDto> OrganizerCandidates,
        IReadOnlyList<ProjectOrganizerDto> CoOrganizerCandidates,
        EditingProjectLinksDto Links
    );

    public record EditingProjectDataDto(
        string Title,
        string Description,
        IReadOnlyList<string> Groups,
        IReadOnlyList<string> Departments,
        IReadOnlyList<string> MediaFileNames,
        IReadOnlyList<string> MediaFileNamesToRemove,
        string? Building,
        string Location,
        string OrganizerId,
        IReadOnlyList<string> CoOrganizerIds
    );

    public record EditingProjectLinksDto(
        string? Save
    );

    public record ProjectListDto(
        IReadOnlyList<ProjectDto> Projects,
        IReadOnlyList<DepartmentDto> AllDepartments,
        IReadOnlyList<BuildingDto> AllBuildings,
        ProjectListLinksDto Links
    );

    public record ProjectListLinksDto(
        string? CreateProject
    );

    public record ProjectDto(
        string Title,
        string Description,
        IReadOnlyList<string> Groups,
        IReadOnlyList<string> Departments,
        string Building,
        string Location,
        ProjectOrganizerDto Organizer,
        IReadOnlyList<ProjectOrganizerDto> CoOrganizers,
        UserRoleForProjectDto CurrentUserStatus,
        List<ProjectMediaDto> Media,
        ProjectLinksDto Links
    )
    {
        public bool IsUserProject =>
            CurrentUserStatus == UserRoleForProjectDto.Organizer
            || CurrentUserStatus == UserRoleForProjectDto.CoOrganizer;
    }

    public record DepartmentDto(
        string Id,
        string Name,
        string Color
    );

    public record BuildingDto(
        string Id,
        string Name
    );

    public record ProjectOrganizerDto(
        string Id,
        string DisplayName
    );

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRoleForProjectDto
    {
        NotRelated,
        Organizer,
        CoOrganizer
    }

    public record ProjectMediaDto(ProjectMediaTypeDto Type, string Url);

    public enum ProjectMediaTypeDto
    {
        Image,
        Video
    }

    public record ProjectLinksDto(
        string? Edit,
        string? Delete
    );
}
