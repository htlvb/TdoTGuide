namespace TdoTGuide.Visitor.Server;

public record ProjectDto(
    string Title,
    string Description,
    string Group,
    List<DepartmentDto> Departments,
    string Location,
    TimeSelectionDto TimeSelection,
    List<ProjectMediaDto> Media);

public record DepartmentDto(string Name, string Color);

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

public record ProjectMediaDto(ProjectMediaTypeDto Type, string Url);

public enum ProjectMediaTypeDto
{
    Image,
    Video
}
