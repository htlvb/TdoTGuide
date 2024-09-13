using Dapper;
using Npgsql;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Server.Common;

public class PgsqlDepartmentStore : IDepartmentStore, IDisposable
{
    private readonly NpgsqlDataSource dataSource;

    public PgsqlDepartmentStore(PgsqlConnectionString dbConnectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConnectionString.Value);
        dataSourceBuilder.EnableDynamicJson();
        dataSource = dataSourceBuilder.Build();
    }

    public async Task<List<Department>> GetDepartments()
    {
        await using var dbConnection = await dataSource.OpenConnectionAsync();
        var dbDepartments = await dbConnection.QueryAsync<DbDepartment>("SELECT id, name, long_name longName, color FROM department ORDER BY name");
        return dbDepartments.Select(v => v.ToDomain()).ToList();
    }

    public void Dispose()
    {
        dataSource.Dispose();
    }

    private class DbDepartment
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string LongName { get; set; }
        public required string Color { get; set; }

        public Department ToDomain()
        {
            return new(Id, Name, LongName, Color);
        }
    }
}
