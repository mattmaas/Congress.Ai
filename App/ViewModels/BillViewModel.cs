using App.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace App.ViewModels
{
    public class BillViewModel : BindableObject
    {
        private Bill _bill;

        public BillViewModel(Bill bill)
        {
            _bill = bill ?? new Bill();
        }

        public string Id => _bill.Id;
        public string Number => $"{_bill.Type}{_bill.Number}";
        public string Title => _bill.Title ?? string.Empty;
        public string Type => _bill.Type ?? string.Empty;
        public string LatestActionText => _bill.LatestAction?.Text ?? "No action";
        public DateTime IntroducedDate => _bill.IntroducedDate;
        public DateTime LatestActionDate => _bill.LatestAction?.ActionDate ?? DateTime.MinValue;
        public List<string> Sponsors => _bill.DetailedCosponsors?.Select(c => $"{c.FirstName} {c.LastName}").ToList() ?? new List<string>();
        public string OpenAiSummary => _bill.OpenAiSummaries?.Summary ?? "No summary available";
        public string OpenAiKeyChanges => _bill.OpenAiSummaries?.KeyChanges != null 
            ? string.Join("\n", _bill.OpenAiSummaries.KeyChanges) 
            : "No key changes available";
    }
}
