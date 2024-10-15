using TdoTGuide.Admin.Server.IntegrationTests.Utils;
using TdoTGuide.Server.Common;
using Testcontainers.PostgreSql;

namespace TdoTGuide.Admin.Server.IntegrationTests;

public class PgsqlProjectStoreTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer pgsqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithBindMount(Path.GetFullPath("db-schema.sql"), "/docker-entrypoint-initdb.d/01-schema.sql")
        .Build();

    public Task InitializeAsync()
    {
        return pgsqlContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return pgsqlContainer.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task CanStoreProject()
    {
        var projectStore = new PgsqlProjectStore(new PgsqlConnectionString(pgsqlContainer.GetConnectionString()));
        var project = FakeData.ProjectFaker.Generate();
        await projectStore.Create(project);
        var actual = await projectStore.GetAll().ToList();
        Assert.Equal([project], actual, new ProjectEqualityComparer());
    }
}
