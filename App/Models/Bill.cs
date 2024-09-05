using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace App.Models
{
    public class Bill
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("congress")]
        public int Congress { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("introducedDate")]
        public DateTime IntroducedDate { get; set; }

        [JsonProperty("latestAction")]
        public LatestAction LatestAction { get; set; }

        [JsonProperty("detailedCosponsors")]
        public List<Cosponsor> DetailedCosponsors { get; set; }
    }

    public class LatestAction
    {
        [JsonProperty("actionDate")]
        public DateTime ActionDate { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Cosponsor
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("party")]
        public string Party { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }
    }
}
