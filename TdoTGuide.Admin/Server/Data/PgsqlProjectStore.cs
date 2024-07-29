using System.Data;
using System.Text.Json.Serialization;
using Npgsql;
using NpgsqlTypes;

namespace TdoTGuide.Admin.Server.Data
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
            using var cmd = new NpgsqlCommand("SELECT id, title, description, departments, location, organizer, co_organizers, time_selection FROM project", dbConnection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return DbProject.FromReader(reader);
            }
        }

        private static async Task<DbProject?> ReadProject(NpgsqlConnection dbConnection, Guid projectGuid)
        {
            using var cmd = new NpgsqlCommand("SELECT id, title, description, departments, location, organizer, co_organizers, time_selection FROM project WHERE id = @projectId", dbConnection);
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
            using var cmd = new NpgsqlCommand("INSERT INTO project (id, title, description, departments, location, organizer, co_organizers, time_selection) VALUES (@id, @title, @description, @departments, @location, @organizer, @co_organizers, @time_selection)", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("departments", NpgsqlDbType.Json, project.Departments);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            cmd.Parameters.AddWithValue("time_selection", NpgsqlDbType.Json, project.TimeSelection);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task UpdateProject(NpgsqlConnection dbConnection, DbProject project)
        {
            using var cmd = new NpgsqlCommand("UPDATE project SET title=@title, description=@description, departments=@departments, location=@location, organizer=@organizer, co_organizers=@co_organizers, time_selection=@time_selection WHERE id=@id", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("departments", NpgsqlDbType.Json, project.Departments);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            cmd.Parameters.AddWithValue("time_selection", NpgsqlDbType.Json, project.TimeSelection);
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

        [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
        [JsonDerivedType(typeof(DbContinuousTimeSelection), typeDiscriminator: "continuous")]
        [JsonDerivedType(typeof(DbRegularTimeSelection), typeDiscriminator: "regular")]
        [JsonDerivedType(typeof(DbIndividualTimeSelection), typeDiscriminator: "individual")]
        private abstract class DbTimeSelection
        {
            public abstract ITimeSelection ToDomain();

            public static DbTimeSelection FromDomain(ITimeSelection timeSelection)
            {
                return timeSelection.Accept(new TimeSelectionToDbModelVisitor());
            }

            private class TimeSelectionToDbModelVisitor : ITimeSelectionVisitor<DbTimeSelection>
            {
                public DbTimeSelection VisitContinuousTimeSelection(ContinuousTimeSelection v)
                {
                    return new DbContinuousTimeSelection();
                }

                public DbTimeSelection VisitRegularTimeSelection(RegularTimeSelection v)
                {
                    return new DbRegularTimeSelection(v.IntervalMinutes);
                }

                public DbTimeSelection VisitIndividualTimeSelection(IndividualTimeSelection v)
                {
                    return new DbIndividualTimeSelection([.. v.Times]);
                }
            }
        }
        private class DbContinuousTimeSelection() : DbTimeSelection
        {
            public override ITimeSelection ToDomain()
            {
                return new ContinuousTimeSelection();
            }
        }
        private class DbRegularTimeSelection(int intervalMinutes) : DbTimeSelection
        {
            [JsonPropertyName("interval_minutes")] public int IntervalMinutes { get; } = intervalMinutes;

            public override ITimeSelection ToDomain()
            {
                return new RegularTimeSelection(IntervalMinutes);
            }
        }
        private class DbIndividualTimeSelection(List<DateTime> times) : DbTimeSelection
        {
            [JsonPropertyName("times")] public List<DateTime> Times { get; } = times;

            public override ITimeSelection ToDomain()
            {
                return new IndividualTimeSelection(Times);
            }
        }

        private record DbProject(
            Guid Id,
            string Title,
            string Description,
            IReadOnlyCollection<int> Departments,
            string Location,
            DbProjectOrganizer Organizer,
            IReadOnlyCollection<DbProjectOrganizer> CoOrganizers,
            DbTimeSelection TimeSelection
        )
        {
            public static DbProject FromReader(NpgsqlDataReader reader)
            {
                return new DbProject(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetFieldValue<int[]>(3),
                    reader.GetString(4),
                    reader.GetFieldValue<DbProjectOrganizer>(5),
                    reader.GetFieldValue<DbProjectOrganizer[]>(6),
                    reader.GetFieldValue<DbTimeSelection>(7)
                );
            }

            public static DbProject FromDomain(Project project)
            {
                return new DbProject(
                    Guid.Parse(project.Id),
                    project.Title,
                    project.Description,
                    [.. project.Departments.Select(int.Parse)],
                    project.Location,
                    DbProjectOrganizer.FromDomain(project.Organizer),
                    project.CoOrganizers.Select(DbProjectOrganizer.FromDomain).ToList(),
                    DbTimeSelection.FromDomain(project.TimeSelection)
                );
            }

            public Project ToDomain()
            {
                return new Project(
                    Id.ToString(),
                    Title,
                    Description,
                    [.. Departments.Select(v => $"{v}")],
                    Location,
                    Organizer.ToDomain(),
                    CoOrganizers.Select(v => v.ToDomain()).ToList(),
                    TimeSelection.ToDomain()
                );
            }
        }
    }
}
