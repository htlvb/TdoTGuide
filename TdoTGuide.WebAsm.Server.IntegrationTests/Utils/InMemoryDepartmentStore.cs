using TdoTGuide.WebAsm.Server.Data;

namespace TdoTGuide.WebAsm.Server.IntegrationTests.Utils;

public class InMemoryDepartmentStore : IDepartmentStore
{
    public async Task<List<Department>> GetDepartments()
    {
        await Task.Yield();
        return [
            new("1", "MB", "#183c7b"),
            new("2", "ME", "#ad1410"),
            new("3", "FS", "#009ec6"),
            new("4", "GT", "#008040"),
            new("5", "IEM", "#e78a00"),
            new("6", "IEI", "#6b1c52")
        ];
    }
}
