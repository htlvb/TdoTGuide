namespace TdoTGuide.Server.Common;

public record ProjectMedia(ProjectMediaType Type, string Url);

public enum ProjectMediaType
{
    Image,
    Video
}
