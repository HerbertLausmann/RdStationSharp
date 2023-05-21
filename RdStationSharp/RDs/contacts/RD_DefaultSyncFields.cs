using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.contacts
{
    /// <summary>
    /// Classe que contém os campos padrão de um Lead na RD Station, implementa também os campos personalizados.
    /// </summary>
    public class RD_DefaultSyncFields : RD_CustomFields
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("job_title")]
        public string JobTitle { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("personal_phone")]
        public string PersonalPhone { get; set; }

        [JsonProperty("mobile_phone")]
        public string MobilePhone { get; set; }

        [JsonProperty("twitter")]
        public string Twitter { get; set; }

        [JsonProperty("facebook")]
        public string Facebook { get; set; }

        [JsonProperty("linkedin")]
        public string Linkedin { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("legal_bases")]
        public List<LegalBas> LegalBases { get; set; }

        public T CopyTo<T>()
        {
            var json = JsonConvert.SerializeObject(this, GetType(), new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            T newInstance = (T)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(T));
            return newInstance;
        }
    }
}
