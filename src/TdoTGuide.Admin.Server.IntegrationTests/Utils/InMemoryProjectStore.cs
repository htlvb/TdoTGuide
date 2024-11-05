using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

public class InMemoryProjectStore : IProjectStore
{
    private readonly List<Project> projects = [];

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

    public async Task<List<ISelectionType>> GetProjectTypes()
    {
        await Task.Yield();
        return [
            new SimpleSelectionType("1", "Allgemeine Infos und Anmeldung", "#000000AA"),
            new SimpleSelectionType("2", "Abteilungsübergreifend", "#ACA100"),
            new SimpleSelectionType("3", "Nur bei uns", "#B24DAD"),
            new MultiSelectSelectionType("4", "Abteilungsspezifisch", [
                new SelectionChoice("1", "#183c7b", "MB", "Maschinenbau - Anlagentechnik"),
                new SelectionChoice("2", "#ad1410", "ME", "Mechatronik"),
                new SelectionChoice("3", "#009ec6", "FS", "Fachschule Maschinenbau"),
                new SelectionChoice("4", "#008040", "GT", "Gebäudetechnik"),
                new SelectionChoice("5", "#e78a00", "IEM", "Industrial Engineering and Management"),
                new SelectionChoice("6", "#6b1c52", "IEI", "Industrial Engineering and Informatics")
            ]),
        ];
    }

    public async Task<List<string>> GetFloors()
    {
        await Task.Yield();
        return [.. projects.Select(v => v.Floor).OfType<string>().Distinct().Order()];
    }
}