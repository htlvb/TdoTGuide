using Dapper;
using Npgsql;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Server.Common;

public class PgsqlBuildingStore : IBuildingStore, IDisposable
{
    private readonly NpgsqlDataSource dataSource;

    public PgsqlBuildingStore(PgsqlConnectionString dbConnectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConnectionString.Value);
        dataSourceBuilder.EnableDynamicJson();
        dataSource = dataSourceBuilder.Build();
    }

    public async Task<List<Building>> GetBuildings()
    {
        await using var dbConnection = await dataSource.OpenConnectionAsync();
        var dbBuildings = await dbConnection.QueryAsync<DbBuilding>("SELECT id, name FROM building");
        return dbBuildings.Select(v => v.ToDomain()).ToList();
    }

    public void Dispose()
    {
        dataSource.Dispose();
    }

    private class DbBuilding
    {
        public required string Id { get; set; }
        public required string Name { get; set; }

        public Building ToDomain()
        {
            return new(Id, Name);
        }
    }
}
