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
                "https://c-ai-cosmos.documents.azure.com:443/",
                "YLNTD88o0cBjrrc6eWH7poHswZxMXazmzaH7IKYQ",
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
