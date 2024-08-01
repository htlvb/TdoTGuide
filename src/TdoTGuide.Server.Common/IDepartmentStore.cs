namespace TdoTGuide.Server.Common;

public interface IDepartmentStore
{
    Task<List<Department>> GetDepartments();
}
