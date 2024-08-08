using CongressDataCollector.Core.Models;

namespace App.ViewModels
{
    public class BillViewModel : BindableObject
    {
        private Bill _bill;

        public BillViewModel(Bill bill)
        {
            _bill = bill;
        }

        public string Id => _bill.Number;
        public string Number => _bill.Number;
        public string Title => _bill.Title;
        public string Type => _bill.Type;
        public string LatestActionText => _bill.LatestAction?.Text;
    }
}
