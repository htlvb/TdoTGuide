using TdoTGuide.WebAsm.Server.Data;

namespace TdoTGuide.WebAsm.Server.IntegrationTests.Utils;

public class InMemoryProjectMediaStore : IProjectMediaStore
{
    private readonly Dictionary<string, List<string>> projectMedia = [];

    public async Task Add(string projectId, IEnumerable<string> fileNames)
    {
        await Task.Yield();
        if (!projectMedia.TryGetValue(projectId, out var list))
        {
            projectMedia[projectId] = list = [];
        }
        list.AddRange(fileNames);
    }

    public async IAsyncEnumerable<string> GetAllMediaNames(string projectId)
    {
        await Task.Yield();
        if (!projectMedia.TryGetValue(projectId, out var list))
        {
            yield break;
        }
        foreach(var item in list)
        {
            yield return item;
        }
    }

    public async Task<Dictionary<string, List<ProjectMedia>>> GetAllMedia(IEnumerable<string> projectIds)
    {
        await Task.Yield();
        Dictionary<string, List<ProjectMedia>> result = [];
        foreach (var projectId in projectIds)
        {
            if (!projectMedia.TryGetValue(projectId, out var list))
            {
                list = [];
            }
            result.Add(projectId, [.. list.Select(v => new ProjectMedia(ProjectMediaType.Image, $"http://localhost/{projectId}/{v}"))]);
        }
        return result;
    }

    public async IAsyncEnumerable<string> GetNewMediaUploadUrls(string projectId, IEnumerable<string> fileNames)
    {
        await Task.Yield();
        foreach (var fileName in fileNames)
        {
            yield return $"http://localhost/{projectId}/{fileName}?key=123";
        }
    }

    public async Task RemoveMedia(string projectId, IEnumerable<string> fileNames)
    {
        await Task.Yield();
        if (projectMedia.TryGetValue(projectId, out var list))
        {
            foreach (var fileName in fileNames)
            {
                list.Remove(fileName);
            }
        }
    }
}