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
        public List<string> Sponsors => _bill.DetailedCosponsors?
            .Where(c => c.CosponsorType == "sponsor")
            .Select(c => $"{c.FirstName} {c.LastName} ({c.Party})")
            .ToList() ?? new List<string>();
        public List<string> Cosponsors => _bill.DetailedCosponsors?
            .Where(c => c.CosponsorType == "cosponsor")
            .Select(c => $"{c.FirstName} {c.LastName} ({c.Party})")
            .ToList() ?? new List<string>();
        public string OpenAiSummary => _bill.OpenAiSummaries?.Summary ?? "No summary available";
        public List<string> OpenAiKeyChanges => (_bill.OpenAiSummaries?.KeyChanges ?? new List<string>())
            .Select(change => change.StartsWith("**Affected parties:**") ? change : change.Replace("**", "<b>").Replace("**", "</b>"))
            .ToList();
        public List<string> Subjects => _bill.DetailedSubjects?.Select(s => s.Name).ToList() ?? new List<string>();
        public List<string> RelatedBills => _bill.DetailedRelatedBills?
            .Select(r => $"{r.Type}{r.Number} - {r.Title}")
            .ToList() ?? new List<string>();
        public List<ActionViewModel> Actions => _bill.DetailedActions?
            .Select(a => new ActionViewModel { Date = a.ActionDate, Text = a.Text })
            .ToList() ?? new List<ActionViewModel>();
    }

    public class ActionViewModel
    {
        public DateTime Date { get; set; }
        public string Text { get; set; }
    }
}
