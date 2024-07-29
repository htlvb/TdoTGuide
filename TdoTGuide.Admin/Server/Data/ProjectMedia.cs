namespace TdoTGuide.Admin.Server.Data;

public record ProjectMedia(ProjectMediaType Type, string Url);

public enum ProjectMediaType
{
    Image,
    Video
}
