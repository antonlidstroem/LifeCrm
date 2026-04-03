using Blazored.LocalStorage;
using LifeCrm.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace LifeCrm.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // FIXED: Use the host's base address for same-origin API requests.
            // In development the API and WASM are served from the same host (API project
            // serves the WASM files). This means BaseAddress == API origin, so all
            // "api/v1/..." calls resolve correctly without hardcoding localhost ports.
            builder.Services.AddScoped(sp =>
            {
                var navManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
                return new HttpClient { BaseAddress = new Uri(navManager.BaseUri) };
            });

            builder.Services.AddMudServices(cfg =>
            {
                cfg.SnackbarConfiguration.PositionClass        = MudBlazor.Defaults.Classes.Position.BottomRight;
                cfg.SnackbarConfiguration.PreventDuplicates    = false;
                cfg.SnackbarConfiguration.NewestOnTop          = true;
                cfg.SnackbarConfiguration.ShowCloseIcon        = true;
                cfg.SnackbarConfiguration.VisibleStateDuration = 4000;
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<ApiClient>();
            // FIXED: AppState is Singleton so it survives Blazor component re-renders
            // without losing in-memory state mid-session (e.g. during navigation).
            builder.Services.AddSingleton<AppState>();
            builder.Services.AddScoped<SignalRService>();

            await builder.Build().RunAsync();
        }
    }
}
