using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server
{
    public class KeycloakUserStore(
        string realmName,
        string organizerGroupId,
        KeycloakAdminApiClientFactory keycloakAdminApiClientFactory) : IUserStore
    {
        public async IAsyncEnumerable<ProjectOrganizer> GetOrganizerCandidates()
        {
            var keycloakAdminApiClient = keycloakAdminApiClientFactory.CreateClient();
            var users = await keycloakAdminApiClient.Admin
                .Realms[realmName]
                .Groups[organizerGroupId]
                .Members
                .GetAsync(v => v.QueryParameters.Max = -1);
            if (users == null)
            {
                yield break;
            }
            var organizers = users
                .Select(v =>
                {
                    if (v.Id == null || v.Username == null)
                    {
                        return null;
                    }

                    return new ProjectOrganizer(
                        v.Id,
                        v.FirstName ?? "",
                        v.LastName ?? "",
                        v.Username.ToUpper()
                    );
                })
                .OfType<ProjectOrganizer>();
            foreach (var organizer in organizers)
            {
                yield return organizer;
            }
        }
    }
}
