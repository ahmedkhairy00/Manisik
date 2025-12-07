using System.ComponentModel.DataAnnotations;

namespace UmarahBooking.Core.DTO
{
    /// <summary>
    /// DTO for assigning/removing roles
    /// </summary>
    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty;
    }
}
