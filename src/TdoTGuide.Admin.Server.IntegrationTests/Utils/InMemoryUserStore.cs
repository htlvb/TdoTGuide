using TdoTGuide.Admin.Server.Data;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

public class InMemoryUserStore : IUserStore
{
    private readonly IReadOnlyCollection<ProjectOrganizer> organizerCandidates;

    public InMemoryUserStore(IReadOnlyCollection<ProjectOrganizer> organizerCandidates)
    {
        this.organizerCandidates = organizerCandidates;
    }

    public async IAsyncEnumerable<ProjectOrganizer> GetOrganizerCandidates()
    {
        foreach (var item in organizerCandidates)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
