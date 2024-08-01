﻿namespace TdoTGuide.Server.Common
{
    public sealed record Project(
        string Id,
        string Title,
        string Description,
        string Group,
        IReadOnlyList<string> Departments,
        string Location,
        ProjectOrganizer Organizer,
        IReadOnlyList<ProjectOrganizer> CoOrganizers,
        ITimeSelection TimeSelection
    );
}