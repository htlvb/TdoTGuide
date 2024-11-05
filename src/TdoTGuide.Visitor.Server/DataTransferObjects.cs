namespace TdoTGuide.Visitor.Server;

public record ProjectListDto(
    List<ProjectDto> Projects,
    List<List<ProjectTagDto>> ProjectTags,
    List<BuildingDto> Buildings
);

public record ProjectDto(
    string Id,
    string Title,
    string Description,
    List<ProjectTagDto> Tags,
    string Building,
    string? Floor,
    string Location,
    List<ProjectMediaDto> Media);

public record ProjectTagDto(string? ShortName, string LongName, string Color);

public record BuildingDto(string Id, string Name);

public record ProjectMediaDto(ProjectMediaTypeDto Type, string Url);

public enum ProjectMediaTypeDto
{
    Image,
    Video
}
