using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

public class InMemoryBuildingStore : IBuildingStore
{
    public async Task<List<Building>> GetBuildings()
    {
        await Task.Yield();
        return [
            new("1", "Theorie"),
            new("2", "Labor"),
            new("3", "Werkstätte"),
        ];
    }
}
