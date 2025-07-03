using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PiaWebApp.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<CustomAuthStateProvider> _logger;

        public CustomAuthStateProvider(IJSRuntime jsRuntime, ILogger<CustomAuthStateProvider> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Try to get authentication data from session storage
                var storeCode = await GetFromSessionStorage("storeCode");
                var storeDesc = await GetFromSessionStorage("storeDesc");
                var authTime = await GetFromSessionStorage("authTime");

                // If no store code exists, return unauthenticated state
                if (string.IsNullOrEmpty(storeCode))
                {
                    _logger.LogInformation("No authentication token found in session storage");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Validate the authentication hasn't expired (optional)
                if (!string.IsNullOrEmpty(authTime) && DateTime.TryParse(authTime, out var authDateTime))
                {
                    if ((DateTime.UtcNow - authDateTime) > TimeSpan.FromHours(12))
                    {
                        _logger.LogWarning("Authentication token expired");
                        await ClearSessionStorage();
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    }
                }

                // Create claims identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, storeCode),
                    new Claim("StoreCode", storeCode),
                    new Claim("StoreDescription", storeDesc ?? string.Empty)
                };

                var identity = new ClaimsIdentity(claims, "customAuth");
                var user = new ClaimsPrincipal(identity);

                _logger.LogInformation($"Successfully authenticated store: {storeCode}");
                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task MarkUserAsAuthenticated(string storeCode, string storeDesc)
        {
            try
            {
                // Store authentication data in session storage
                await SetToSessionStorage("storeCode", storeCode);
                await SetToSessionStorage("storeDesc", storeDesc);
                await SetToSessionStorage("authTime", DateTime.UtcNow.ToString("o"));

                // Get the new authentication state
                var authState = await GetAuthenticationStateAsync();

                // Notify that authentication state has changed
                NotifyAuthenticationStateChanged(Task.FromResult(authState));

                _logger.LogInformation($"User authenticated: {storeCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking user as authenticated");
                throw;
            }
        }

        public async Task MarkUserAsLoggedOut()
        {
            try
            {
                await ClearSessionStorage();
                NotifyAuthenticationStateChanged(
                    Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));

                _logger.LogInformation("User logged out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking user as logged out");
                throw;
            }
        }

        private async Task<string?> GetFromSessionStorage(string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {key} from session storage");
                return null;
            }
        }

        private async Task SetToSessionStorage(string key, string value)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting {key} in session storage");
                throw;
            }
        }

        private async Task ClearSessionStorage()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("sessionStorage.clear");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing session storage");
                throw;
            }
        }
    }
}