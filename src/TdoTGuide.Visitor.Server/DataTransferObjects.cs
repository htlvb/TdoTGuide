namespace TdoTGuide.Visitor.Server;

public record ProjectListDto(
    List<ProjectDto> Projects,
    List<DepartmentDto> Departments,
    List<BuildingDto> Buildings
);

public record ProjectDto(
    string Id,
    string Title,
    string Description,
    List<string> Groups,
    List<string> Departments,
    string Building,
    string Location,
    List<ProjectMediaDto> Media);

public record DepartmentDto(string Id, string Name, string LongName, string Color);
public record BuildingDto(string Id, string Name);

public record ProjectMediaDto(ProjectMediaTypeDto Type, string Url);

public enum ProjectMediaTypeDto
{
    Image,
    Video
}
