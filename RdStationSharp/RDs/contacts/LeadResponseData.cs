using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.contacts
{
    public class Link
    {
        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("media")]
        public string Media { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
    public class LeadResponseData : RD_DefaultSyncFields
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("extra_emails")]
        public List<object> ExtraEmails { get; set; }

        [JsonProperty("links")]
        public List<Link> Links { get; set; }

    }
}