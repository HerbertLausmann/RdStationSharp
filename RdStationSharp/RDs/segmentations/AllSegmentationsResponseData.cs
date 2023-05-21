using Newtonsoft.Json;
using RdStationSharp.RDs.contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.segmentations
{
    public class SegmentationInfosResponseData
    {
        [JsonProperty("segmentations")]
        public List<SegmentationInfo> segmentations { get; set; }
    }

    public class SegmentationInfo
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("standard")]
        public bool? standard { get; set; }

        [JsonProperty("created_at")]
        public DateTime created_at { get; set; }

        [JsonProperty("updated_at")]
        public DateTime updated_at { get; set; }

        [JsonProperty("process_status")]
        public string process_status { get; set; }

        [JsonProperty("links")]
        public List<Link> links { get; set; }
    }
}
