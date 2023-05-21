using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.contacts
{
    public class LegalBas
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class Payload : RD_DefaultConversionFields
    {
        public Payload()
        {
            LegalBases = new List<LegalBas>();
            LegalBases.Add(new LegalBas() { Category = "communications", Type = "consent", Status = "granted", });
            AvailableForMailing = true;
            ConversionIdentifier = "integracao-rd";
        }

        [JsonProperty("conversion_identifier")]
        public string ConversionIdentifier { get; set; }

        [JsonProperty("client_tracking_id")]
        public string ClientTrackingId { get; set; }

        [JsonProperty("traffic_source")]
        public string TrafficSource { get; set; }

        [JsonProperty("traffic_medium")]
        public string TrafficMedium { get; set; }

        [JsonProperty("traffic_campaign")]
        public string TrafficCampaign { get; set; }

        [JsonProperty("traffic_value")]
        public string TrafficValue { get; set; }

        [JsonProperty("available_for_mailing")]
        public bool AvailableForMailing { get; set; }

    }

    public class LeadConversionData
    {
        public LeadConversionData()
        {
            EventType = "CONVERSION";
            EventFamily = "CDP";
            Payload = new Payload();
        }

        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("event_family")]
        public string EventFamily { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }

        public contacts.LeadUpdateData ToLeadUpdateData()
        {
            return contacts.LeadUpdateData.FromLead(this);
        }
    }
}
