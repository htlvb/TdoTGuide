using TdoTGuide.WebAsm.Server.Data;

namespace TdoTGuide.WebAsm.Server.IntegrationTests.Utils;

public class InMemoryProjectStore : IProjectStore
{
    private readonly List<Project> projects = new();

    public async IAsyncEnumerable<Project> GetAll()
    {
        foreach (var project in projects)
        {
            await Task.Yield();
            yield return project;
        }
    }

    public async Task<Project?> Get(string projectId)
    {
        await Task.Yield();
        return projects.Find(v => v.Id == projectId);
    }

    public async Task Delete(string projectId)
    {
        await Task.Yield();
        projects.RemoveAll(v => v.Id == projectId);
    }

    public async Task Create(Project project)
    {
        await Task.Yield();
        projects.Add(project);
    }

    public Task Update(Project project)
    {
        throw new NotImplementedException();
    }
}