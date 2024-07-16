﻿using System.Text.Json.Serialization;

namespace TdoTGuide.WebAsm.Shared
{
    public record EditingProjectDto(
        EditingProjectDataDto Data,
        IReadOnlyList<ProjectOrganizerDto> OrganizerCandidates,
        IReadOnlyList<ProjectOrganizerDto> CoOrganizerCandidates,
        EditingProjectLinksDto Links
    );

    public record EditingProjectDataDto(
        string Title,
        string Description,
        string Location,
        string OrganizerId,
        IReadOnlyList<string> CoOrganizerIds,
        DateOnly Date,
        TimeOnly StartTime,
        TimeOnly? EndTime
    );

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
        DateOnly Date,
        TimeOnly StartTime,
        TimeOnly? EndTime,
        UserRoleForProjectDto CurrentUserStatus,
        ProjectLinksDto Links
    )
    {
        public bool IsUserProject =>
            CurrentUserStatus == UserRoleForProjectDto.Organizer
            || CurrentUserStatus == UserRoleForProjectDto.CoOrganizer;
    }

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

    public record ProjectLinksDto(
        string? Edit,
        string? Delete
    );
}