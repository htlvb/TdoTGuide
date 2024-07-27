namespace TdoTGuide.WebAsm.Server.Data;

public interface IDepartmentStore
{
    Task<List<Department>> GetDepartments();
}
