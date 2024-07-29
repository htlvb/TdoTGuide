using Dapper;
using Npgsql;

namespace TdoTGuide.Admin.Server.Data;

public class PgsqlDepartmentStore : IDepartmentStore, IDisposable
{
    private readonly NpgsqlDataSource dataSource;

    public PgsqlDepartmentStore(string dbConnectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConnectionString);
        dataSourceBuilder.EnableDynamicJson();
        dataSource = dataSourceBuilder.Build();
    }

    public async Task<List<Department>> GetDepartments()
    {
        await using var dbConnection = await dataSource.OpenConnectionAsync();
        var dbDepartments = await dbConnection.QueryAsync<DbDepartment>("SELECT id, name, color FROM department ORDER BY name");
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
        public required string Color { get; set; }

        public Department ToDomain()
        {
            return new(Id, Name, Color);
        }
    }
}
