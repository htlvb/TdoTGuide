namespace TdoTGuide.Admin.Server.Data;

public interface IDepartmentStore
{
    Task<List<Department>> GetDepartments();
}
