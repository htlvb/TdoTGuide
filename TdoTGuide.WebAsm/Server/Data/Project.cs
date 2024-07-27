using TdoTGuide.WebAsm.Shared;
using System.Diagnostics.CodeAnalysis;

namespace TdoTGuide.WebAsm.Server.Data
{
    public sealed record Project(
        string Id,
        string Title,
        string Description,
        IReadOnlyList<string> Departments,
        string Location,
        ProjectOrganizer Organizer,
        IReadOnlyList<ProjectOrganizer> CoOrganizers,
        ITimeSelection TimeSelection
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
            ITimeSelection timeSelection;
            if (projectData.TimeSelection.Type == TimeSelectionTypeDto.Continuous)
            {
                timeSelection = new ContinuousTimeSelection();
            }
            else if (projectData.TimeSelection.Type == TimeSelectionTypeDto.Regular)
            {
                if (projectData.TimeSelection.RegularIntervalMinutes <= 0 || projectData.TimeSelection.RegularIntervalMinutes % 5 != 0)
                {
                    project = null;
                    errorMessage = "Regular time interval must be positive and divisible by 5.";
                    return false;
                }
                timeSelection = new RegularTimeSelection(projectData.TimeSelection.RegularIntervalMinutes);
            }
            else if (projectData.TimeSelection.Type == TimeSelectionTypeDto.Individual)
            {
                if (projectData.TimeSelection.IndividualTimes.Count == 0 || projectData.TimeSelection.IndividualTimes.Any(v => v < DateTime.Now))
                {
                    project = null;
                    errorMessage = "At least one custom time must be provided and every time must be in the future.";
                    return false;
                }
                timeSelection = new IndividualTimeSelection(projectData.TimeSelection.IndividualTimes);
            }
            else 
            {
                project = null;
                errorMessage = "Invalid time selection.";
                return false;
            }
            project = new Project(
                projectId,
                projectData.Title,
                projectData.Description,
                projectData.Departments,
                projectData.Location,
                organizer,
                coOrganizers,
                timeSelection
            );
            errorMessage = null;
            return true;
        }
    }
}
