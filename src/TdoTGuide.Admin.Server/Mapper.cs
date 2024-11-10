using System.Diagnostics.CodeAnalysis;
using TdoTGuide.Admin.Shared;
using TdoTGuide.Server.Common;

public static class Mapper
{
    public static bool TryMapEditingProjectDataDtoToProject(
        EditingProjectDataDto projectData,
        string projectId,
        IReadOnlyDictionary<string, ProjectOrganizer> organizerCandidates,
        [NotNullWhen(true)] out Project? project,
        [NotNullWhen(false)] out string? errorMessage
    )
    {
        if (string.IsNullOrWhiteSpace(projectData.Title))
        {
            project = null;
            errorMessage = "Project title must not be empty.";
            return false;
        }
        var projectType = projectData.Type.Accept(new AnonymousSelectionReferenceDtoVisitor<ISelection>(
            (SimpleSelectionReferenceDto selectionReference) => new SimpleSelection(selectionReference.Name),
            (MultiSelectSelectionReferenceDto selectionReference) => new MultiSelectSelection(selectionReference.Name, selectionReference.SelectedValues)
        ));
        if (projectData.Building is null)
        {
            project = null;
            errorMessage = "Building must be selected.";
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
        project = new Project(
            projectId,
            projectData.Title,
            projectData.Description,
            projectType,
            projectData.Building,
            !string.IsNullOrWhiteSpace(projectData.Floor) ? projectData.Floor : null,
            projectData.Location,
            organizer,
            coOrganizers,
            true,
            null
        );
        errorMessage = null;
        return true;
    }
}
