namespace TdoTGuide.Admin.Server.Data
{
    public interface IUserStore
    {
        IAsyncEnumerable<ProjectOrganizer> GetOrganizerCandidates();
    }
}