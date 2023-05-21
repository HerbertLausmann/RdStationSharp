using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RdStationSharp.Data
{
    /// <summary>
    /// Classe que permite carregar e acessar localmente a Base completa de Leads da RD Station.
    /// </summary>
    public class RDCloud
    {
        private List<RDs.contacts.LeadResponseData> _leads;
        private RDs.segmentations.SegmentationResponseData _disengaged;
        private RDs.segmentations.SegmentationResponseData _leadsBasicData;
        /// <summary>
        /// Obtém a lista de leads.
        /// </summary>
        public List<RDs.contacts.LeadResponseData> Leads
        {
            get
            {
                if (_leads is null)
                {
                    _leads = new List<RDs.contacts.LeadResponseData>();
                }
                return _leads;
            }
        }

        private RdStationSharp.RDs.RdStationClient _source;

        private static RDCloud _Default;

        /// <summary>
        /// Obtém a instância padrão da classe RDCloud.
        /// </summary>
        public static RDCloud Default
        {
            get
            {
                if (_Default is null)
                    _Default = new RDCloud();
                return _Default;
            }

        }
        /// <summary>
        /// Inicializa a instância RDCloud carregando os dados de leads do cliente de origem.
        /// </summary>
        /// <param name="sourceClient">A instância RdStationClient para carregar dados de lead.</param>
        public void Initialize(RdStationSharp.RDs.RdStationClient sourceClient)
        {
            _source = sourceClient;
            //5214587 é o ID da Base Completa
            _leadsBasicData = _source.GetLeadsFromSegmentationRDStation("5214587");

            Status.Status.Current.ReportProgress("Loading RD Cloud Data!");
            Status.Status.Current.ReportProgress(0, _leadsBasicData.contacts.Count);
            int current = 1;
            foreach (var lead in _leadsBasicData.contacts)
            {
                Status.Status.Current.ReportProgress(current, _leadsBasicData.contacts.Count);
                Status.Status.Current.ReportProgress(string.Format("Loading RD Cloud Data for lead {0} de {1}!", current, _leadsBasicData.contacts.Count));
                try
                {
                    int trials = 1;
                init:
                    var l = _source.GetLeadSync2(lead.uuid);
                    if (l is null && trials < 4)
                    {
                        trials += 1;
                        System.Threading.Thread.Sleep(3000);
                        goto init;
                    }
                    if (!(l is null))
                        Leads.Add(l);
                    else
                        Status.Status.Current.ReportProgress(string.Format("Lead Failed {0} de {1}!", current, _leadsBasicData.contacts.Count));
                    System.Threading.Thread.Sleep(100);
                }
                catch
                {
                    Status.Status.Current.ReportProgress(string.Format("Lead Failed {0} de {1}!", current, _leadsBasicData.contacts.Count));
                }
                current += 1;
            }
        }

        /// <summary>
        /// Limpa a nuvem removendo leads com base legal recusada e processando outras ações relacionadas.
        /// </summary>
        public void ClearOptOut()
        {
            int optIn = 0;
            int sucessfullyRemoved = 0;
            int failedRemove = 0;
            for (int i = 0; i < _leads.Count; i++)
            {
                try
                {
                    Status.Status.Current.ReportProgress($"Processando lead {i} de {_leads.Count}", i, _leads.Count);
                    var item = Leads[i];
                    var col = item.LegalBases.Where(l => l.Status == "declined");
                    if (col.Count() > 0)
                    {
                        Leads.RemoveAt(i);
                        i -= 1;
                        if (_source.DeleteLeadSync(item.Uuid))
                        {
                            OptOutDataBase.Default.OptOutLead(item.Email);
                            sucessfullyRemoved += 1;
                        }
                        else
                            failedRemove += 1;
                        System.Threading.Thread.Sleep(400);
                    }
                    else
                    {
                        var oi = OptOutDataBase.Default.OptInLead(item.Email);
                        if (oi)
                            optIn += 1;
                    }

                }
                catch
                {
                    failedRemove += 1;
                }
            }

            Status.Status.Current.ReportProgress("Total de Leads OPT-IN: " + optIn.ToString());

            Status.Status.Current.ReportProgress("Total de Leads removidos da RD Station: " + sucessfullyRemoved.ToString());
            Status.Status.Current.ReportProgress("Total de falhas ao remover Leads da RD Station: " + failedRemove.ToString());
        }

        /// <summary>
        /// Limpa a nuvem removendo leads que estão na base a mais de 180 dias sem engajar com nenhum e-mail recebido
        /// </summary>
        public void ClearDisengaged()
        {
            //6053210 é o ID da Segmentação de Desengajados
            _disengaged = _source.GetLeadsFromSegmentationRDStation("6053210");

            if (_disengaged.contacts.Count == 0)
            {
                Status.Status.Current.ReportProgress("Falha ao obter a lista de desengajados da RD Station", HL.MVVM.CSVLog.MessageType.Fail);
                Status.Status.Current.ReportProgress("Cancelando limpeza de desengajados", HL.MVVM.CSVLog.MessageType.Fail);
                return;
            }

            var toDelete = _disengaged.contacts.Where(contact => contact.created_at.IsOlderThan(180)).ToList();

            int sucessfullyRemoved = 0;
            int failedRemove = 0;

            if (toDelete.Count() > 0)
            {
                foreach (var contact in toDelete)
                {
                    Status.Status.Current.ReportProgress($"Processando lead {toDelete.IndexOf(contact)} de {toDelete.Count}", toDelete.IndexOf(contact), toDelete.Count);
                    try
                    {
                        if (_source.DeleteLeadSync(contact.uuid))
                        {
                            DisengagedDataBase.RDStation.Disengage(contact.email);
                            _disengaged.contacts.Remove(contact);
                            sucessfullyRemoved += 1;
                        }
                        else
                            failedRemove += 1;
                        System.Threading.Thread.Sleep(400);
                    }
                    catch
                    {
                        failedRemove += 1;
                    }
                }
            }

            int reengaged = 0;
            for (int i = 0; i < _leadsBasicData.contacts.Count; i++)
            {
                try
                {
                    var item = _leadsBasicData.contacts[i];
                    if (item.created_at.IsOlderThan(180) == false)
                    {
                        var e = DisengagedDataBase.RDStation.Engage(item.email);
                        if (e)
                            reengaged += 1;
                    }
                }
                catch
                {

                }
            }
            Status.Status.Current.ReportProgress("Total de Leads RE-ENGAJADOS: " + reengaged.ToString());
            Status.Status.Current.ReportProgress("Total de Leads desengajados removidos da RD Station: " + sucessfullyRemoved.ToString());
            Status.Status.Current.ReportProgress("Total de falhas ao remover Leads desengajados da RD Station: " + failedRemove.ToString());
        }

        /// <summary>
        /// Exporta os dados dos leads em um formato específico para uso no Facebook.
        /// </summary>
        /// <returns>Um objeto DataTable contendo os dados dos leads para exportação.</returns>
        public DataTable ExportFacebookPublic(bool leads, bool prospects, bool customers)
        {
            var table = new DataTable();
            table.TableName = "Facebook Ready Audience DataTable";
            table.Columns.Add("email", typeof(string));
            //table.Columns.Add("email2", typeof(string));
            //table.Columns.Add("email3", typeof(string));
            table.Columns.Add("phone", typeof(string));
            //table.Columns.Add("phone4", typeof(string));
            //table.Columns.Add("phone5", typeof(string));
            //table.Columns.Add("madid", typeof(string));
            table.Columns.Add("fn", typeof(string));
            table.Columns.Add("ln", typeof(string));
            //table.Columns.Add("zip", typeof(string));
            table.Columns.Add("ct", typeof(string));
            table.Columns.Add("st", typeof(string));
            table.Columns.Add("country", typeof(string));
            //table.Columns.Add("dob", typeof(string));
            //table.Columns.Add("doby", typeof(string));
            //table.Columns.Add("gen", typeof(string));
            //table.Columns.Add("age", typeof(string));
            //table.Columns.Add("uid", typeof(string));

            var selectedLeads = Leads.Where(lead => (lead.CfStatusComercial?.Contains("Cliente") == true && customers)
                                                || (lead.CfStatusComercial?.Contains("Unidade sem compras") == true && customers)
                                                || (lead.CfStatusComercial?.Contains("Prospect") == true && prospects)
                                                || (string.IsNullOrWhiteSpace(lead.CfStatusComercial) && leads));

            foreach (var item in selectedLeads)
            {
                bool ignore = false;
                if ((!string.IsNullOrWhiteSpace(item.Country)) && ((item.Country?.ToLower().Trim() != "brasil" || item.Country?.ToLower().Trim() != "br")))
                    ignore = true;

                if (!ignore)
                {
                    DataRow r = table.NewRow();
                    r["email"] = item.Email;
                    r["phone"] = item.MobilePhone;
                    r["fn"] = item.Name?.FirstName();
                    r["ln"] = item.Name?.LastName();
                    r["ct"] = item.City;
                    r["st"] = item.State;
                    r["country"] = "BR";
                    table.Rows.Add(r);
                }
            }
            return table;
        }

        /// <summary>
        /// Obtém um lead pelo email fornecido.
        /// </summary>
        /// <param name="mail">O email para buscar o lead.</param>
        /// <returns>O objeto LeadResponseData correspondente ao email fornecido.</returns>
        public RDs.contacts.LeadResponseData GetLeadByEmail(string mail)
        {
            var lead = Leads.First(l => l.Email == mail);
            return lead;
        }

        /// <summary>
        /// Obtém um lead pelo UUID fornecido.
        /// </summary>
        /// <param name="Uuid">O UUID para buscar o lead.</param>
        /// <returns>O objeto LeadResponseData correspondente ao UUID fornecido.</returns>
        public RDs.contacts.LeadResponseData GetLeadByUuid(string Uuid)
        {
            var lead = Leads.First(l => l.Uuid == Uuid);
            return lead;
        }
    }
}
