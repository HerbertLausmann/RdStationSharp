using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RdStationSharp.ADS
{
    /// <summary>
    /// Gerencia públicos personalizados do Facebook.
    /// </summary>
    public class FBCustomAudienceManagerSync
    {
        private string _accessToken;
        private string _accountId;
        private string _appId;
        private string _appSecret;
        private string _redirectUri;
        private bool _IsAuthenticated;

        /// <summary>
        /// Obtém o token de acesso.
        /// </summary>
        public string AccessToken { get => _accessToken; }
        /// <summary>
        /// Obtém ou define o ID da conta.
        /// </summary>
        public string AccountId
        {
            get => _accountId;
            set
            {

                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
                // A API do Facebook precisa que o ID da Conta de Anúncios inicie com "act_".
                if (!value.StartsWith("act_"))
                {
                    _accountId = string.Concat("act_", value);
                }
                else
                    _accountId = value;
            }
        }
        /// <summary>
        /// Obtém o ID do aplicativo.
        /// </summary>
        public string AppId { get => _appId; }
        /// <summary>
        /// Obtém o segredo do aplicativo.
        /// </summary>
        public string AppSecret { get => _appSecret; }
        /// <summary>
        /// Obtém a URI de redirecionamento.
        /// </summary>
        public string RedirectUri { get => _redirectUri; }
        /// <summary>
        /// Obtém o valor que indica se o usuário está autenticado.
        /// </summary>
        public bool IsAuthenticated { get => _IsAuthenticated; }

        /// <summary>
        /// Inicializa uma nova instância da classe FBCustomAudienceManagerSync.
        /// </summary>
        /// <param name="appId">O ID do aplicativo.</param>
        /// <param name="appSecret">O segredo do aplicativo.</param>
        /// <param name="redirectUri">A URI de redirecionamento.</param>
        public FBCustomAudienceManagerSync(string appId, string appSecret, string redirectUri)
        {
            _appId = appId;
            _appSecret = appSecret;
            _redirectUri = redirectUri;
        }

        /// <summary>
        /// Cria ou atualiza um público personalizado.
        /// </summary>
        /// <param name="customerDataTable">Tabela de dados dos clientes.</param>
        /// <param name="customAudienceName">Nome do público personalizado.</param>
        /// <param name="description">Descrição do público personalizado.</param>
        /// <returns>Verdadeiro se a audiência foi criada ou atualizada com sucesso, falso caso contrário.</returns>
        public bool CreateOrUpdateCustomAudience(DataTable customerDataTable, string customAudienceName, string description)
        {
            string customAudienceId = GetCustomAudienceIdByName(customAudienceName);

            if (customAudienceId == null)
            {
                customAudienceId = CreateCustomAudience(customAudienceName, description);
            }

            return AddUsersToCustomAudience(customAudienceId, customerDataTable);
        }

        /// <summary>
        /// Obtém o ID do público personalizado pelo nome.
        /// </summary>
        /// <param name="customAudienceName">Nome do público personalizado.</param>
        /// <returns>O ID do público personalizado, ou nulo se não for encontrado.</returns>
        private string GetCustomAudienceIdByName(string customAudienceName)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                string url = $"https://graph.facebook.com/v16.0/{_accountId}/customaudiences?fields=name&access_token={_accessToken}";
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    JObject data = JsonConvert.DeserializeObject<JObject>(content);
                    JArray customAudiences = (JArray)data["data"];

                    var customAudience = customAudiences.FirstOrDefault(a => a["name"].ToString() == customAudienceName);
                    if (customAudience != null)
                    {
                        return customAudience["id"].ToString();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Cria um público personalizado.
        /// </summary>
        /// <param name="customAudienceName">Nome do público personalizado.</param>
        /// <param name="description">Descrição do público personalizado.</param>
        /// <returns>O ID do público personalizado criado, ou nulo se falhar.</returns>
        private string CreateCustomAudience(string customAudienceName, string description)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                string url = $"https://graph.facebook.com/v16.0/{_accountId}/customaudiences";
                var postData = new JObject
                {
                    { "name", customAudienceName },
                    { "subtype", "CUSTOM" },
                    { "description", description },
                    { "customer_file_source", "PARTNER_PROVIDED_ONLY" }
                };

                HttpContent content = new StringContent(postData.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    JObject data = JsonConvert.DeserializeObject<JObject>(responseContent);
                    return data["id"].ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Adiciona usuários ao público personalizado.
        /// </summary>
        /// <param name="customAudienceId">O ID do público personalizado.</param>
        /// <param name="customerDataTable">Tabela de dados dos clientes.</param>
        /// <returns>Verdadeiro se os usuários foram adicionados com sucesso, falso caso contrário.</returns>
        private bool AddUsersToCustomAudience(string customAudienceId, DataTable customerDataTable)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                string url = $"https://graph.facebook.com/v16.0/{customAudienceId}/users";

                string payload = null;
                if (customerDataTable.TableName == "Facebook Ready Audience DataTable")
                    payload = CreatePayloadFromFBReadyTable(customerDataTable);
                else
                    payload = CreatePayloadFromMailDataTable(customerDataTable);

                var postData = new JObject
                {
                    { "payload", payload }
                };

                HttpContent content = new StringContent(postData.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PostAsync(url, content).Result;
                return response.IsSuccessStatusCode;
            }
        }

        /// <summary>
        /// Autentica o usuário e obtém o token de acesso.
        /// </summary>
        public void Auth()
        {
            if (IsAccessTokenValid(RdStationSharp.Properties.Settings.Default.FacebookToken, AppId, AppSecret))
            {
                _accessToken = RdStationSharp.Properties.Settings.Default.FacebookToken;
                _IsAuthenticated = true;
            }
            else
            {
                string authorizationCode = null;
                authorizationCode = GetAuthorizationCode(_appId, _redirectUri);

                if (!string.IsNullOrEmpty(authorizationCode))
                {

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        string url = $"https://graph.facebook.com/v16.0/oauth/access_token?client_id={_appId}&redirect_uri={_redirectUri}&client_secret={_appSecret}&code={authorizationCode}";
                        HttpResponseMessage response = httpClient.GetAsync(url).Result;

                        if (response.Headers.WwwAuthenticate.ToString().Contains("invalid_code"))
                        {
                            RdStationSharp.Properties.Settings.Default.FacebookToken = null;
                            RdStationSharp.Properties.Settings.Default.Save();
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            string content = response.Content.ReadAsStringAsync().Result;
                            JObject data = JsonConvert.DeserializeObject<JObject>(content);
                            _accessToken = data["access_token"].ToString();
                            _accessToken = GetLongLivedAccessToken(AppId, AppSecret, _accessToken);
                            RdStationSharp.Properties.Settings.Default.FacebookToken = _accessToken;
                            RdStationSharp.Properties.Settings.Default.Save();
                            _IsAuthenticated = true;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Verifica se o token de acesso é válido.
        /// </summary>
        /// <param name="accessToken">O token de acesso.</param>
        /// <param name="appId">O ID do aplicativo.</param>
        /// <param name="appSecret">O segredo do aplicativo.</param>
        /// <returns>Verdadeiro se o token de acesso for válido, falso caso contrário.</returns>
        private bool IsAccessTokenValid(string accessToken, string appId, string appSecret)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string appAccessToken = $"{appId}|{appSecret}";
                string url = $"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appAccessToken}";

                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    JObject data = JsonConvert.DeserializeObject<JObject>(content);

                    bool isValid = data["data"]["is_valid"].Value<bool>();
                    DateTime expirationTime = DateTimeOffset.FromUnixTimeSeconds(data["data"]["expires_at"].Value<long>()).DateTime;

                    if (isValid && expirationTime > DateTime.UtcNow)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica se é necessário autenticar o usuário.
        /// </summary>
        /// <returns>Verdadeiro se a autenticação do usuário for necessária, falso caso contrário.</returns>
        public bool NeedUserAuthentication()
        {
            return !IsAccessTokenValid(RdStationSharp.Properties.Settings.Default.FacebookToken, AppId, AppSecret);
        }

        /// <summary>
        /// Obtém o token de acesso de longa duração.
        /// </summary>
        /// <param name="appId">O ID do aplicativo.</param>
        /// <param name="appSecret">O segredo do aplicativo.</param>
        /// <param name="shortLivedAccessToken">O token de acesso de curta duração.</param>
        /// <returns>O token de acesso de longa duração, ou nulo se falhar.</returns>
        private string GetLongLivedAccessToken(string appId, string appSecret, string shortLivedAccessToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string url = $"https://graph.facebook.com/v16.0/oauth/access_token?grant_type=fb_exchange_token&client_id={appId}&client_secret={appSecret}&fb_exchange_token={shortLivedAccessToken}";
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    JObject data = JsonConvert.DeserializeObject<JObject>(content);
                    return data["access_token"].ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém o código de autorização.
        /// </summary>
        /// <param name="appId">O ID do aplicativo.</param>
        /// <param name="redirectUri">A URI de redirecionamento.</param>
        /// <returns>O código de autorização, ou nulo se falhar.</returns>
        private string GetAuthorizationCode(string appId, string redirectUri)
        {
            string authorizationCode = null;
            string authorizationUrl = $"https://www.facebook.com/dialog/oauth?client_id={appId}&redirect_uri={redirectUri}&scope=ads_management";
            authorizationCode = OAuth2Sharp.OAuth2.AuthAsync(authorizationUrl, redirectUri).Result.Token;
            return authorizationCode;
        }

        /// <summary>
        /// Cria o payload a partir da tabela de dados que contenha uma coluna de e-mails.
        /// </summary>
        /// <param name="customerDataTable">Tabela de dados dos clientes.</param>
        /// <returns>Uma string JSON correspondente ao payload.</returns>
        private string CreatePayloadFromMailDataTable(DataTable customerDataTable)
        {
            int mailColumnIndex = -1;
            foreach (DataColumn item in customerDataTable.Columns)
            {
                if (item.ColumnName.ToLower().Contains("mail"))
                {
                    mailColumnIndex = customerDataTable.Columns.IndexOf(item);
                    break;
                }
            }
            var schema = new
            {
                schema = new[] { "EMAIL" },
                data = new string[customerDataTable.Rows.Count][]
            };

            for (int i = 0; i < customerDataTable.Rows.Count; i++)
            {
                DataRow row = customerDataTable.Rows[i];
                string email = row[mailColumnIndex].ToString();
                string hashedEmail = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(email.ToLower().Trim()))).Replace("-", string.Empty).ToLower();
                schema.data[i] = new[] { hashedEmail };
            }

            return JsonConvert.SerializeObject(schema);
        }

        /// <summary>
        /// Cria o payload a partir da tabela de dados exportada pela método <b>ExportFacebookPublic()</b> da classe <b>RDCloud.</b>
        /// </summary>
        /// <param name="customerDataTable">Tabela de dados dos clientes.</param>
        /// <returns>Uma string JSON correspondente ao payload.</returns>
        private string CreatePayloadFromFBReadyTable(DataTable customerDataTable)
        {
            var columnsToHash = new List<string> { "email", "phone", "fn", "ln", "ct", "st", "country" };
            var schemaColumns = new List<string> { "EMAIL", "PHONE", "FN", "LN", "CT", "ST", "COUNTRY" };
            var columnIndexMapping = new Dictionary<string, int>();

            foreach (string columnName in columnsToHash)
            {
                int columnIndex = customerDataTable.Columns.IndexOf(columnName);
                if (columnIndex != -1)
                {
                    columnIndexMapping[columnName] = columnIndex;
                }
            }

            var schemaData = new List<string[]>();

            for (int i = 0; i < customerDataTable.Rows.Count; i++)
            {
                DataRow row = customerDataTable.Rows[i];
                var rowData = new List<string>();

                for (int j = 0; j < columnsToHash.Count; j++)
                {
                    if (columnIndexMapping.ContainsKey(columnsToHash[j]))
                    {
                        string value = row[columnIndexMapping[columnsToHash[j]]].ToString();
                        string hashedValue = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(value.ToLower().Trim()))).Replace("-", string.Empty).ToLower();

                        rowData.Add(hashedValue);
                    }
                }

                schemaData.Add(rowData.ToArray());
            }

            var payload = new
            {
                schema = schemaColumns,
                data = schemaData
            };

            return JsonConvert.SerializeObject(payload);
        }

        // <summary>
        /// Obtém a lista de contas de anúncios do usuário.
        /// </summary>
        /// <returns>Uma lista de contas de anúncios.</returns>
        public List<AdAccount> GetUserAdAccounts()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                string url = "https://graph.facebook.com/v12.0/me/adaccounts?fields=id,name";
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    JObject data = JsonConvert.DeserializeObject<JObject>(content);
                    JArray adAccountsArray = (JArray)data["data"];

                    return adAccountsArray.ToObject<List<AdAccount>>();
                }
            }

            return new List<AdAccount>();
        }

    }

    /// <summary>
    /// Representa uma conta de anúncio do Facebook.
    /// </summary>
    public class AdAccount
    {
        /// <summary>
        /// Obtém ou define o ID da conta de anúncio.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Obtém ou define o nome da conta de anúncio.
        /// </summary>
        public string Name { get; set; }
    }

}