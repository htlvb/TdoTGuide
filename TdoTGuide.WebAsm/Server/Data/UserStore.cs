using Microsoft.Graph;
using System.Text.RegularExpressions;

namespace TdoTGuide.WebAsm.Server.Data
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
            var userPageRequest = graphServiceClient.Groups[organizerGroupId].Members.Request().Top(999);
            while (userPageRequest != null)
            {
                var userPage = await userPageRequest.GetAsync();
                var users = userPage
                    .OfType<User>()
                    .Select(v => new ProjectOrganizer(v.Id, v.GivenName, v.Surname, Regex.Replace(v.UserPrincipalName, "@.*$", "")));
                foreach (var user in users)
                {
                    yield return user;
                }

                userPageRequest = userPage.NextPageRequest;
            }
        }
    }
}
