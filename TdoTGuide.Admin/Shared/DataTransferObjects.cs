using System.Text.Json.Serialization;

namespace TdoTGuide.Admin.Shared
{
    public record EditingProjectDto(
        EditingProjectDataDto Data,
        IReadOnlyList<DepartmentDto> AllDepartments,
        IReadOnlyList<ProjectOrganizerDto> OrganizerCandidates,
        IReadOnlyList<ProjectOrganizerDto> CoOrganizerCandidates,
        EditingProjectLinksDto Links
    );

    public record EditingProjectDataDto(
        string Title,
        string Description,
        IReadOnlyList<string> Departments,
        IReadOnlyList<string> MediaFileNames,
        IReadOnlyList<string> MediaFileNamesToRemove,
        string Location,
        string OrganizerId,
        IReadOnlyList<string> CoOrganizerIds,
        TimeSelectionDto TimeSelection
    );

    public record TimeSelectionDto(
        TimeSelectionTypeDto Type,
        int RegularIntervalMinutes,
        List<DateTime> IndividualTimes
    );

    public enum TimeSelectionTypeDto
    {
        Continuous,
        Regular,
        Individual
    }

    public record EditingProjectLinksDto(
        string? Save
    );

    public record ProjectListDto(
        IReadOnlyList<ProjectDto> Projects,
        ProjectListLinksDto Links
    );

    public record ProjectListLinksDto(
        string? CreateProject
    );

    public record ProjectDto(
        string Title,
        string Description,
        string Location,
        ProjectOrganizerDto Organizer,
        IReadOnlyList<ProjectOrganizerDto> CoOrganizers,
        TimeSelectionDto TimeSelection,
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
