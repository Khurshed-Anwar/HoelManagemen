using HotelManagement.Models.Admin;

namespace HotelManagement.Services.Admin
{
    public interface IDepartmentService
    {
        Task<IList<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);

        // Creates department + auto-creates {Name}Admin and {Name}Reader roles
        Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);

        Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto);

        // Soft delete — sets IsDeleted = true
        Task<bool> DeleteAsync(int id);

        // Returns the Admin and Reader roles for a department
        Task<IList<DepartmentRoleDto>> GetRolesAsync(int id);
    }
}
