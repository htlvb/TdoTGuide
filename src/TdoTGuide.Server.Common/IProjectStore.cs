﻿namespace TdoTGuide.Server.Common
{
    public interface IProjectStore
    {
        IAsyncEnumerable<Project> GetAll();
        Task<Project?> Get(string projectId);
        Task Delete(string projectId);
        Task Create(Project project);
        Task Update(Project project);
        Task<List<ISelectionType>> GetProjectTypes();
        Task<List<string>> GetFloors();
    }
}