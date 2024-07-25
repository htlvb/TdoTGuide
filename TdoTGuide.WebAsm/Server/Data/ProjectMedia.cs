namespace TdoTGuide.WebAsm.Server.Data;

public record ProjectMedia(ProjectMediaType Type, string Url);

public enum ProjectMediaType
{
    Image,
    Video
}
