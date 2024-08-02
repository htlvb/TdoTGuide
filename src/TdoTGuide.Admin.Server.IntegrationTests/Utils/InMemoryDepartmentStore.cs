using TdoTGuide.Server.Common;

namespace TdoTGuide.Admin.Server.IntegrationTests.Utils;

public class InMemoryDepartmentStore : IDepartmentStore
{
    public async Task<List<Department>> GetDepartments()
    {
        await Task.Yield();
        return [
            new("1", "MB", "Maschinenbau - Anlagentechnik", "#183c7b"),
            new("2", "ME", "Mechatronik", "#ad1410"),
            new("3", "FS", "Fachschule Maschinenbau", "#009ec6"),
            new("4", "GT", "Gebäudetechnik", "#008040"),
            new("5", "IEM", "Industrial Engineering and Management", "#e78a00"),
            new("6", "IEI", "Industrial Engineering and Informatics", "#6b1c52")
        ];
    }
}
