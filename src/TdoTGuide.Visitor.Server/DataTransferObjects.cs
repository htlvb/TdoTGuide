namespace TdoTGuide.Visitor.Server;

public record ProjectListDto(
    List<ProjectDto> Projects,
    List<DepartmentDto> Departments
);

public record ProjectDto(
    string Id,
    string Title,
    string Description,
    string? Group,
    List<string> Departments,
    string Location,
    List<ProjectMediaDto> Media);

public record DepartmentDto(string Id, string Name, string LongName, string Color);

public record ProjectMediaDto(ProjectMediaTypeDto Type, string Url);

public enum ProjectMediaTypeDto
{
    Image,
    Video
}
