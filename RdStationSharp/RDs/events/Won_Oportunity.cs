using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.events
{
    public class Won_Oportunity_Payload
    {
        public Won_Oportunity_Payload()
        {
            FunnelName = "default";
        }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("funnel_name")]
        public string FunnelName { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }

    public class Won_Oportunity
    {
        public Won_Oportunity()
        {
            EventType = "SALE";
            EventFamily = "CDP";
        }

        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("event_family")]
        public string EventFamily { get; set; }

        [JsonProperty("payload")]
        public Won_Oportunity_Payload Payload { get; set; }
    }
}
