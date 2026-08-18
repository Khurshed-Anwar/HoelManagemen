using HotelManagement.Models.Admin;

namespace HotelManagement.Services.Admin
{
    public interface IUserManagementService
    {
        // Returns all users with their roles
        Task<IList<UserDto>> GetAllUsersAsync();

        // Returns full user detail including permissions
        Task<UserDetailDto?> GetUserByIdAsync(string userId);

        // Updates FirstName, LastName, Email
        Task<UserDto?> UpdateUserAsync(string userId, UpdateUserDto dto);

        // Replaces all roles for a user
        Task AssignRolesAsync(string userId, AssignRolesDto dto);

        // Locks the user account indefinitely
        Task<bool> LockUserAsync(string userId);

        // Unlocks the user account
        Task<bool> UnlockUserAsync(string userId);

        // Admin reset — no current-password check
        Task<bool> ResetPasswordAsync(string userId, string newPassword);

        // Deletes the user entirely
        Task<bool> DeleteUserAsync(string userId);
    }
}
