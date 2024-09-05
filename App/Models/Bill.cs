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
        public List<Cosponsor> DetailedCosponsors { get; set; }
    }

    public class LatestAction
    {
        public DateTime ActionDate { get; set; }
        public string Text { get; set; }
    }

    public class Cosponsor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
