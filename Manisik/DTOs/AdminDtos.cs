namespace Manisik.DTOs
{
    public class RoleDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class AssignRoleDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
