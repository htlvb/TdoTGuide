using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text.Json.Serialization;
using TdoTGuide.Server.Common;

namespace TdoTGuide.Server.Common
{
    public class PgsqlProjectStore : IProjectStore, IDisposable
    {
        private readonly NpgsqlDataSource dataSource;

        public PgsqlProjectStore(PgsqlConnectionString dbConnectionString)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConnectionString.Value);
            dataSourceBuilder.EnableDynamicJson();
            dataSource = dataSourceBuilder.Build();
        }

        public async IAsyncEnumerable<Project> GetAll()
        {
            List<DbProject> projects;
            await using (var dbConnection = await dataSource.OpenConnectionAsync())
            {
                projects = await ReadAllProjects(dbConnection).ToList();
            }

            foreach (var project in projects)
            {
                yield return project.ToDomain();
            }
        }

        public async Task<Project?> Get(string projectId)
        {
            await using var dbConnection = await dataSource.OpenConnectionAsync();

            if (!Guid.TryParse(projectId, out var projectGuid))
            {
                return null;
            }
            var dbProject = await ReadProject(dbConnection, projectGuid);
            if (dbProject == null)
            {
                return null;
            }
            return dbProject.ToDomain();
        }

        public async IAsyncEnumerable<string> GetProjectGroups()
        {
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            var groups = await dbConnection.QueryAsync<string>("SELECT DISTINCT \"group\" FROM project WHERE \"group\" IS NOT NULL ORDER BY \"group\"");
            foreach (var group in groups)
            {
                yield return group;
            }
        }

        public async Task Delete(string projectId)
        {
            if (!Guid.TryParse(projectId, out var projectGuid))
            {
                return;
            }
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM project WHERE id = @id", dbConnection);
            cmd.Parameters.AddWithValue("id", projectGuid);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Create(Project project)
        {
            var dbProject = DbProject.FromDomain(project);
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            await CreateProject(dbConnection, dbProject);
        }

        public async Task Update(Project project)
        {
            var dbProject = DbProject.FromDomain(project);
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            await UpdateProject(dbConnection, dbProject);
        }

        public void Dispose()
        {
            dataSource.Dispose();
        }

        private static async IAsyncEnumerable<DbProject> ReadAllProjects(NpgsqlConnection dbConnection)
        {
            using var cmd = new NpgsqlCommand("SELECT id, title, description, \"group\", departments, building, location, organizer, co_organizers FROM project", dbConnection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return DbProject.FromReader(reader);
            }
        }

        private static async Task<DbProject?> ReadProject(NpgsqlConnection dbConnection, Guid projectGuid)
        {
            using var cmd = new NpgsqlCommand("SELECT id, title, description, \"group\", departments, building, location, organizer, co_organizers FROM project WHERE id = @projectId", dbConnection);
            cmd.Parameters.AddWithValue("projectId", projectGuid);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }
            return DbProject.FromReader(reader);
        }

        private static async Task CreateProject(NpgsqlConnection dbConnection, DbProject project)
        {
            using var cmd = new NpgsqlCommand("INSERT INTO project (id, title, description, \"group\", departments, building, location, organizer, co_organizers) VALUES (@id, @title, @description, @group, @departments, @building, @location, @organizer, @co_organizers)", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("group", (object?)project.Group ?? DBNull.Value);
            cmd.Parameters.AddWithValue("departments", NpgsqlDbType.Json, project.Departments);
            cmd.Parameters.AddWithValue("building", project.Building);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task UpdateProject(NpgsqlConnection dbConnection, DbProject project)
        {
            using var cmd = new NpgsqlCommand("UPDATE project SET title=@title, description=@description, \"group\"=@group, departments=@departments, building=@building, location=@location, organizer=@organizer, co_organizers=@co_organizers WHERE id=@id", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("group", (object?)project.Group ?? DBNull.Value);
            cmd.Parameters.AddWithValue("departments", NpgsqlDbType.Json, project.Departments);
            cmd.Parameters.AddWithValue("building", project.Building);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            await cmd.ExecuteNonQueryAsync();
        }

        private record DbProjectOrganizer(
            [property: JsonPropertyName("id")] Guid Id,
            [property: JsonPropertyName("first_name")] string FirstName,
            [property: JsonPropertyName("last_name")] string LastName,
            [property: JsonPropertyName("short_name")] string ShortName
        )
        {
            public ProjectOrganizer ToDomain()
            {
                return new ProjectOrganizer
                (
                    Id.ToString(),
                    FirstName,
                    LastName,
                    ShortName
                );
            }

            public static DbProjectOrganizer FromDomain(ProjectOrganizer organizer)
            {
                return new DbProjectOrganizer
                (
                    Guid.Parse(organizer.Id),
                    organizer.FirstName,
                    organizer.LastName,
                    organizer.ShortName
                );
            }
        }

        private record DbProject(
            Guid Id,
            string Title,
            string Description,
            string? Group,
            IReadOnlyCollection<int> Departments,
            int Building,
            string Location,
            DbProjectOrganizer Organizer,
            IReadOnlyCollection<DbProjectOrganizer> CoOrganizers
        )
        {
            public static DbProject FromReader(NpgsqlDataReader reader)
            {
                return new DbProject(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3),
                    reader.GetFieldValue<int[]>(4),
                    reader.GetInt32(5),
                    reader.GetString(6),
                    reader.GetFieldValue<DbProjectOrganizer>(7),
                    reader.GetFieldValue<DbProjectOrganizer[]>(8)
                );
            }

            public static DbProject FromDomain(Project project)
            {
                return new DbProject(
                    Guid.Parse(project.Id),
                    project.Title,
                    project.Description,
                    project.Group,
                    [.. project.Departments.Select(int.Parse)],
                    int.Parse(project.Building),
                    project.Location,
                    DbProjectOrganizer.FromDomain(project.Organizer),
                    project.CoOrganizers.Select(DbProjectOrganizer.FromDomain).ToList()
                );
            }

            public Project ToDomain()
            {
                return new Project(
                    Id.ToString(),
                    Title,
                    Description,
                    Group,
                    [.. Departments.Select(v => $"{v}")],
                    $"{Building}",
                    Location,
                    Organizer.ToDomain(),
                    CoOrganizers.Select(v => v.ToDomain()).ToList()
                );
            }
        }
    }
}
