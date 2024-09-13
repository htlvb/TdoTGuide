namespace TdoTGuide.Server.Common;

public interface IBuildingStore
{
    Task<List<Building>> GetBuildings();
}