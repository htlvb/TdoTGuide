namespace TdoTGuide.WebAsm.Server.Data
{
    public interface IUserStore
    {
        IAsyncEnumerable<ProjectOrganizer> GetOrganizerCandidates();
    }
}