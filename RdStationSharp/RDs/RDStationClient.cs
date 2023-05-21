using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using RdStationSharp.Properties;
using RdStationSharp.Helper;
using RdStationSharp.RDs.segmentations;
using RdStationSharp.RDs.contacts;
using System.Linq;
using System.Collections.Generic;

namespace RdStationSharp.RDs
{
    /// <summary>
    /// Classe responsável por fazer a comunicação com a RD Station, através da API REST e autenticação OAuth2.
    /// Com essa classe é possível criar, atualizar, deletar e obter dados de Leads na RD Station.
    /// </summary>
    public class RdStationClient
    {
        public const string BASE_ADDRESS = @"https://api.rd.services/";
        public const string CONVERSION_URL = "https://api.rd.services/platform/events";

        private HttpClient _Client;

        private string _ClientID;
        private string _ClientSecret;
        private string _oAuthUrlCallback;

        /// <summary>
        /// Setado internamente, no construtor da classe.
        /// </summary>
        private string _OAuth2Url;

        /// <summary>
        /// ClientID da Conta do RD Station com que se deseja comunicar.
        /// </summary>
        public string ClientID
        {
            get => _ClientID;
            private set => _ClientID = value;
        }

        /// <summary>
        /// ClientSecret da Conta do RD Station com que se deseja comunicar.
        /// </summary>
        public string ClientSecret
        {
            get => _ClientSecret;
            private set => _ClientSecret = value;
        }

        /// <summary>
        /// URL de retorno, utilizado para autenticação.
        /// </summary>
        public string oAuth2UrlCallback
        {
            get => _oAuthUrlCallback;
            private set => _oAuthUrlCallback = value;
        }

        /// <summary>
        /// O objeto da API Rest utilizado para fazer a comunicação com a RD, a nível de requests e responses.
        /// </summary>
        public HttpClient InnerClient
        {
            get => _Client;
        }

        /// <summary>
        /// Cria uma nova instância para efetuar comunicação com a RD Station
        /// </summary>
        /// <param name="ClientID">ID encontrado na conta da RD Station, para uso em integrações</param>
        /// <param name="ClientSecret">Secret encontrado na conta da RD Station, para uso em integrações</param>
        /// <param name="AuthCallbackURL">Url de retorno que será utilizada no momento da Autenticação</param>
        public RdStationClient(string ClientID, string ClientSecret, string AuthCallbackURL)
        {
            _ClientID = ClientID;
            _ClientSecret = ClientSecret;
            _oAuthUrlCallback = AuthCallbackURL;
            _OAuth2Url = string.Format(@"https://api.rd.services/auth/dialog?client_id={0}&redirect_uri={1}", _ClientID, _oAuthUrlCallback);
            _Client = new HttpClient() { BaseAddress = new Uri(BASE_ADDRESS) };
            System.Net.WebRequest.DefaultWebProxy = null;
            _Client.SetDefaultHeaders();
        }

        private bool _IsAuthenticated;

        /// <summary>
        /// Retorna um valor indicando se o Client foi autenticado e pode ser utilizado para comunicar com a RD Station.
        /// </summary>
        public bool IsAuthenticated
        {
            get => _IsAuthenticated;
        }

        private bool _Authenticating = false;

        /// <summary>
        /// Retorna um valor indicando se o Client foi autenticado e pode ser utilizado para comunicar com a RD Station.
        /// </summary>
        public bool Authenticating
        {
            get => _Authenticating;
        }


        /// <summary>
        /// Realiza a autenticação com a RD Station para realizar as comunicações. Caso as credenciais tenham expirado, faz a atualização das credenciais de forma automática no back-end.
        /// </summary>
        /// <param name="ReAuthUser">Se verdadeiro, irá solicitar novamente as credenciais ao usuário. Se falso, tentará realizar a autenticação com o token que foi previamente obtido na sessão anterior.</param>
        public void Authenticate(bool ReAuthUser = false)
        {
            //autenticação
            _Authenticating = true;
            if (ReAuthUser)
            {
                try
                {
                    string token = OAuth2Sharp.OAuth2.AuthAsync(_OAuth2Url, _oAuthUrlCallback).Result.Token;
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        _IsAuthenticated = false;
                    }
                    else
                    {
                        Auth(token);
                        _IsAuthenticated = true;
                    }
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                if (RefreshAuth())
                    _IsAuthenticated = true;
                else
                {
                    string token = OAuth2Sharp.OAuth2.AuthAsync(_OAuth2Url, _oAuthUrlCallback).Result.Token;
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        _IsAuthenticated = false;
                    }
                    else
                    {
                        Auth(token);
                        _IsAuthenticated = true;
                    }
                }
            }

            _Authenticating = false;
        }

        /// <summary>
        /// Verifica se é necessário autenticar o usuário.
        /// </summary>
        /// <returns>Verdadeiro se o usuário precisar se autenticar, falso caso contrário.</returns>
        public bool NeedUserAuthentication()
        {
            if (!TestCurrentToken())
            {
                if (!RefreshAuth())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Realiza a autenticação da API RestClient com o RD Station utilizando OAuth 2.
        /// </summary>
        /// <param name="token">Token obtido após login do Integrador na RD Station, realizado atravrés de uma Janela WPF com um Browser em que são solicitadas as credenciais da conta.</param>
        /// <returns></returns>
        private bool Auth(string token)
        {
            var task = _Client.PostAsJsonAsync<oauthflow.oAuthRequestData>("https://api.rd.services/auth/token", new oauthflow.oAuthRequestData() { ClientId = _ClientID, ClientSecret = _ClientSecret, Code = token });
            task.Wait();

            oauthflow.oAuthResponseData response = (oauthflow.oAuthResponseData)Newtonsoft.Json.JsonConvert.DeserializeObject(task.Result.Content.ReadAsStringAsync().Result, typeof(oauthflow.oAuthResponseData));
            Settings.Default.AcessToken = response.AccessToken;
            Settings.Default.RefreshToken = response.RefreshToken;
            Settings.Default.Save();

            _Client.SetToken(response.AccessToken);

            return task.Result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Utilizado internamente para atualizar o token de acesso da conta, com base em um token já existente obtido anteriormente.
        /// </summary>
        /// <returns></returns>
        private bool RefreshAuth()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.RefreshToken)) return false;

            var task = _Client.PostAsJsonAsync<oauthflow.oAuthRequestRefreshTokenData>("https://api.rd.services/auth/token", new oauthflow.oAuthRequestRefreshTokenData() { ClientId = _ClientID, ClientSecret = _ClientSecret, RefreshToken = Settings.Default.RefreshToken });
            task.Wait();

            oauthflow.oAuthResponseData response = (oauthflow.oAuthResponseData)Newtonsoft.Json.JsonConvert.DeserializeObject(task.Result.Content.ReadAsStringAsync().Result, typeof(oauthflow.oAuthResponseData));

            _Client.SetToken(response.AccessToken);

            Settings.Default.AcessToken = response.AccessToken;
            Settings.Default.RefreshToken = response.RefreshToken;
            Settings.Default.Save();
            return task.Result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Utilizado internamente para verificar se o Token atual é válido.
        /// </summary>
        /// <returns></returns>
        private bool TestCurrentToken()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.AcessToken)) return false;
            _Client.SetToken(Settings.Default.AcessToken);
            return IsCurrentTokenValid();
        }

        /// <summary>
        /// Verifica se o token atual é válido.
        /// </summary>
        /// <returns>Verdadeiro se o token atual for válido, falso caso contrário.</returns>
        public bool IsCurrentTokenValid()
        {
            var Lead = new LeadConversionData();
            Lead.Payload.Email = "teste@teste.com.br";
            Lead.Payload.CfVendedor = "Teste";
            Lead.Payload.JobTitle = "Teste";
            Lead.Payload.Name = "Teste";
            Lead.Payload.CompanyName = "Teste";
            Lead.Payload.Country = "Brasil";

            Lead.Payload.Tags = new System.Collections.Generic.List<string>();
            Lead.Payload.Tags.Add("teste");
            Lead.Payload.Tags.Add("teste-01-22");

            JsonSerializerSettings sett = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            sett.Converters.Add(new IntJsonConverter());

            _Client.Timeout = new TimeSpan(0, 1, 0);
            _Client.BaseAddress = new Uri(CONVERSION_URL);
            var task = _Client.PostAsJsonAsync(CONVERSION_URL, Lead, sett);
            task.Wait();

            return task.Result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Envia um Lead para a RD Station de forma síncrona.
        /// </summary>
        /// <param name="lead">Lead a ser enviado</param>
        /// <returns>Retorna true se o lead for enviado com sucesso e false se houve alguma falha no envio.</returns>
        public bool GenerateConversionSync(LeadConversionData Lead)
        {
        //Na conversão todos os campos devem ser enviados como Strings. Por isso, é utilizado um conversor para converter int? para string.
        invalidtokenRetry:
            JsonSerializerSettings sett = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            sett.Converters.Add(new IntJsonConverter());

            var task = _Client.PostAsJsonAsync(CONVERSION_URL, Lead, sett);

            bool b = task.Wait(20000);
            if (!b)
                RefreshAuth();

            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;

            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created))
            {
                Status.Status.Current.ReportProgress(string.Format("Falha na conversão do LEAD {0} | CÓD: {1}", Lead.Payload.Email, Lead.Payload.CfCodigo.ToString()), HL.MVVM.CSVLog.MessageType.Fail);
                Status.Status.Current.ReportProgress("Mensagem de Erro: " + task.Result.Content.ReadAsStringAsync().Result);
                if (task.Result.Content.ReadAsStringAsync().Result.Contains("INVALID_EMAIL_FORMAT"))
                {
                    RDUserLog.Default.LogBadMail(Lead.Payload.Email, Lead.Payload.CfCodigo.ToString());
                }
            }
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            return task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Altera o estágio do Lead identificado pelo email fornecido.
        /// </summary>
        /// <param name="email">O email do Lead.</param>
        /// <param name="Stage">Os dados do estágio a serem atualizados.</param>
        /// <returns>Verdadeiro se a alteração foi bem sucedida, falso caso contrário.</returns>
        public async Task<bool> ChangeLeadStage(string email, contacts.LeadStageData Stage)
        {
            var response = await _Client.PutAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/email:{0}/funnels/default", email), Stage);
            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Atualiza os dados do Lead identificado pelo email fornecido.
        /// </summary>
        /// <param name="email">O email do Lead.</param>
        /// <param name="Lead">Os dados do Lead a serem atualizados.</param>
        /// <returns>Verdadeiro se a atualização foi bem sucedida, falso caso contrário.</returns>
        public async Task<bool> UpdateLead(string email, contacts.LeadUpdateData Lead)
        {
            var response = await _Client.PatchAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/email:{0}", email), Lead,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Sincroniza a atualização dos dados do Lead identificado pelo uuid fornecido.
        /// </summary>
        /// <param name="uuid">O uuid do Lead.</param>
        /// <param name="Lead">Os dados do Lead a serem atualizados.</param>
        /// <returns>Verdadeiro se a atualização foi bem sucedida, falso caso contrário.</returns>
        public bool UpdateLeadSync(string uuid, contacts.LeadUpdateData Lead)
        {
        invalidtokenRetry:
            var task = _Client.PatchAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/uuid:{0}", uuid), Lead,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            task.Wait();
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created))
            {
                Status.Status.Current.ReportProgress(string.Format("Falha no update do LEAD {0} | CÓD: {1}", Lead.Email, Lead.CfCodigo.ToString()), HL.MVVM.CSVLog.MessageType.Fail);
                Status.Status.Current.ReportProgress("Mensagem de Erro: " + task.Result.Content.ReadAsStringAsync().Result);
                if (task.Result.Content.ReadAsStringAsync().Result.Contains("INVALID_EMAIL_FORMAT"))
                {
                    RDUserLog.Default.LogBadMail(Lead.Email, Lead.CfCodigo.ToString());
                }
            }
            return task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Sincroniza a alteração do estágio do Lead identificado pelo email fornecido.
        /// </summary>
        /// <param name="email">O email do Lead.</param>
        /// <param name="Stage">Os dados do estágio a serem atualizados.</param>
        /// <returns>Verdadeiro se a alteração foi bem sucedida, falso caso contrário.</returns>
        public bool ChangeLeadStageSync(string email, contacts.LeadStageData Stage)
        {
        invalidtokenRetry:
            var task = _Client.PutAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/email:{0}/funnels/default", email), Stage,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            task.Wait();
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created))
            {
                Status.Status.Current.ReportProgress("Falha no estágio de FUNIL do Lead " + email, HL.MVVM.CSVLog.MessageType.Fail);
                Status.Status.Current.ReportProgress("Status retornado: " + task.Result.Content.ReadAsStringAsync().Result);
            }
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            return task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created;
        }

        /// <summary>
        /// Retorna os dados do Lead identificado pelo email fornecido.
        /// </summary>
        /// <param name="email">O email do Lead.</param>
        /// <returns>Os dados do Lead ou null se a solicitação falhou.</returns>
        public contacts.LeadResponseData GetLeadSync(string email)
        {
        invalidtokenRetry:
            var task = _Client.GetAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/email:{0}", email));
            task.Wait();
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created)) return null;
            contacts.LeadResponseData response = (contacts.LeadResponseData)Newtonsoft.Json.JsonConvert.DeserializeObject(task.Result.Content.ReadAsStringAsync().Result, typeof(contacts.LeadResponseData));
            return response;
        }

        /// <summary>
        /// Retorna os dados do Lead identificado pelo uuid fornecido.
        /// </summary>
        /// <param name="uuid">O uuid do Lead.</param>
        /// <returns>Os dados do Lead ou null se a solicitação falhou.</returns>
        public contacts.LeadResponseData GetLeadSync2(string uuid)
        {
        invalidtokenRetry:
            var task = _Client.GetAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/uuid:{0}", uuid));
            task.Wait();
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created)) return null;
            contacts.LeadResponseData response = (contacts.LeadResponseData)Newtonsoft.Json.JsonConvert.DeserializeObject(task.Result.Content.ReadAsStringAsync().Result, typeof(contacts.LeadResponseData));
            return response;
        }

        /// <summary>
        /// Exclui o Lead identificado pelo uuid fornecido.
        /// </summary>
        /// <param name="uuid">O uuid do Lead.</param>
        /// <returns>Verdadeiro se a exclusão foi bem sucedida, falso caso contrário.</returns>
        public bool DeleteLeadSync(string uuid)
        {
        invalidtokenRetry:
            var task = _Client.DeleteAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/uuid:{0}", uuid));
            task.Wait();
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            return task.Result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Exclui o Lead identificado pelo email fornecido.
        /// </summary>
        /// <param name="mail">O email do Lead.</param>
        /// <returns>Verdadeiro se a exclusão foi bem sucedida, falso caso contrário.</returns>
        public bool DeleteLeadSync2(string mail)
        {
        invalidtokenRetry:
            var task = _Client.DeleteAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/email:{0}", mail));
            task.Wait();
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            return task.Result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Registra a venda do lead.
        /// </summary>
        /// <param name="op">Os dados da conversão.</param>
        /// <returns>Verdadeiro se a conversão foi registrada com sucesso, falso caso contrário.</returns>
        public bool WonOpportunity(events.Won_Oportunity op)
        {
        invalidtokenRetry:
            var task = _Client.PostAsJsonAsync(CONVERSION_URL, op,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
            task.Wait();
            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created))
            {
                Status.Status.Current.ReportProgress("Falha no registro da venda do lead: " + op.Payload.Email, HL.MVVM.CSVLog.MessageType.Fail);
                Status.Status.Current.ReportProgress("Status retornado: " + task.Result.Content.ReadAsStringAsync().Result);
            }
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            return task.Result.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retorna os dados do funil do Lead identificado pelo email fornecido.
        /// </summary>
        /// <param name="email">O email do Lead.</param>
        /// <returns>Os dados do funil do Lead ou null se a solicitação falhou.</returns>
        public funnels.Funnel_Response_Data GetLeadFunnel(string email)
        {
        invalidtokenRetry:
            var task = _Client.GetAsJsonAsync(string.Format("https://api.rd.services/platform/contacts/email:{0}/funnels/default", email));
            task.Wait();
            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;
            funnels.Funnel_Response_Data response = (funnels.Funnel_Response_Data)Newtonsoft.Json.JsonConvert.DeserializeObject(task.Result.Content.ReadAsStringAsync().Result, typeof(funnels.Funnel_Response_Data));
            return response;
        }

        /// <summary>
        /// Retorna todos os leads na segmentação padrão.
        /// </summary>
        /// <returns>Os dados dos leads na segmentação padrão.</returns>
        public SegmentationResponseData GetLeadsFromSegmentationRDStation(string SegmentationID)
        {
            //usado para exportar apenas a primeira paginação de leads
            bool test = false;

            //O RD retorna os Leads de uma segmentação em "páginas" de no máximo 125 "linhas".
            //A lógica abaixo irá "folhar" estas páginas retornando todos os leads da segmentação principal, ou seja, todos os leads da base.
            SegmentationResponseData r = new SegmentationResponseData();
            r.contacts = new System.Collections.Generic.List<SegmentationContact>();

            Status.Status.Current.ReportProgress("Iniciando acesso à base de leads da RD Station");

            int page = 1;
        invalidtokenRetry:
        startpoint:
            int retrials = 1;
        retry:
            System.Threading.Thread.Sleep(1000);
            var task = _Client.GetAsJsonAsync(string.Format("https://api.rd.services/platform/segmentations/{0}/contacts?page={1}&page_size={2}", SegmentationID, page.ToString(), "125"));
            task.Wait();

            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;

            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created))
            {
                retrials += 1;
                if (retrials < 4)
                    goto retry;
                else
                    return null;
            }
            segmentations.SegmentationResponseData response = (segmentations.SegmentationResponseData)Newtonsoft.Json.JsonConvert.DeserializeObject(task.Result.Content.ReadAsStringAsync().Result, typeof(segmentations.SegmentationResponseData));

            r.contacts.AddRange(response.contacts);

            //header: "pagination-total-pages" index is 9
            int totalPages = 1;

            HttpResponseMessage result = task.Result;
            var pagination_total_pages = result.Headers.ElementAt(9);
            int.TryParse(pagination_total_pages.Value?.ElementAt(0), out totalPages);
            Status.Status.Current.ReportProgress($"{page * 125} leads processados de um total esperado de {totalPages * 125}", page * 125, totalPages * 125);
            if (page < totalPages && !test)
            {
                page += 1;
                goto startpoint;
            }

            return r;
        }

        /// <summary>
        /// Obtém todas as informações de segmentação de leads da plataforma RD Station.
        /// </summary>
        /// <returns>
        /// Um objeto SegmentationInfosResponseData contendo uma lista de informações de segmentação.
        /// </returns>
        public SegmentationInfosResponseData GetSegmentations()
        {
            //usado para exportar apenas a primeira paginação de leads
            bool test = false;

            //O RD retorna os Leads de uma segmentação em "páginas" de no máximo 125 "linhas".
            //A lógica abaixo irá "folhar" estas páginas retornando todos os leads da segmentação principal, ou seja, todos os leads da base.
            SegmentationInfosResponseData r = new SegmentationInfosResponseData();
            r.segmentations = new System.Collections.Generic.List<SegmentationInfo>();

            Status.Status.Current.ReportProgress("Iniciando acesso às segmentações da conta da RD Station");

            int page = 1;
        invalidtokenRetry:
        startpoint:
            int retrials = 1;
        retry:
            System.Threading.Thread.Sleep(1000);
            var task = _Client.GetAsJsonAsync($"https://api.rd.services/platform/segmentations?page={page}&page_size=125");
            task.Wait();

            CheckResponseAuthStatus(task.Result.Content.ReadAsStringAsync().Result);
            if (ShallRetry(task.Result.Content.ReadAsStringAsync().Result)) goto invalidtokenRetry;

            if (!(task.Result.StatusCode == HttpStatusCode.OK || task.Result.StatusCode == HttpStatusCode.Created))
            {
                retrials += 1;
                if (retrials < 4)
                    goto retry;
                else
                    return null;
            }
            segmentations.SegmentationInfosResponseData response = (segmentations.SegmentationInfosResponseData)Newtonsoft.Json.JsonConvert.DeserializeObject(task.Result.Content.ReadAsStringAsync().Result, typeof(segmentations.SegmentationInfosResponseData));

            r.segmentations.AddRange(response.segmentations);

            //header: "pagination-total-pages" index is 9
            int totalPages = 1;

            HttpResponseMessage result = task.Result;
            var pagination_total_pages = result.Headers.ElementAt(9);
            int.TryParse(pagination_total_pages.Value?.ElementAt(0), out totalPages);
            Status.Status.Current.ReportProgress($"{page * 125} segmentações processadas de um total esperado de {totalPages * 125}", page * 125, totalPages * 125);
            if (page < totalPages && !test)
            {
                page += 1;
                goto startpoint;
            }

            return r;
        }

        /// <summary>
        /// Verifica o status de autenticação da resposta e autentica novamente se a autenticação falhou.
        /// </summary>
        /// <param name="responseContent">O conteúdo da resposta.</param>
        /// <returns>Verdadeiro se a autenticação é bem sucedida, falso caso contrário.</returns>
        private bool CheckResponseAuthStatus(string responseContent)
        {
            if (Authenticating) return false;
            if (responseContent.Contains("invalid_token"))
            {
                Status.Status.Current.ReportProgress("Returned: Invalid Token.", HL.MVVM.CSVLog.MessageType.Error);
                Status.Status.Current.ReportProgress("Trying to authenticate again!", HL.MVVM.CSVLog.MessageType.Warning);
                try
                {
                    Authenticate();
                }
                catch
                {

                }
            }
            return IsAuthenticated;
        }

        private int retries = 0;

        /// <summary>
        /// Verifica se é necessário repetir uma solicitação que falhou devido a um token inválido.
        /// </summary>
        /// <param name="responseContent">O conteúdo da resposta.</param>
        /// <returns>Verdadeiro se a solicitação deve ser repetida, falso caso contrário.</returns>
        private bool ShallRetry(string responseContent)
        {
            bool r = responseContent.Contains("invalid_token");

            if (r && IsAuthenticated)
            {
                retries += 1;
                if (retries < 30)
                {
                    System.Threading.Thread.Sleep(10000);
                    Status.Status.Current.ReportProgress("Repetindo comando...");
                    return true;
                }

            }
            return false;
        }
    }
}