using System;
using System.Collections.Generic;
using App.Models;

namespace App.ViewModels
{
    public class BillViewModel : BindableObject
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string Title { get; set; }
        public DateTime IntroducedDate { get; set; }
        public DateTime LatestActionDate { get; set; }
        public string LatestActionText { get; set; }
        public List<string> Sponsors { get; set; }

        public BillViewModel(Bill bill)
        {
            Id = bill.Id;
            Number = $"{bill.Type}{bill.Number}";
            Title = bill.Title;
            IntroducedDate = bill.IntroducedDate;
            LatestActionDate = bill.LatestAction?.ActionDate ?? DateTime.MinValue;
            LatestActionText = bill.LatestAction?.Text ?? "No action";
            Sponsors = bill.DetailedCosponsors?.Select(c => $"{c.FirstName} {c.LastName}").ToList() ?? new List<string>();
        }
    }
}
