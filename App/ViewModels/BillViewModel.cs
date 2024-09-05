using CongressDataCollector.Core.Models;
using System;

namespace App.ViewModels
{
    public class BillViewModel : BindableObject
    {
        private Bill _bill;

        public BillViewModel(Bill bill)
        {
            _bill = bill ?? new Bill();
        }

        public string Id => _bill.Number;
        public string Number => _bill.Number ?? string.Empty;
        public string Title => _bill.Title ?? string.Empty;
        public string Type => _bill.Type ?? string.Empty;
        public string LatestActionText => _bill.LatestAction?.Text ?? string.Empty;
        public DateTime IntroducedDate => _bill.IntroducedDate ?? DateTime.MinValue;
    }
}
