using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.segmentations
{
    public class SegmentationContact
    {
        [JsonProperty("uuid")]
        public string uuid { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("last_conversion_date")]
        public DateTime last_conversion_date { get; set; }

        [JsonProperty("created_at")]
        public DateTime created_at { get; set; }

        [JsonProperty("links")]
        public List<SegmentationLink> links { get; set; }
    }

    public class SegmentationLink
    {
        [JsonProperty("rel")]
        public string rel { get; set; }

        [JsonProperty("href")]
        public string href { get; set; }

        [JsonProperty("media")]
        public string media { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }
    }

    public class SegmentationResponseData
    {
        [JsonProperty("contacts")]
        public List<SegmentationContact> contacts { get; set; }
    }


}
