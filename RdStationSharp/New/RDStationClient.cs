using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RdStationSharp.New
{
    public class RdStationApiClient : IRdStationApiClient
    {
        public const string BASE_ADDRESS = @"https://api.rd.services/";
        public const string CONVERSION_URL = BASE_ADDRESS + "1.3/conversions";
        public const string CHANGE_LEAD_URL = BASE_ADDRESS + "1.2/leads/";
        private readonly HttpClient _httpClient;

        public RdStationApiClient(HttpClient client = default(HttpClient))
        {
            _httpClient = client ?? new HttpClient { BaseAddress = new Uri(BASE_ADDRESS) };
        }

        /// <summary>
        /// Send Lead to RdStation Async
        /// </summary>
        /// <param name="lead">Lead to be Sent</param>
        /// <returns>true if sent</returns>
        public async Task<bool> SendLead(ILead lead)
        {
            var response = await _httpClient.PostAsJsonAsync2(CONVERSION_URL, lead);
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Send Lead to RdStation Sync
        /// </summary>
        /// <param name="lead">Lead to be Sent</param>
        /// <returns>true if sent</returns>
        public bool SendLeadSync(ILead lead)
        {
            var response = _httpClient.PostAsJsonAsync2(CONVERSION_URL, lead).Result;
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Change Lead Status RdStation Async
        /// </summary>
        /// <param name="email">Lead e-mail</param>
        /// <param name="leadStatusRoot">Lead Status to be Sent</param>
        /// <returns>true if sent</returns>
        public bool ChangeLeadStatusSync(string email, LeadStatusRoot leadStatusRoot)
        {
            var response = _httpClient.PostAsJsonAsync2(CHANGE_LEAD_URL + email, leadStatusRoot).Result;
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Change Lead Status RdStation Sync
        /// </summary>
        /// <param name="email">Lead e-mail</param>
        /// <param name="leadStatusRoot">Lead Status to be Sent</param>
        /// <returns>true if sent</returns>
        public async Task<bool> ChangeLeadStatus(string email, LeadStatusRoot leadStatusRoot)
        {
            var response = await _httpClient.PostAsJsonAsync2(CHANGE_LEAD_URL + email, leadStatusRoot);
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
        }


    }
}