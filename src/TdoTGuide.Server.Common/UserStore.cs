using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TdoTGuide.Server.Common
{
    public class UserStore : IUserStore
    {
        private readonly string organizerGroupId;
        private readonly GraphServiceClient graphServiceClient;

        public UserStore(
            string organizerGroupId,
            GraphServiceClient graphServiceClient)
        {
            this.organizerGroupId = organizerGroupId;
            this.graphServiceClient = graphServiceClient;
        }

        public async IAsyncEnumerable<ProjectOrganizer> GetOrganizerCandidates()
        {
            var groupMembers = graphServiceClient.ReadAll<User, UserCollectionResponse>(
                graphServiceClient.Groups[organizerGroupId].Members.GraphUser.GetAsync(v => v.QueryParameters.Top = 999)
            );
            await foreach (var user in groupMembers)
            {
                Debug.Assert(user.Id != null && user.GivenName != null && user.Surname != null && user.UserPrincipalName != null);
                yield return new ProjectOrganizer(user.Id, user.GivenName, user.Surname, Regex.Replace(user.UserPrincipalName, "@.*$", ""));
            }
        }
    }
}
