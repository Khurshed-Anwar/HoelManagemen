namespace HotelManagement.Models.Admin
{
    public class UserDto
    {
        public string Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsLockedOut { get; set; }
        public IList<string> Roles { get; set; } = [];
    }

    public class UserDetailDto : UserDto
    {
        public IList<string> Permissions { get; set; } = [];
        public IList<string> DirectPermissions { get; set; } = [];
    }

    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class AssignRolesDto
    {
        public IList<string> RoleNames { get; set; } = [];
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetPasswordDto
    {
        public string NewPassword { get; set; }
    }
}
