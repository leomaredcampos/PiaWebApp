using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PiaWebApp.Auth;
using PiaWebApp.Data;
using System;
using System.Threading.Tasks;

namespace PiaWebApp.Pages.Login
{
    public class LoginViewModel // Removed : ComponentBase
    {
        public NavigationManager Navigation { get; set; }
        public AuthenticationStateProvider AuthStateProvider { get; set; }
        public ApplicationDbContext DbContext { get; set; }
        public ILogger<LoginViewModel> Logger { get; set; }

        public string StoreCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsLoading { get; set; } = false;
        public bool IsInvalid => !string.IsNullOrEmpty(ErrorMessage);

        public async Task InitializeAsync()
        {
            try
            {
                var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated ?? false)
                {
                    Navigation.NavigateTo("/promos");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error during login initialization");
                ErrorMessage = "Initialization error. Please refresh the page.";
            }
        }

        public async Task HandleLogin()
        {
            if (IsLoading) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(StoreCode))
                {
                    ErrorMessage = "Please enter a store code";
                    return;
                }

                StoreCode = StoreCode.Trim().ToUpper();
                Logger.LogInformation("Attempting login for store code: {StoreCode}", StoreCode);

                var store = await DbContext.Accesstbl
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.storez == StoreCode);

                if (store == null)
                {
                    ErrorMessage = "Invalid store code. Please try again.";
                    Logger.LogWarning("Invalid store code attempt: {StoreCode}", StoreCode);
                    return;
                }

                if (AuthStateProvider is CustomAuthStateProvider customAuthProvider)
                {
                    Logger.LogInformation("Successful login for store: {StoreCode}", StoreCode);
                    await customAuthProvider.MarkUserAsAuthenticated(store.storez, store.sdescription);

                    await Task.Delay(100);
                    Navigation.NavigateTo("/promos", forceLoad: true);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Login failed. Please try again later.";
                Logger.LogError(ex, "Login error for store {StoreCode}: {ErrorMessage}", StoreCode, ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}