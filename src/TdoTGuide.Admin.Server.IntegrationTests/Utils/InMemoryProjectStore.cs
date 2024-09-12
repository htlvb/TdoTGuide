using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

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

    public async IAsyncEnumerable<string> GetProjectGroups()
    {
        await Task.Yield();
        foreach (var group in projects.Select(v => v.Group).OfType<string>().Distinct())
        {
            yield return group;
        }
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

    public async Task Update(Project project)
    {
        await Task.Yield();
        var index = projects.FindIndex(v => v.Id == project.Id);
        if (index == -1)
        {
            throw new Exception("Project cannot be updated because it doesn't exist.");
        }
        projects[index] = project;
    }
}