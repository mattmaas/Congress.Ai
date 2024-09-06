using App.Services;
using App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Reflection;

namespace App
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();

            // Build configuration
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("App.appsettings.json");
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            services.AddSingleton<IConfiguration>(config);

            // Register the CosmosDbService
            services.AddSingleton<CosmosDbService>();

            // Register your ViewModels
            services.AddTransient<BillListPageViewModel>();
            services.AddTransient<BillDetailsPageViewModel>();

            Services = services.BuildServiceProvider();

            MainPage = new AppShell();

            // Set up custom splash screen
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                if (view is IWindow window)
                {
                    window.LoadingIndicator = new SplashScreen();
                }
            });
        }
    }
}
