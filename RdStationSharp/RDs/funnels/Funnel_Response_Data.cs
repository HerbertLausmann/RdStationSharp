using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.funnels
{
    public class Funnel_Response_Data
    {
        [JsonProperty("lifecycle_stage")]
        public string LifecycleStage { get; set; }

        [JsonProperty("opportunity")]
        public bool Opportunity { get; set; }

        [JsonProperty("contact_owner_email")]
        public object ContactOwnerEmail { get; set; }

        [JsonProperty("interest")]
        public int Interest { get; set; }

        [JsonProperty("fit")]
        public int Fit { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }
    }

}
