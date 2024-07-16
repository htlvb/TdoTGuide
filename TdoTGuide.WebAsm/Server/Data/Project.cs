﻿using TdoTGuide.WebAsm.Shared;
using System.Diagnostics.CodeAnalysis;

namespace TdoTGuide.WebAsm.Server.Data
{
    public sealed record Project(
        string Id,
        string Title,
        string Description,
        string Location,
        ProjectOrganizer Organizer,
        IReadOnlyList<ProjectOrganizer> CoOrganizers,
        DateOnly Date,
        TimeOnly StartTime,
        TimeOnly? EndTime
    )
    {
        public static bool TryCreateFromEditingProjectDataDto(
            EditingProjectDataDto projectData,
            string projectId,
            IReadOnlyDictionary<string, ProjectOrganizer> organizerCandidates,
            [NotNullWhen(true)]out Project? project,
            [NotNullWhen(false)]out string? errorMessage
        )
        {
            if (string.IsNullOrWhiteSpace(projectData.Title))
            {
                project = null;
                errorMessage = "Project title must not be empty.";
                return false;
            }
            if (!organizerCandidates.TryGetValue(projectData.OrganizerId, out var organizer))
            {
                project = null;
                errorMessage = $"Organizer with ID \"{projectData.OrganizerId}\" not found";
                return false;
            }
            var coOrganizerIds = projectData.CoOrganizerIds.Except(new[] { projectData.OrganizerId });
            var coOrganizerErrors = coOrganizerIds
                .Where(coOrganizerId => !organizerCandidates.ContainsKey(coOrganizerId))
                .ToList();
            if (coOrganizerErrors.Count > 0)
            {
                project = null;
                errorMessage = $"Co-Organizers with ID(s) {string.Join(", ", coOrganizerErrors.Select(v => $"\"{v}\""))} not found";
                return false;
            }
            var coOrganizers = coOrganizerIds
                .Select(v => organizerCandidates[v])
                .ToList();
            if (projectData.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                project = null;
                errorMessage = "Project date must be in the future.";
                return false;
            }
            if (projectData.EndTime != null && projectData.StartTime > projectData.EndTime.Value)
            {
                project = null;
                errorMessage = "Project start and end times are invalid.";
                return false;
            }
            project = new Project(
                projectId,
                projectData.Title,
                projectData.Description,
                projectData.Location,
                organizer,
                coOrganizers,
                projectData.Date,
                projectData.StartTime,
                projectData.EndTime
            );
            errorMessage = null;
            return true;
        }
    }
}
