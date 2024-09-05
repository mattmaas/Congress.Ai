using System;
using System.Collections.Generic;

namespace App.Models
{
    public class Bill
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime IntroducedDate { get; set; }
        public LatestAction LatestAction { get; set; }
        public List<Sponsor> Sponsors { get; set; }
        public List<Cosponsor> DetailedCosponsors { get; set; }
        public OpenAiSummaries OpenAiSummaries { get; set; }
        public List<Subject> DetailedSubjects { get; set; }
        public List<RelatedBill> DetailedRelatedBills { get; set; }
        public List<Action> DetailedActions { get; set; }
    }

    public class LatestAction
    {
        public DateTime ActionDate { get; set; }
        public string Text { get; set; }
    }

    public class Sponsor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Party { get; set; }
        public string State { get; set; }
    }

    public class Cosponsor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Party { get; set; }
        public string State { get; set; }
        public bool IsOriginalCosponsor { get; set; }
    }

    public class OpenAiSummaries
    {
        public string Summary { get; set; }
        public List<string> KeyChanges { get; set; }
    }

    public class Subject
    {
        public string Name { get; set; }
    }

    public class RelatedBill
    {
        public string Type { get; set; }
        public string Number { get; set; }
        public string Title { get; set; }
    }

    public class Action
    {
        public DateTime ActionDate { get; set; }
        public string Text { get; set; }
    }
}
