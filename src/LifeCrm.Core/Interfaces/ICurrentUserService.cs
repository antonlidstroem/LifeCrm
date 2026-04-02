namespace LifeCrm.Core.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        Guid? OrganizationId { get; }
        string? UserRole { get; }
        bool IsAuthenticated { get; }
    }
}
