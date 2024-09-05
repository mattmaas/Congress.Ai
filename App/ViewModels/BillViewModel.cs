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
        public string LatestActionText => _bill.LatestAction?.Text ?? "No action available";
        public DateTime IntroducedDate => _bill.IntroducedDate;
        public DateTime LatestActionDate => _bill.LatestAction?.ActionDate ?? DateTime.MinValue;
        public List<string> Sponsors => GetSponsors();
        public List<string> Cosponsors => GetCosponsors();
        public string OpenAiSummary => _bill.OpenAiSummaries?.Summary ?? "No AI-generated summary available";
        public List<string> OpenAiKeyChanges => GetOpenAiKeyChanges();
        public List<string> Subjects => GetSubjects();
        public List<string> RelatedBills => GetRelatedBills();
        public List<ActionViewModel> Actions => GetActions();

        private List<string> GetSponsors()
        {
            var sponsors = _bill.DetailedCosponsors?
                .Where(c => c.CosponsorType == "sponsor")
                .Select(c => $"{c.FirstName} {c.LastName} ({c.Party}-{c.State})")
                .ToList();
            return sponsors?.Count > 0 ? sponsors : new List<string> { "No sponsors information available" };
        }

        private List<string> GetCosponsors()
        {
            var cosponsors = _bill.DetailedCosponsors?
                .Where(c => c.CosponsorType == "cosponsor")
                .Select(c => $"{c.FirstName} {c.LastName} ({c.Party}-{c.State})")
                .ToList();
            return cosponsors?.Count > 0 ? cosponsors : new List<string> { "No cosponsors information available" };
        }

        private List<string> GetOpenAiKeyChanges()
        {
            var keyChanges = _bill.OpenAiSummaries?.KeyChanges?
                .Select(change => change.StartsWith("**Affected parties:**") ? change : change.Replace("**", "<b>").Replace("**", "</b>"))
                .ToList();
            return keyChanges?.Count > 0 ? keyChanges : new List<string> { "No AI-generated key changes available" };
        }

        private List<string> GetSubjects()
        {
            var subjects = _bill.DetailedSubjects?.Select(s => s.Name).ToList();
            return subjects?.Count > 0 ? subjects : new List<string> { "No subject information available" };
        }

        private List<string> GetRelatedBills()
        {
            var relatedBills = _bill.DetailedRelatedBills?
                .Select(r => $"{r.Type}{r.Number} - {r.Title}")
                .ToList();
            return relatedBills?.Count > 0 ? relatedBills : new List<string> { "No related bills information available" };
        }

        private List<ActionViewModel> GetActions()
        {
            var actions = _bill.DetailedActions?
                .Select(a => new ActionViewModel { Date = a.ActionDate, Text = a.Text })
                .ToList();
            return actions?.Count > 0 ? actions : new List<ActionViewModel> { new ActionViewModel { Date = DateTime.MinValue, Text = "No actions information available" } };
        }
    }

    public class ActionViewModel
    {
        public DateTime Date { get; set; }
        public string Text { get; set; }
    }
}
