namespace TdoTGuide.Server.Common
{
    public interface IUserStore
    {
        IAsyncEnumerable<ProjectOrganizer> GetOrganizerCandidates();
    }
}