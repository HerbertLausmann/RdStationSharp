using RdStationSharp.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using RdStationSharp.Helper;
using RestSharp;


namespace RdStationSharp
{
    public class RDStationSharp : HL.MVVM.ViewModelBase
    {
        private Data.RDDataSource _Source;
        private RDs.RdStationClient _Client;

        public RDDataSource Source { get => _Source; private set => _Source = value; }

        private readonly int diasAtivo = 100;
        private readonly int diasInativo = 120;
        private readonly int diasSuperInativo = 485;


        private HL.MVVM.Threading.ThreadRelayCommand.ThreadRelayCommandExecuteEventArgs currentTask = null;

        public HL.MVVM.Threading.ThreadRelayCommand SyncCommand =>
           GetCommand(new HL.MVVM.Threading.ThreadRelayCommand(
               new HL.MVVM.Threading.ThreadRelayCommand.ThreadRelayCommandExecute((
                   HL.MVVM.Threading.ThreadRelayCommand.ThreadRelayCommandExecuteEventArgs e) =>
               {
                   currentTask = e;
                   Helper.RDUserLog.Default.Write("Autenticando");
                   e.ReportProgressText("Autenticando...");
                   _Client = new RDs.RdStationClient("2bf2cb3b-6a90-4e36-a03b-b936881fb615", "584289722db640d487f9c0df95472d7a", @"https://segurimax.com.br/auth/callback");
                   try
                   {
                       _Client.Authenticate();
                   }
                   catch
                   {

                   }

                   if (!_Client.Authorized)
                   {
                       e.ReportProgressText("Não foi possível se conectar com a RD Station;");
                       Helper.RDUserLog.Default.Write("Não foi possível se conectar com o RD Station!");
                       Helper.RDUserLog.Default.Finish();
                       return;
                   }

                   //var r = _Client.GetAllLeadsOnRDStation();
                   //var test = _Client.GetLeadSync("herbert.l@segurimax.com.br");
                   //
                   e.ReportProgressText("Carregando base de dados...");
                   Source = new RDDataSource();

                   try
                   {

                       Source.LoadDataSourceV2();

                       e.ReportProgressText("Corrigindo campos de e-mails e removendo duplicatas...");
                       Source.CorrectMailField();

                       e.ReportProgressText("Realizando filtragem na base...");

                       Source.ExportBase();

                       Source.GenerateDefaultFiltersV2();
                       Source.RunFiltering();

                       e.ReportProgressText("Iniciando sincronização com a RD Station...");
                   }
                   catch (Exception ee)
                   {

                   }

                   try
                   {
                       e.ReportProgressText("Carregando base da RD Station");
                       Helper.RDUserLog.Default.Write("Carregando base da RD Station");
                   }
                   catch
                   {

                   }

                   e.ReportTotalProgress(Source.Table.Rows.Count);

                   List<RDs.Lead> leads = new List<RDs.Lead>();
                   List<RDs.Lead> priorityLeads = new List<RDs.Lead>();

                   int totalArquivoMorto = 0;
                   //sincroniza todos os cadastros
                   for (int i = 0; i < Source.Table.Rows.Count; i++)
                   {
                       try
                       {

                           e.ReportProgress(i + 1);

                           DataRow currentRow = Source.Table.Rows[i];
                           e.ReportProgressText(string.Format("Preparando Lead {0} de {1}", i + 1, Source.Table.Rows.Count));

                           if (string.IsNullOrWhiteSpace(currentRow["E-mail"].ToString()))
                           {
                               e.ReportProgressText(string.Format("Lead de número {0} é inválido.", i + 1));
                               continue;
                           }

                           //Se o lead constar na lista de desengajados / softbounces, ele será ignorado no upload.
                           e.ReportProgressText(string.Format("Verificando cadastro {0} de {1}", i + 1, Source.Table.Rows.Count));

                           if (IgnoreDataBase.Default.IsOptOut(currentRow["E-mail"].ToString()))
                           {
                               continue;
                           }

                           if (currentRow["Status"].ToString() == "Morto")
                           {
                               e.ReportProgressText(string.Format("Cadastro {0} de {1} é Arquivo Morto e será ignorado.", i + 1, Source.Table.Rows.Count));
                               totalArquivoMorto += 1;
                               continue;
                           }

                           if (!IsEngaged(currentRow["E-mail"].ToString()))
                           {

                               int diasSemCompra = 2000;
                               int.TryParse(currentRow["QTDIASULTIMACOMPRA"].ToString(), out diasSemCompra);

                               //se o lead tiver comprado nos últimos 5 anos, mesmo sendo desengajado ele irá para o RD Station
                               if (diasSemCompra > 1825)
                               {
                                   e.ReportProgressText(string.Format("Cadastro {0} de {1} é um lead desengajado. Ignorando...", i + 1, Source.Table.Rows.Count));
                                   continue;
                               }

                           }

                           //caso o lead não conste na lista de desengajados, seguirá para upload

                           //Leads com compra hoje são colocados com prioridade para serem feito upload

                           var lead = LeadFromROW(currentRow);

                           var dataInclusao = DateTime.MinValue;
                           DateTime.TryParse(currentRow["DtInclusao"].ToString(), out dataInclusao);

                           if (lead.Payload.CfDiasSemCompra == "0")
                               priorityLeads.Add(lead);
                           else
                           {
                               if (dataInclusao.Date == DateTime.Today)
                                   priorityLeads.Add(lead);
                               else
                                   leads.Add(lead);
                           }

                       }
                       catch (Exception thisE)
                       {
                           e.ReportProgressText(thisE.ToString());
                       }
                   }

                   Helper.RDUserLog.Default.Write("Total de Leads para sincronizar: " + (leads.Count + priorityLeads.Count));
                   Helper.RDUserLog.Default.Write("Sincronizando Leads.");
                   e.ReportProgressText("Iniciando sincronização de Leads prioritários!");
                   SyncLeads(priorityLeads);
                   Helper.RDUserLog.Default.Write("Sincronização de Leads prioritários concluída!");
                   e.ReportProgressText("Iniciando sincronização de Leads secundários!");
                   SyncLeads(leads);

                   e.ReportProgressText("Sincronização finalizada.");
                   Helper.RDUserLog.Default.Write("Sincronização concluída!");

                   IgnoreDataBase.Default.Dispose();

                   //desligamento automático
                   var doShutdown = true;
                   Helper.RDUserLog.Default.Finish();
                   var t = Task.Run(() =>
                   {
                       System.Threading.Thread.Sleep(60000);
                       if (doShutdown)
                       {
                           Process.Start("shutdown", "/s /f");
                           System.Windows.Application.Current.Dispatcher.Invoke(() =>
                           {
                               System.Windows.Application.Current.Shutdown();
                           });
                       }
                   });
                   e.ReportProgressText("Desligamento automático programado!");

                   System.Windows.Application.Current.Dispatcher.Invoke(() =>
                              {
                                  if (System.Windows.MessageBox.Show("O computador será desligado!", "Desligamento em 1 Minuto!", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Exclamation) == System.Windows.MessageBoxResult.Cancel)
                                      doShutdown = false;
                              });

                   e.ReportProgressText("Thread de sincronização finalizada.");
               }),

               (object o) =>
               {
                   return !SyncCommand.IsRunning;
               }), "Sync");
        public void Sync()
        {
            if (SyncCommand.CanExecute(null)) SyncCommand.Execute(null);
        }

        private void SyncLeads(List<RDs.Lead> leads)
        {
            //sincroniza o cadastro na RD Station e gera conversão se necessário.
            currentTask.ReportProgress(0);
            currentTask.ReportTotalProgress(leads.Count);
            for (int i = 0; i < leads.Count; i++)
            {
                try
                {
                    //500
                    System.Threading.Thread.Sleep(10);
                    currentTask.ReportProgress(i + 1);
                    currentTask.ReportProgressText(string.Format("Sincronizando Lead {0} de {1}", i + 1, leads.Count));

                    if (SyncLead(leads[i]))
                    {
                        currentTask.ReportProgressText(string.Format("Lead {0} sincronizado com sucesso!", leads[i].Payload.Email));
                    }
                    else
                    {
                        currentTask.ReportProgressText(string.Format("Lead {0} falhou no envio", leads[i].Payload.Email));
                    }
                    //300
                    System.Threading.Thread.Sleep(10);
                }
                catch (Exception thisE)
                {
                    currentTask.ReportProgressText(thisE.ToString());
                }

            }

            //sincroniza todos os estágios de funil
            currentTask.ReportProgress(0);
            currentTask.ReportTotalProgress(leads.Count);
            for (int i = 0; i < leads.Count; i++)
            {
                try
                {
                    //500
                    System.Threading.Thread.Sleep(10);
                    currentTask.ReportProgress(i + 1);
                    currentTask.ReportProgressText(string.Format("Atualizando estágio de funil do cadastro {0} de {1}", i + 1, leads.Count));

                    if (UpdateLeadFunnel(leads[i]))
                    {
                        currentTask.ReportProgressText(string.Format("Lead {0} atualizado com sucesso!", leads[i].Payload.Email));
                    }
                    else
                    {
                        currentTask.ReportProgressText(string.Format("Lead {0} falhou no envio...", leads[i].Payload.Email));
                    }
                }
                catch (Exception thisE)
                {
                    currentTask.ReportProgressText(thisE.ToString());
                }

            }

            //sincroniza todas as vendas
            currentTask.ReportProgress(0);
            currentTask.ReportTotalProgress(leads.Count);
            for (int i = 0; i < leads.Count; i++)
            {
                try
                {
                    //sincroniza todas as vendas
                    System.Threading.Thread.Sleep(10);
                    currentTask.ReportProgress(i + 1);
                    currentTask.ReportProgressText(string.Format("Atualizando VENDA {0} de {1}", i + 1, leads.Count));

                    if (CheckMarkSale(leads[i]))
                    {
                        currentTask.ReportProgressText(string.Format("Lead {0} atualizado com sucesso!", leads[i].Payload.Email));
                    }
                    else
                    {
                        currentTask.ReportProgressText(string.Format("Lead {0} falhou no envio...", leads[i].Payload.Email));
                    }
                }
                catch (Exception thisE)
                {
                    currentTask.ReportProgressText(thisE.ToString());
                }
            }
        }

        private readonly string disengagedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BI", "disengaged.txt");
        private List<string> Disengaged;
        private bool IsEngaged(string email)
        {
            if (Disengaged is null)
            {
                if (!File.Exists(disengagedFilePath)) return true;
                Disengaged = new List<string>();
                using (StreamReader reader = new StreamReader(disengagedFilePath, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        Disengaged.Add(reader.ReadLine());
                    }
                }
            }

            return !Disengaged.Contains(email);
        }

        private RDs.Lead LeadFromROW(DataRow data)
        {
            RDs.Lead lead = new RDs.Lead();
            lead.Payload.CfCodigo = GetRowField(data, "CdCliente");
            lead.Payload.CfRazaoSocial = GetRowField(data, "Razão Social");
            lead.Payload.CfDdd = GetRowField(data, "DDD");
            lead.Payload.City = GetRowField(data, "Cidade");
            lead.Payload.State = GetRowField(data, "UF");
            lead.Payload.Email = GetRowField(data, "E-mail");
            lead.Payload.CfRamoDeAtividade = GetRowField(data, "Descrição Atividade");
            lead.Payload.CfPerfilCliente = GetRowField(data, "TIPO_CONTA");
            lead.Payload.CfDiasSemCompra = Helper.Helper.GetNumbers(GetRowField(data, "QTDIASULTIMACOMPRA"));
            lead.Payload.CfQtdCompras = Helper.Helper.GetNumbers(GetRowField(data, "QTCOMPRA"));
            lead.Payload.CompanyName = GetRowField(data, "Fantasia");

            lead.Payload.CfGerente = GetRowField(data, "ULTIMOGERENTEVENDEU");
            lead.Payload.CfVendedor = GetRowField(data, "ULTIMOVENDEDORVENDEU");

            lead.Payload.Cf_inadimplente = GetRowField(data, "INADIMPLENTE");

            lead.Payload.Cf_grandes_contas = GetRowField(data, "GRANDESCONTAS");

            if (Helper.Helper.TryParseStringToInt2(GetRowField(data, "QTDIASULTNEGOCIOINCLUIDO")) > 5 & Helper.Helper.TryParseStringToInt2(GetRowField(data, "QTDIASULTNEGOCIOINCLUIDO")) <= 40)
                lead.Payload.Cf_negociacao_aberta_ultimos_30_dias = "SIM";
            else
                lead.Payload.Cf_negociacao_aberta_ultimos_30_dias = "NAO";

            int qtComprFiliais = Helper.Helper.TryParseStringToInt2(GetRowField(data, "JACOMPROU"));
            if (qtComprFiliais == 1)
                lead.Payload.Cf_a_rede_possui_compra = "SIM";
            else if (qtComprFiliais == 0)
                lead.Payload.Cf_a_rede_possui_compra = "NAO";


            int qtCompr = Helper.Helper.TryParseStringToInt2(lead.Payload.CfQtdCompras);
            int diasSemCompra = Helper.Helper.TryParseStringToInt2(lead.Payload.CfDiasSemCompra);

            if (qtCompr > 0 & diasSemCompra <= diasAtivo)
            {
                lead.Payload.CfStatusComercial = "Cliente Ativo";
            }
            else if (qtCompr > 0 & diasSemCompra > diasAtivo & diasSemCompra <= diasInativo)
            {
                lead.Payload.CfStatusComercial = "Cliente Semi-inativo";
            }
            else if (qtCompr > 0 & diasSemCompra > diasInativo & diasSemCompra <= diasSuperInativo)
            {
                lead.Payload.CfStatusComercial = "Cliente Inativo";
            }
            else if (qtCompr > 0 & diasSemCompra > diasSuperInativo)
            {
                lead.Payload.CfStatusComercial = "Cliente Super Inativo (Mais de 1 ano)";
            }
            else if (qtCompr == 0 & qtComprFiliais <= 0)
            {
                lead.Payload.CfStatusComercial = "Prospect";
            }
            else if (qtCompr == 0 & qtComprFiliais == 1)
            {
                lead.Payload.CfStatusComercial = "Unidade sem compras";
            }

            if (lead.Payload.Email == "metasulextintores@gmail.com")
                lead.Payload.Email = "metasulextintores@gmail.com";

            return lead;

        }

        private string GetRowField(DataRow r, string field)
        {
            string str = null;
            try
            {
                str = r[field].ToString().Trim();
            }
            catch { }
            return str;
        }

        private bool SyncLead(RDs.Lead lead)
        {
            currentTask.ReportProgressText(">>> Getting data from RD Station: " + lead.Payload.Email);
            var leadRD = _Client.GetLeadSync(lead.Payload.Email);

            if (leadRD is null)
                currentTask.ReportProgressText(">>> No data found for Lead " + lead.Payload.Email);
            else
            {
                //processamento de lógicas de ERP vs. RD Station
                currentTask.ReportProgressText(">>> Processing logics between Local and Cloud for " + lead.Payload.Email);
                lead = ProcessarReativacao(lead, leadRD);
            }

            if (string.IsNullOrEmpty(leadRD?.Email))
            {
                currentTask.ReportProgressText(">>> Generating conversion for Lead " + lead.Payload.Email);
                var r = GerarConversao(lead);
                if (r)
                    return true;
                else
                    return false;
            }
            else
            {
                if (((leadRD.CfStatusComercial == "Prospect") | string.IsNullOrWhiteSpace(leadRD.CfStatusComercial)) & (lead.Payload.CfStatusComercial == "Cliente Ativo"))
                {
                    currentTask.ReportProgressText(">>> It's a Lead on RD Station. Generating conversion " + lead.Payload.Email);
                    var r = GerarConversao(lead);
                    if (r)
                        return true;
                    else
                        return false;
                }
                else
                {
                    var u = UpdateLead(lead, leadRD.Uuid);
                    return u;
                }
            }
        }

        private bool GerarConversao(RDs.Lead lead)
        {
            return _Client.GenerateConversionSync(lead);
        }

        private bool UpdateLead(RDs.Lead lead, string uuid)
        {
            currentTask.ReportProgressText(">>> Updating Lead on RD Station " + lead.Payload.Email);
            var r = _Client.UpdateLeadSync(uuid, RDs.contacts.LeadUpdateData.FromLead(lead));
            if (r)
                currentTask.ReportProgressText(">>> Lead update success: " + lead.Payload.Email);
            else
            {
                currentTask.ReportProgressText(">>> Lead update FAILED: " + lead.Payload.Email);
            }

            return r;
        }

        private bool UpdateLeadFunnel(RDs.Lead lead)
        {
            var _currentStage = _Client.GetLeadFunnel(lead.Payload.Email);
            if (lead.Payload.CfQtdCompras != "0")
            {
                if (_currentStage == null | (_currentStage?.LifecycleStage != RDs.contacts.LeadStageData.Client))
                {
                    currentTask.ReportProgressText(">>> Updating Lead Funnel Stage: " + lead.Payload.Email);
                    var r = _Client.ChangeLeadStageSync(lead.Payload.Email, new RDs.contacts.LeadStageData() { LifecycleStage = RDs.contacts.LeadStageData.Client });
                    if (r)
                        currentTask.ReportProgressText(">>> Lead Stage update success: " + lead.Payload.Email);
                    else
                    {
                        currentTask.ReportProgressText(">>> Lead Stage update FAILED: " + lead.Payload.Email);
                    }
                    return r;
                }
            }
            else
            {
                if (_currentStage == null | (_currentStage?.LifecycleStage == RDs.contacts.LeadStageData.Lead))
                {
                    var r = _Client.ChangeLeadStageSync(lead.Payload.Email, new RDs.contacts.LeadStageData() { LifecycleStage = RDs.contacts.LeadStageData.QualifiedLead, Opportunity = true });
                    if (r)
                        currentTask.ReportProgressText(">>> Lead Stage update success: " + lead.Payload.Email);
                    else
                    {
                        currentTask.ReportProgressText(">>> Lead Stage update FAILED: " + lead.Payload.Email);
                    }

                    return r;
                }
            }
            return true;
        }

        private bool CheckMarkSale(RDs.Lead lead)
        {
            if (lead.Payload.CfDiasSemCompra == "0")
            {
                currentTask.ReportProgressText("Marking sale on: " + lead.Payload.Email);
                var r = _Client.WonOpportunity(new RDs.events.Won_Oportunity() { Payload = new RDs.events.Won_Oportunity_Payload() { Email = lead.Payload.Email } });
                if (r)
                    currentTask.ReportProgressText(">>> Lead sale mark success: " + lead.Payload.Email);
                else
                {
                    currentTask.ReportProgressText(">>> Lead sale mark FAILED: " + lead.Payload.Email);
                }

                return r;
            }
            return true;
        }

        private RDs.Lead ProcessarReativacao(RDs.Lead leadERP, RDs.contacts.LeadResponseData leadRDs)
        {
            int qtCompr = Helper.Helper.TryParseStringToInt2(leadERP.Payload.CfQtdCompras);
            int diasSemCompra = Helper.Helper.TryParseStringToInt2(leadERP.Payload.CfDiasSemCompra);
            int diasSemCompraRDSTATION = Helper.Helper.TryParseStringToInt2(leadRDs.CfDiasSemCompra);

            if (qtCompr > 0)
            {
                if (diasSemCompraRDSTATION > diasInativo)
                {
                    if (diasSemCompra == 0)
                    {
                        leadERP.Payload.Tags = new List<string>();

                        var strReativacao = string.Format("reativado-{0}-{1}", DateTime.Today.Month, DateTime.Today.Year);

                        try
                        {
                            if (!(leadRDs.Tags is null))
                                leadERP.Payload.Tags.AddRange(leadRDs.Tags.Where(l => l != strReativacao));
                        }
                        catch
                        {

                        }

                        leadERP.Payload.Tags.Add(strReativacao);
                        currentTask.ReportProgressText(strReativacao);
                    }
                }
            }

            return leadERP;
        }
    }
}
