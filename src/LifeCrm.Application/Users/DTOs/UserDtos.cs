using LifeCrm.Core.Enums;

namespace LifeCrm.Application.Users.DTOs
{
    public record UserSummaryDto
    {
        public Guid            Id          { get; init; }
        public string          FullName    { get; init; } = string.Empty;
        public string          Email       { get; init; } = string.Empty;
        public UserRole        Role        { get; init; }
        public bool            IsActive    { get; init; }
        public DateTimeOffset? LastLoginAt { get; init; }
        public DateTimeOffset  CreatedAt   { get; init; }
    }

    public record CreateUserRequest
    {
        public string   FullName { get; init; } = string.Empty;
        public string   Email    { get; init; } = string.Empty;
        public string   Password { get; init; } = string.Empty;
        public UserRole Role     { get; init; } = UserRole.Manager;
    }
}
