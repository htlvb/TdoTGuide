namespace TdoTGuide.Server.Common
{
    public interface IProjectStore
    {
        IAsyncEnumerable<Project> GetAll();
        Task<Project?> Get(string projectId);
        IAsyncEnumerable<string> GetProjectGroups();
        Task Delete(string projectId);
        Task Create(Project project);
        Task Update(Project project);
    }
}