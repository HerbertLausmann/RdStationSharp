using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.contacts
{
    public class LeadStageData
    {
        public static readonly string Lead = "Lead";
        public static readonly string QualifiedLead = "Qualified Lead";
        public static readonly string Client = "Client";
        [JsonProperty("lifecycle_stage")]
        public string LifecycleStage { get; set; }

        [JsonProperty("opportunity")]
        public bool Opportunity { get; set; }

        [JsonProperty("contact_owner_email")]
        public string ContactOwnerEmail { get; set; }
    }

}
