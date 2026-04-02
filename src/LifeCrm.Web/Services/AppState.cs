using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Constants;

namespace LifeCrm.Web.Services
{
    public class AppState
    {
        private LoginResponse? _login;

        public string? UserId           => _login?.UserId;
        public string? UserFullName     => _login?.FullName;
        public string? UserEmail        => _login?.Email;
        public string? UserRole         => _login?.Role;
        public string? OrganizationId   => _login?.OrganizationId;
        public string? OrganizationName => _login?.OrganizationName;
        public bool    IsAuthenticated  => _login is not null;

        public bool IsAdmin          => UserRole == Roles.Admin;
        public bool IsFinanceOrAdmin => UserRole is Roles.Admin or Roles.Finance;
        public bool CanWrite         => UserRole is Roles.Admin or Roles.Finance or Roles.Manager;

        public event Action? StateChanged;

        public void SetUser(LoginResponse login) { _login = login; StateChanged?.Invoke(); }
        public void ClearUser()                   { _login = null;  StateChanged?.Invoke(); }
    }
}
