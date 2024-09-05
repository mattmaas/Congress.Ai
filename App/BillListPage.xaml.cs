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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as BillListPageViewModel)?.RefreshBills();
        }
    }
}
