using App.ViewModels;

namespace App
{
    public partial class BillListPage : ContentPage
    {
        public BillListPage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetRequiredService<BillListPageViewModel>();
        }
    }
}
