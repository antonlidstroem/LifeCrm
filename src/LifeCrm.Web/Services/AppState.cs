
using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using LifeCrm.Application.Common.DTOs;
using LifeCrm.Core.Constants;

namespace LifeCrm.Web.Services
{
    /// <summary>
    /// Holds the current authenticated user's session state.
    /// FIXED: Restores state from localStorage on startup so page refresh doesn't log the user out.
    /// </summary>
    public class AppState
    {
        private LoginResponse? _login;
        private const string StorageKey = "lifecrm_user";

        public string? UserId           => _login?.UserId;
        public string? UserFullName     => _login?.FullName;
        public string? UserEmail        => _login?.Email;
        public string? UserRole         => _login?.Role;
        public string? OrganizationId   => _login?.OrganizationId;
        public string? OrganizationName => _login?.OrganizationName;
        public string? Token            => _login?.Token;
        public bool    IsAuthenticated  => _login is not null && !IsTokenExpired(_login.Token);

        public bool IsAdmin          => UserRole == Roles.Admin;
        public bool IsFinanceOrAdmin => UserRole is Roles.Admin or Roles.Finance;
        public bool CanWrite         => UserRole is Roles.Admin or Roles.Finance or Roles.Manager;

        public event Action? StateChanged;

        public void SetUser(LoginResponse login)
        {
            _login = login;
            StateChanged?.Invoke();
        }

        public void ClearUser()
        {
            _login = null;
            StateChanged?.Invoke();
        }

        /// <summary>
        /// Restores session from localStorage on app startup (survives page refresh).
        /// Called once from a top-level component's OnInitializedAsync.
        /// </summary>
        public async Task TryRestoreAsync(ILocalStorageService storage)
        {
            if (_login is not null) return; // already set (e.g. just logged in)

            try
            {
                var stored = await storage.GetItemAsync<LoginResponse>(StorageKey);
                if (stored is not null && !IsTokenExpired(stored.Token))
                {
                    _login = stored;
                    StateChanged?.Invoke();
                }
                else if (stored is not null)
                {
                    // Token expired — clean up
                    await storage.RemoveItemAsync(StorageKey);
                }
            }
            catch
            {
                // localStorage unavailable or corrupt — stay logged out
            }
        }

        /// <summary>Persists the user session to localStorage.</summary>
        public async Task PersistAsync(ILocalStorageService storage)
        {
            if (_login is null) return;
            try { await storage.SetItemAsync(StorageKey, _login); }
            catch { /* non-fatal */ }
        }

        /// <summary>Clears the persisted session from localStorage.</summary>
        public async Task ClearPersistedAsync(ILocalStorageService storage)
        {
            try { await storage.RemoveItemAsync(StorageKey); }
            catch { /* non-fatal */ }
        }

        private static bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt     = handler.ReadJwtToken(token);
                // Add 30s buffer so we don't use a token that expires mid-request
                return jwt.ValidTo.ToUniversalTime() < DateTime.UtcNow.AddSeconds(30);
            }
            catch { return true; }
        }
    }
}
