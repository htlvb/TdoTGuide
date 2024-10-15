namespace TdoTGuide.Server.Common
{
    public sealed record Project(
        string Id,
        string Title,
        string Description,
        IReadOnlyList<string> Groups,
        IReadOnlyList<string> Departments,
        string Building,
        string Location,
        ProjectOrganizer Organizer,
        IReadOnlyList<ProjectOrganizer> CoOrganizers
    );
}
