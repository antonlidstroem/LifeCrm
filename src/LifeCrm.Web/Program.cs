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

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:59152") });

            builder.Services.AddMudServices(cfg =>
            {
                cfg.SnackbarConfiguration.PositionClass      = MudBlazor.Defaults.Classes.Position.BottomRight;
                cfg.SnackbarConfiguration.PreventDuplicates  = false;
                cfg.SnackbarConfiguration.NewestOnTop        = true;
                cfg.SnackbarConfiguration.ShowCloseIcon      = true;
                cfg.SnackbarConfiguration.VisibleStateDuration = 4000;
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<ApiClient>();
            builder.Services.AddScoped<AppState>();
            builder.Services.AddScoped<SignalRService>();

            await builder.Build().RunAsync();
        }
    }
}
