using App.Services;
using App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();

            var services = new ServiceCollection();

            // Register the CosmosDbService
            services.AddSingleton<CosmosDbService>(sp => new CosmosDbService(
                "YOUR_COSMOS_DB_ENDPOINT",
                "YOUR_COSMOS_DB_KEY",
                "CongressDB",
                "Bills"
            ));

            // Register your ViewModels
            services.AddTransient<BillListPageViewModel>();
            services.AddTransient<BillDetailsPageViewModel>();

            Services = services.BuildServiceProvider();

            MainPage = new AppShell();
        }
    }
}
