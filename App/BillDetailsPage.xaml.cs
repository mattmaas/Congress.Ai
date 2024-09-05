using App.ViewModels;

namespace App
{
    public partial class BillDetailsPage : ContentPage
    {
        public BillDetailsPage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetRequiredService<BillDetailsPageViewModel>();
        }
    }
}
