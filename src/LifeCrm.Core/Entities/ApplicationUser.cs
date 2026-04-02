using LifeCrm.Core.Enums;

namespace LifeCrm.Core.Entities
{
    public class ApplicationUser : TenantEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Viewer;
        public bool IsActive { get; set; } = true;
        public DateTimeOffset? LastLoginAt { get; set; }
        public string CreatedBy { get; set; } = "system";
        public Organization? Organization { get; set; }
    }
}
