using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.events
{
    public class ORDER_PLACED_ITEM_Payload
    {
        public ORDER_PLACED_ITEM_Payload()
        {
            LegalBases = new List<RDs.contacts.LegalBas>();
            LegalBases.Add(new RDs.contacts.LegalBas() { Category = "communications", Type = "consent", Status = "granted", });
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("cf_order_id")]
        public string CfOrderId { get; set; }

        [JsonProperty("cf_order_product_id")]
        public string CfOrderProductId { get; set; }

        [JsonProperty("cf_order_product_sku")]
        public string CfOrderProductSku { get; set; }

        [JsonProperty("legal_bases")]
        public List<RDs.contacts.LegalBas> LegalBases { get; set; }
    }

    public class ORDER_PLACED_ITEM
    {
        public ORDER_PLACED_ITEM()
        {
            EventType = "ORDER_PLACED_ITEM";
            EventFamily = "CDP";
        }

        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("event_family")]
        public string EventFamily { get; set; }

        [JsonProperty("payload")]
        public ORDER_PLACED_ITEM_Payload Payload { get; set; }
    }
}
