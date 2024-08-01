namespace TdoTGuide.Server.Common;

public interface IProjectMediaStore
{
    IAsyncEnumerable<string> GetNewMediaUploadUrls(string projectId, IEnumerable<string> fileNames);
    Task RemoveMedia(string projectId, IEnumerable<string> fileNames);
    IAsyncEnumerable<string> GetAllMediaNames(string projectId);
    Task<Dictionary<string, List<ProjectMedia>>> GetAllMedia(IEnumerable<string> projectIds);
}
