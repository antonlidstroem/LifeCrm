// LifeCrm.Web — Blazor WebAssembly entry point
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

            // ── HTTP Client base address ─────────────────────────────────────
            // The project has two valid hosting configurations:
            //
            //   DEVELOPMENT: Blazor WASM dev server on :59153, API on :59152.
            //     The WASM app must send API requests to :59152 explicitly.
            //     Set in wwwroot/appsettings.Development.json: { "ApiBaseUrl": "https://localhost:59152" }
            //
            //   PRODUCTION:  API serves both static files and API on the same host.
            //     Relative URLs work — use BaseUri (same origin).
            //     Set in wwwroot/appsettings.json: { "ApiBaseUrl": "" }
            //
            // Using navManager.BaseUri alone was WRONG for development because it
            // returns the WASM dev server URL (:59153), not the API URL (:59152),
            // causing every API call to return 405 Method Not Allowed.
            var apiBaseUrl = builder.Configuration["ApiBaseUrl"];

            builder.Services.AddScoped(sp =>
            {
                var navManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
                // Use configured API URL if set; fall back to same-origin (production).
                var baseAddress = !string.IsNullOrWhiteSpace(apiBaseUrl)
                    ? apiBaseUrl.TrimEnd('/') + "/"
                    : navManager.BaseUri;

                return new HttpClient { BaseAddress = new Uri(baseAddress) };
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
            // Singleton so AppState survives Blazor component re-renders during navigation
            builder.Services.AddSingleton<AppState>();
            builder.Services.AddScoped<SignalRService>();

            await builder.Build().RunAsync();
        }
    }
}
