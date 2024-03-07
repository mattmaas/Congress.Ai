
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Azure.Core;

namespace CongressDataCollector.Core.Models
{
    public class LatestAction
    {
        [JsonProperty("actionDate")]
        public DateTime? ActionDate { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }


    // Response classes with pagination support

    // Additional models or helper classes as needed...
}