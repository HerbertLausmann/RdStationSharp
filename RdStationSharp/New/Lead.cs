using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.New
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Company
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Funnel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lifecycle_stage")]
        public string LifecycleStage { get; set; }

        [JsonProperty("opportunity")]
        public bool Opportunity { get; set; }

        [JsonProperty("contact_owner_email")]
        public string ContactOwnerEmail { get; set; }

        [JsonProperty("interest")]
        public int Interest { get; set; }

        [JsonProperty("fit")]
        public int Fit { get; set; }

        [JsonProperty("origin")]
        public string Origin { get; set; }
    }

    public class Contact
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("job_title")]
        public string JobTitle { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("personal_phone")]
        public string PersonalPhone { get; set; }

        [JsonProperty("mobile_phone")]
        public string MobilePhone { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("facebook")]
        public string Facebook { get; set; }

        [JsonProperty("linkedin")]
        public string Linkedin { get; set; }

        [JsonProperty("twitter")]
        public string Twitter { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("cf_custom_field_example")]
        public List<string> CfCustomFieldExample { get; set; }

        [JsonProperty("company")]
        public Company Company { get; set; }

        [JsonProperty("funnel")]
        public Funnel Funnel { get; set; }
    }

    public class Root
    {
        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("entity_type")]
        public string EntityType { get; set; }

        [JsonProperty("event_identifier")]
        public string EventIdentifier { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("event_timestamp")]
        public DateTime EventTimestamp { get; set; }

        [JsonProperty("contact")]
        public Contact Contact { get; set; }
    }
}
