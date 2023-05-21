using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.contacts
{
    /// <summary>
    /// Classe que contém os campos utilizados somente no momento de gerar a conversão dos Leads. Implementa os campos padrão e também os campos personalizados.
    /// </summary>
    public class RD_DefaultConversionFields : RD_DefaultSyncFields
    {
        [JsonProperty("company_name")]
        public string CompanyName { get; set; }

        [JsonProperty("company_site")]
        public string CompanySite { get; set; }

        [JsonProperty("company_address")]
        public string CompanyAddress { get; set; }
    }
}
