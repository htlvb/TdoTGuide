using System.Data;
using System.Text.Json.Serialization;
using Npgsql;
using NpgsqlTypes;

namespace TdoTGuide.WebAsm.Server.Data
{
    public class PgsqlProjectStore : IProjectStore, IDisposable
    {
        private readonly NpgsqlDataSource dataSource;

        public PgsqlProjectStore(string dbConnectionString)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbConnectionString);
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
            using var cmd = new NpgsqlCommand("SELECT id, title, description, location, organizer, co_organizers, date, start_time, end_time FROM project", dbConnection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return DbProject.FromReader(reader);
            }
        }

        private async Task<DbProject?> ReadProject(NpgsqlConnection dbConnection, Guid projectGuid)
        {
            using var cmd = new NpgsqlCommand("SELECT id, title, description, location, organizer, co_organizers, date, start_time, end_time FROM project WHERE id = @projectId", dbConnection);
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
            using var cmd = new NpgsqlCommand("INSERT INTO project (id, title, description, location, organizer, co_organizers, date, start_time, end_time) VALUES (@id, @title, @description, @location, @organizer, @co_organizers, @date, @start_time, @end_time)", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            cmd.Parameters.AddWithValue("date", project.Date);
            cmd.Parameters.AddWithValue("start_time", project.StartTime);
            cmd.Parameters.AddWithValue("end_time", (object?)project.EndTime ?? DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task UpdateProject(NpgsqlConnection dbConnection, DbProject project)
        {
            using var cmd = new NpgsqlCommand("UPDATE project SET title=@title, description=@description, location=@location, organizer=@organizer, co_organizers=@co_organizers, date=@date, start_time=@start_time, end_time=@end_time WHERE id=@id", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            cmd.Parameters.AddWithValue("date", project.Date);
            cmd.Parameters.AddWithValue("start_time", project.StartTime);
            cmd.Parameters.AddWithValue("end_time", (object?)project.EndTime ?? DBNull.Value);
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
            string Location,
            DbProjectOrganizer Organizer,
            IReadOnlyCollection<DbProjectOrganizer> CoOrganizers,
            DateTime Date,
            TimeSpan StartTime,
            TimeSpan? EndTime
        )
        {
            public static DbProject FromReader(NpgsqlDataReader reader)
            {
                return new DbProject(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetFieldValue<DbProjectOrganizer>(4),
                    reader.GetFieldValue<DbProjectOrganizer[]>(5),
                    reader.GetDateTime(6),
                    reader.GetTimeSpan(7),
                    reader.GetFieldValue<TimeSpan?>(8)
                );
            }

            public static DbProject FromDomain(Project project)
            {
                return new DbProject(
                    Guid.Parse(project.Id),
                    project.Title,
                    project.Description,
                    project.Location,
                    DbProjectOrganizer.FromDomain(project.Organizer),
                    project.CoOrganizers.Select(DbProjectOrganizer.FromDomain).ToList(),
                    project.Date.ToDateTime(TimeOnly.MinValue),
                    project.StartTime.ToTimeSpan(),
                    project.EndTime?.ToTimeSpan()
                );
            }

            public Project ToDomain()
            {
                return new Project(
                    Id.ToString(),
                    Title,
                    Description,
                    Location,
                    Organizer.ToDomain(),
                    CoOrganizers.Select(v => v.ToDomain()).ToList(),
                    DateOnly.FromDateTime(Date),
                    TimeOnly.FromTimeSpan(StartTime),
                    EndTime != null ? TimeOnly.FromTimeSpan(EndTime.Value) : null
                );
            }
        }
    }
}
