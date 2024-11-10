using Dapper;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;
using System.Text.Json;
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
            List<DbStoredProject> projects;
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
            var dbProject = DbNewProject.FromDomain(project);
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            await CreateProject(dbConnection, dbProject);
        }

        public async Task Update(Project project)
        {
            var dbProject = DbNewProject.FromDomain(project);
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            await UpdateProject(dbConnection, dbProject);
        }

        public async Task<List<ISelectionType>> GetProjectTypes()
        {
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            var dbDepartments = await dbConnection.QueryAsync<DbSelectionType>("SELECT id, title, selection_data->>'type' as type, selection_data->>'color' as color, selection_data->'choices' as choices FROM project_type ORDER BY \"order\"");
            return dbDepartments.Select(v => v.ToDomain()).ToList();
        }

        public async Task<List<string>> GetFloors()
        {
            await using var dbConnection = await dataSource.OpenConnectionAsync();
            var floors = await dbConnection.QueryAsync<string>("SELECT DISTINCT(floor) FROM project WHERE floor IS NOT NULL ORDER BY floor");
            return [.. floors];
        }

        public void Dispose()
        {
            dataSource.Dispose();
        }

        private static readonly string readProjectCommand = """
            SELECT
                p.id,
                p.title,
                p.description,
                p.type || json_object('title': pt.title)::jsonb || pt.selection_data AS type,
                p.building,
                p.floor,
                p.location,
                p.organizer,
                p.co_organizers,
                p.show_in_print_overview,
                p.print_overview_group_name
            FROM project p
            JOIN project_type pt on p.type->>'name' = pt.id
            """;

        private static async IAsyncEnumerable<DbStoredProject> ReadAllProjects(NpgsqlConnection dbConnection)
        {
            using var cmd = new NpgsqlCommand(readProjectCommand, dbConnection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return DbStoredProject.FromReader(reader);
            }
        }

        private static async Task<DbStoredProject?> ReadProject(NpgsqlConnection dbConnection, Guid projectGuid)
        {
            using var cmd = new NpgsqlCommand($"{readProjectCommand} WHERE p.id = @projectId", dbConnection);
            cmd.Parameters.AddWithValue("projectId", projectGuid);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }
            return DbStoredProject.FromReader(reader);
        }

        private static async Task CreateProject(NpgsqlConnection dbConnection, DbNewProject project)
        {
            using var cmd = new NpgsqlCommand("INSERT INTO project (id, title, description, type, building, floor, location, organizer, co_organizers) VALUES (@id, @title, @description, @type, @building, @floor, @location, @organizer, @co_organizers)", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("type", NpgsqlDbType.Jsonb, project.Type);
            cmd.Parameters.AddWithValue("building", project.Building);
            cmd.Parameters.AddWithValue("floor", (object?)project.Floor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task UpdateProject(NpgsqlConnection dbConnection, DbNewProject project)
        {
            using var cmd = new NpgsqlCommand("UPDATE project SET title=@title, description=@description, type=@type, building=@building, floor=@floor, location=@location, organizer=@organizer, co_organizers=@co_organizers WHERE id=@id", dbConnection);
            cmd.Parameters.AddWithValue("id", project.Id);
            cmd.Parameters.AddWithValue("title", project.Title);
            cmd.Parameters.AddWithValue("description", project.Description);
            cmd.Parameters.AddWithValue("type", NpgsqlDbType.Jsonb, project.Type);
            cmd.Parameters.AddWithValue("building", project.Building);
            cmd.Parameters.AddWithValue("floor", (object?)project.Floor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("location", project.Location);
            cmd.Parameters.AddWithValue("organizer", NpgsqlDbType.Json, project.Organizer);
            cmd.Parameters.AddWithValue("co_organizers", NpgsqlDbType.Json, project.CoOrganizers);
            await cmd.ExecuteNonQueryAsync();
        }

        private record DbSelection(
#pragma warning disable IDE1006 // Naming Styles
            string type,
            string name,
            string title,
            string? color, // if type == 'simple'
            int[]? selected_values, // if type == 'multi-select'
            DbSelectionChoice[]? choices
#pragma warning restore IDE1006 // Naming Styles
        )
        {
            public ISelection ToDomain()
            {
                if (type == "simple")
                {
                    return new SimpleSelection(name);
                }
                else if (type == "multi-select")
                {
                    return new MultiSelectSelection(name, [.. selected_values!.Select(v => $"{v}")]);
                }
                throw new Exception($"Unknown selection type: \"{type}\"");
            }
        }

        private record DbSelectionReference(
#pragma warning disable IDE1006 // Naming Styles
            string name,
            [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]int[]? selected_values
#pragma warning restore IDE1006 // Naming Styles
        )
        {
            public static DbSelectionReference FromDomain(ISelection projectType)
            {
                return projectType.Accept(new AnonymousSelectionVisitor<DbSelectionReference>(
                    (SimpleSelection selection) => new DbSelectionReference(selection.Name, null),
                    (MultiSelectSelection selection) => new DbSelectionReference(selection.Name, [.. selection.SelectedValues.Select(int.Parse)])
                ));
            }
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

        private record DbStoredProject(
            Guid Id,
            string Title,
            string Description,
            DbSelection Type,
            int Building,
            string? Floor,
            string Location,
            DbProjectOrganizer Organizer,
            IReadOnlyCollection<DbProjectOrganizer> CoOrganizers,
            bool ShowInPrintOverview,
            string? PrintOverviewGroupName
        )
        {
            public static DbStoredProject FromReader(NpgsqlDataReader reader)
            {
                return new DbStoredProject(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetFieldValue<DbSelection>(3),
                    reader.GetInt32(4),
                    !reader.IsDBNull(5) ? reader.GetString(5) : null,
                    reader.GetString(6),
                    reader.GetFieldValue<DbProjectOrganizer>(7),
                    reader.GetFieldValue<DbProjectOrganizer[]>(8),
                    reader.GetBoolean(9),
                    !reader.IsDBNull(10) ? reader.GetString(10) : null
                );
            }

            public Project ToDomain()
            {
                return new Project(
                    Id.ToString(),
                    Title,
                    Description,
                    Type.ToDomain(),
                    $"{Building}",
                    Floor,
                    Location,
                    Organizer.ToDomain(),
                    CoOrganizers.Select(v => v.ToDomain()).ToList(),
                    ShowInPrintOverview,
                    PrintOverviewGroupName
                );
            }
        }

        private record DbNewProject(
            Guid Id,
            string Title,
            string Description,
            DbSelectionReference Type,
            int Building,
            string? Floor,
            string Location,
            DbProjectOrganizer Organizer,
            IReadOnlyCollection<DbProjectOrganizer> CoOrganizers
        )
        {
            public static DbNewProject FromDomain(Project project)
            {
                return new DbNewProject(
                    Guid.Parse(project.Id),
                    project.Title,
                    project.Description,
                    DbSelectionReference.FromDomain(project.Type),
                    int.Parse(project.Building),
                    project.Floor,
                    project.Location,
                    DbProjectOrganizer.FromDomain(project.Organizer),
                    project.CoOrganizers.Select(DbProjectOrganizer.FromDomain).ToList()
                );
            }
        }

        private record DbSelectionType(
#pragma warning disable IDE1006 // Naming Styles
            string id,
            string title,
            string type,
            string color, // if type == 'simple'
            string choices // if type == 'multi-select'
#pragma warning restore IDE1006 // Naming Styles
            )
        {
            public ISelectionType ToDomain()
            {
                if (type == "simple")
                {
                    return new SimpleSelectionType(id, title, color!);
                }
                else if (type == "multi-select")
                {
                    return new MultiSelectSelectionType(id, title, [.. JsonSerializer.Deserialize<DbSelectionChoice[]>(choices!)!.Select(v => v.ToDomain())]);
                }
                else
                {
                    throw new Exception($"Unknown selection type: \"{type}\"");
                }
            }
        }

        private record DbSelectionChoice(
#pragma warning disable IDE1006 // Naming Styles
            int id,
            string color,
            string short_name,
            string long_name
#pragma warning restore IDE1006 // Naming Styles
        )
        {
            public SelectionChoice ToDomain()
            {
                return new SelectionChoice($"{id}", color, short_name, long_name);
            }
        }
    }
}
