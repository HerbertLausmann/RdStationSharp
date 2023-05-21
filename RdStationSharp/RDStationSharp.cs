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
using RdStationSharp.RDs.contacts;
using RdStationSharp.ADS;
using System.Threading;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using RdStationSharp.Status;

namespace RdStationSharp
{
    public class RDStationSharp : HL.MVVM.ViewModelBase
    {

        public RDStationSharp()
        {
            OptOutDataBase.Default.EnsureLoad();
        }

        private RDs.RdStationClient _Client;

        public HL.MVVM.Async.AsyncRelayCommand SyncCommand =>
           GetCommand(new HL.MVVM.Async.AsyncRelayCommand(
               new Action<object>((
                   object e) =>
               {
                   Status.Status.Current.ReportProgress("INICIANDO MODO DE SINCRONIZAÇÃO ERP <> RD STATION!");
                   FollowUp();



                   Status.Status.Current.ReportProgress("Autenticando Facebook...");

                   FBCustomAudienceManagerSync fbManager = new FBCustomAudienceManagerSync("", "", "");
                   fbManager.AccountId = "";
                   try
                   {
                       fbManager.Auth();
                   }
                   catch (Exception)
                   {
                       Status.Status.Current.ReportProgress("Não foi possível se conectar com o Facebook;", HL.MVVM.CSVLog.MessageType.Error);
                   }


                   Status.Status.Current.ReportProgress("Autenticando RD Station...");
                   _Client = new RDs.RdStationClient("", "", @"");
                   try
                   {
                       _Client.Authenticate();
                   }
                   catch (Exception)
                   {

                   }

                   if (!_Client.IsAuthenticated)
                   {
                       Status.Status.Current.ReportProgress("Não foi possível se conectar com o RD Station!", HL.MVVM.CSVLog.MessageType.Error);
                       ShutdownApplication(true);
                       return;
                   }

                   Status.Status.Current.ReportProgress("Carregando base de dados...");


                   Status.Status.Current.ReportProgress("Sincronizando público no FACEBOOK ADS");

                   if (fbManager.IsAuthenticated)
                   {
                       var success = fbManager.CreateOrUpdateCustomAudience(null, "", "");
                       if (success)
                       {
                           Status.Status.Current.ReportProgress("Público sincronizado com sucesso no Facebook ADS");
                       }
                       else
                       {
                           Status.Status.Current.ReportProgress("Falha na sincronização com o Facebook ADS", HL.MVVM.CSVLog.MessageType.Error);
                       }
                   }

                   Status.Status.Current.ReportProgress("Sincronizando Leads.");
                   Status.Status.Current.ReportProgress("Iniciando sincronização de Leads prioritários!");
                   Status.Status.Current.ReportProgress("Sincronização de Leads prioritários concluída!");
                   Status.Status.Current.ReportProgress("Iniciando sincronização de Leads secundários!");

                   Status.Status.Current.ReportProgress("Sincronização finalizada.");

                   Status.Status.Current.ReportProgress("Desligamento automático programado!");
                   Status.Status.Current.ReportProgress("Thread de sincronização finalizada.");

                   ShutdownApplication(true);
               }),

               (object o) =>
               {
                   return !IsBusy;
               }), "Sync");
        public HL.MVVM.Async.AsyncRelayCommand CleanUpCommand =>
           GetCommand(new HL.MVVM.Async.AsyncRelayCommand(
               new Action<object>((
                   object e) =>
               {

                   Status.Status.Current.ReportProgress("INICIANDO MODO LIMPEZA DA BASE DO RD STATION!");
                   Status.Status.Current.ReportProgress("INICIANDO MODO DE SINCRONIZAÇÃO DA RD COM O FACEBOOK ADS");

                   FollowUp(7, false);

                   Status.Status.Current.ReportProgress("Autenticando Facebook...");

                   FBCustomAudienceManagerSync fbManager = new FBCustomAudienceManagerSync("", "", "");
                   fbManager.AccountId = "act_892850625266026";
                   try
                   {
                       fbManager.Auth();
                   }
                   catch (Exception)
                   {
                       Status.Status.Current.ReportProgress("Não foi possível se conectar com o Facebook;", HL.MVVM.CSVLog.MessageType.Error);
                   }


                   Status.Status.Current.ReportProgress("Autenticando RD Station...");
                   _Client = new RDs.RdStationClient("", "", @"");
                   try
                   {
                       _Client.Authenticate();
                   }
                   catch (Exception)
                   {

                   }

                   if (!_Client.IsAuthenticated)
                   {
                       Status.Status.Current.ReportProgress("Não foi possível se conectar com o RD Station!", HL.MVVM.CSVLog.MessageType.Error);
                       ShutdownApplication();
                       return;
                   }

                   try
                   {
                       Status.Status.Current.ReportProgress("Iniciando Limpeza de Base!");
                       RDCloud.Default.Initialize(_Client);

                       if (fbManager.IsAuthenticated)
                       {
                           var success = fbManager.CreateOrUpdateCustomAudience(RDCloud.Default.ExportFacebookPublic(true, false, false), "Leads RD Station", "Base de leads da RD Station, atualizada diariamente.");
                           if (success)
                           {
                               Status.Status.Current.ReportProgress("Público de Leads sincronizado com sucesso no Facebook ADS");
                           }
                           else
                           {
                               Status.Status.Current.ReportProgress("Falha na sincronização do público de Leads com o Facebook ADS", HL.MVVM.CSVLog.MessageType.Error);
                           }
                       }

                       if (fbManager.IsAuthenticated)
                       {
                           var success = fbManager.CreateOrUpdateCustomAudience(RDCloud.Default.ExportFacebookPublic(false, true, false), "Prospects RD Station", "Base de Prospects da RD Station, atualizada diariamente.");
                           if (success)
                           {
                               Status.Status.Current.ReportProgress("Público de Prospects sincronizado com sucesso no Facebook ADS");
                           }
                           else
                           {
                               Status.Status.Current.ReportProgress("Falha na sincronização do público de Prospects com o Facebook ADS", HL.MVVM.CSVLog.MessageType.Error);
                           }
                       }

                       Status.Status.Current.ReportProgress("Processando Limpeza de Leads OptOut");
                       RDCloud.Default.ClearOptOut();

                       Status.Status.Current.ReportProgress("Processando Limpeza de Leads Desengajados da RD Station");
                       RDCloud.Default.ClearDisengaged();

                       Status.Status.Current.ReportProgress("LIMPEZA DE BASE CONCLUÍDA!");
                   }
                   catch (Exception ex)
                   {
                       Status.Status.Current.ReportProgress("O processo de limpeza da base falhou!", HL.MVVM.CSVLog.MessageType.Fail);
                   }

                   Status.Status.Current.ReportProgress("Thread de sincronização finalizada.");
                   Status.Status.Current.ReportProgress("ENCERRANDO INTEGRADOR!");

                   ShutdownApplication();

               }),

               (object o) =>
               {
                   return !IsBusy;
               }), "CleanUp");

        public void Sync(bool CleanUpMode = false)
        {
            if (SyncCommand.CanExecute(null)) SyncCommand.Execute(CleanUpMode);
        }

        public void CleanUpAndSyncFB()
        {
            if (CleanUpCommand.CanExecute(null)) CleanUpCommand.Execute(null);
        }

        public bool IsBusy
        {
            get
            {
                return SyncCommand.IsRunning || CleanUpCommand.IsRunning;
            }
        }

        private Task FollowUp(int hours = 8, bool ShutdownPC = true)
        {
            //Task responsável por desligar o PC após o tempo especificado em caso do integrador falhar miseravelmente.
            return Task.Run(async () =>
              {
                  Status.Status.Current.ReportProgress("TASK DE ACOMPANHAMENTO INICIADA COM SUCESSO!");
                  await Task.Delay(new TimeSpan(hours, 0, 0));
                  Status.Status.Current.ReportProgress($"A EXECUÇÃO DO INTEGRADOR EXCEDEU {hours} HORAS DE TRABALHO!");
                  Status.Status.Current.ReportProgress("Dado o longo tempo de execução, houve alguma falha.");

                  Status.Status.Current.ReportProgress("Programando desligamento em 1 minuto!");

                  ShutdownApplication(ShutdownPC);
              });
        }

        private void SyncLeads(List<LeadConversionData> leads)
        {
            //sincroniza o cadastro na RD Station e gera conversão se necessário.
            Status.Status.Current.ReportProgress(0, leads.Count);
            for (int i = 0; i < leads.Count; i++)
            {
                try
                {
                    //500
                    System.Threading.Thread.Sleep(10);
                    Status.Status.Current.ReportProgress(i + 1);
                    Status.Status.Current.ReportProgress(string.Format("Sincronizando Lead {0} de {1}", i + 1, leads.Count));

                    if (SyncLead(leads[i]))
                    {
                        Status.Status.Current.ReportProgress(string.Format("Lead {0} sincronizado com sucesso!", leads[i].Payload.Email));
                    }
                    else
                    {
                        Status.Status.Current.ReportProgress(string.Format("Lead {0} falhou no envio", leads[i].Payload.Email), HL.MVVM.CSVLog.MessageType.Fail);
                    }
                    //300
                    System.Threading.Thread.Sleep(10);
                }
                catch (Exception thisE)
                {
                    Status.Status.Current.ReportProgress(thisE.ToString());
                }

            }

            //sincroniza todos os estágios de funil
            Status.Status.Current.ReportProgress(0, leads.Count);
            for (int i = 0; i < leads.Count; i++)
            {
                try
                {
                    //500
                    System.Threading.Thread.Sleep(10);
                    Status.Status.Current.ReportProgress(i + 1);
                    Status.Status.Current.ReportProgress(string.Format("Atualizando estágio de funil do cadastro {0} de {1}", i + 1, leads.Count));

                    if (UpdateLeadFunnel(leads[i]))
                    {
                        Status.Status.Current.ReportProgress(string.Format("Lead {0} atualizado com sucesso!", leads[i].Payload.Email));
                    }
                    else
                    {
                        Status.Status.Current.ReportProgress(string.Format("Lead {0} falhou no envio...", leads[i].Payload.Email), HL.MVVM.CSVLog.MessageType.Fail);
                    }
                }
                catch (Exception thisE)
                {
                    Status.Status.Current.ReportProgress(thisE.ToString(), HL.MVVM.CSVLog.MessageType.Error);
                }

            }

            //sincroniza todas as vendas
            Status.Status.Current.ReportProgress(0, leads.Count);
            for (int i = 0; i < leads.Count; i++)
            {
                try
                {
                    Status.Status.Current.ReportProgress(i + 1);
                    Status.Status.Current.ReportProgress(string.Format("Atualizando VENDA {0} de {1}", i + 1, leads.Count));

                    if (CheckMarkSale(leads[i]))
                    {
                        Status.Status.Current.ReportProgress(string.Format("Lead {0} atualizado venda com sucesso!", leads[i].Payload.Email));
                    }
                    else
                    {
                        Status.Status.Current.ReportProgress(string.Format("Lead {0} falhou na atualização da venda...", leads[i].Payload.Email), HL.MVVM.CSVLog.MessageType.Fail);
                    }

                }
                catch (Exception thisE)
                {
                    Status.Status.Current.ReportProgress(thisE.ToString(), HL.MVVM.CSVLog.MessageType.Error);
                }
            }
        }

        private string GetRowField(DataRow r, string field)
        {
            string str = null;
            try
            {
                if (r.Table.Columns.Contains(field))
                    str = r[field].ToString().Trim();
            }
            catch { }
            if (string.IsNullOrWhiteSpace(str)) return null;
            return str;
        }

        private bool SyncLead(LeadConversionData lead)
        {
            Status.Status.Current.ReportProgress(">>> Getting data from RD Station: " + lead.Payload.Email);
            var leadRD = _Client.GetLeadSync(lead.Payload.Email);

            if (leadRD is null)
                Status.Status.Current.ReportProgress(">>> No data found for Lead " + lead.Payload.Email);
            else
            {
                //processamento de lógicas de ERP vs. RD Station
                Status.Status.Current.ReportProgress(">>> Processing logics between Local and Cloud for " + lead.Payload.Email);
            }

            if (string.IsNullOrEmpty(leadRD?.Email))
            {
                Status.Status.Current.ReportProgress(">>> Generating conversion for Lead " + lead.Payload.Email);

                DateTime dataPrimeiraCompra = DateTime.MinValue;
                DateTime.TryParse(lead.GetProperty<string>("DTPRIMEIRACOMPRA"), out dataPrimeiraCompra);
                if (dataPrimeiraCompra.Date == DateTime.Today)
                {
                   return GerarConversao(lead);
                }
            }
            else
            {
                if (((leadRD.CfStatusComercial == "Prospect") || string.IsNullOrWhiteSpace(leadRD.CfStatusComercial)) && (lead.Payload.CfStatusComercial == "Cliente Ativo"))
                {
                    Status.Status.Current.ReportProgress(">>> It's a Lead on RD Station. Generating conversion " + lead.Payload.Email);
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
            return false;
        }

        private bool GerarConversao(LeadConversionData lead)
        {
            return _Client.GenerateConversionSync(lead);
        }

        private bool UpdateLead(LeadConversionData lead, string uuid)
        {
            Status.Status.Current.ReportProgress(">>> Updating Lead on RD Station " + lead.Payload.Email);
            var r = _Client.UpdateLeadSync(uuid, LeadUpdateData.FromLead(lead));
            if (r)
                Status.Status.Current.ReportProgress(">>> Lead update success: " + lead.Payload.Email);
            else
            {
                Status.Status.Current.ReportProgress(">>> Lead update FAILED: " + lead.Payload.Email);
            }

            return r;
        }

        private bool UpdateLeadFunnel(LeadConversionData lead)
        {
            var _currentStage = _Client.GetLeadFunnel(lead.Payload.Email);
            if (lead.Payload.CfQtdCompras != 0 && !(lead.Payload.CfQtdCompras is null))
            {
                if (_currentStage == null || (_currentStage?.LifecycleStage != RDs.contacts.LeadStageData.Client))
                {
                    Status.Status.Current.ReportProgress(">>> Updating Lead Funnel Stage: " + lead.Payload.Email);
                    var r = _Client.ChangeLeadStageSync(lead.Payload.Email, new RDs.contacts.LeadStageData() { LifecycleStage = RDs.contacts.LeadStageData.Client });
                    if (r)
                        Status.Status.Current.ReportProgress(">>> Lead Stage update success: " + lead.Payload.Email);
                    else
                    {
                        Status.Status.Current.ReportProgress(">>> Lead Stage update FAILED: " + lead.Payload.Email, HL.MVVM.CSVLog.MessageType.Fail);
                    }
                    return r;
                }
            }
            else
            {
                if (_currentStage == null || (_currentStage?.LifecycleStage == RDs.contacts.LeadStageData.Lead))
                {
                    var r = _Client.ChangeLeadStageSync(lead.Payload.Email, new RDs.contacts.LeadStageData() { LifecycleStage = RDs.contacts.LeadStageData.QualifiedLead, Opportunity = true });
                    if (r)
                        Status.Status.Current.ReportProgress(">>> Lead Stage update success: " + lead.Payload.Email);
                    else
                    {
                        Status.Status.Current.ReportProgress(">>> Lead Stage update FAILED: " + lead.Payload.Email, HL.MVVM.CSVLog.MessageType.Fail);
                    }

                    return r;
                }
            }
            return true;
        }

        private bool CheckMarkSale(LeadConversionData lead)
        {
            try
            {
                if (lead.Payload.CfDiasSemCompra == 0)
                {
                    RDLog.Default.LogSale(lead.Payload.Email);
                    Status.Status.Current.ReportProgress("Marking sale on: " + lead.Payload.Email);
                    //Pausa para não sobrecarregar a API REST

                    System.Threading.Thread.Sleep(1000);
                    var r = _Client.WonOpportunity(new RDs.events.Won_Oportunity() { Payload = new RDs.events.Won_Oportunity_Payload() { Email = lead.Payload.Email } });
                    if (r)
                        Status.Status.Current.ReportProgress(">>> Lead sale mark success: " + lead.Payload.Email);
                    else
                    {
                        Status.Status.Current.ReportProgress(">>> Lead sale mark FAILED: " + lead.Payload.Email, HL.MVVM.CSVLog.MessageType.Fail);
                        RDLog.Default.LogSaleFail(lead.Payload.Email);
                    }
                    return r;
                }
                return true;
            }
            catch (Exception)
            {
                if (lead?.Payload?.CfDiasSemCompra == 0)
                    RDLog.Default.LogSaleFail(lead.Payload.Email);
                return false;
            }

        }
        /// <summary>
        /// Utilizado para programar o desligamento do integrador ou da máquina que está executando o integrador.
        /// </summary>
        /// <param name="ShutdownPC">Indica se deve encerrar apenas o aplicativo ou se deve encerrar o PC. Por padrão, irá fechar apenas o aplicativo.</param>
        private void ShutdownApplication(bool ShutdownPC = false)
        {
            DisposeAPI();
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(60000);
                bool doShutdown = true;
                if (ShutdownPC)
                {

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

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (System.Windows.MessageBox.Show("O computador será desligado!", "Desligamento em 1 Minuto!", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Exclamation) == System.Windows.MessageBoxResult.Cancel)
                            doShutdown = false;
                    });

                }
                else
                {

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        System.Windows.Application.Current.Shutdown();
                                    });

                }
            });

        }
        /// <summary>
        /// Indica se os membros estáticos já foram finalizados
        /// </summary>
        private static bool disposed = false;
        /// <summary>
        /// Indica se os membros estáticos estão em processo de encerramento
        /// </summary>
        private static bool disposing = false;
        /// <summary>
        /// Efetua o encerramento de todos os membros estáticos da API, tais como núcleos de LOG, bancos de dados e etc.
        /// </summary>
        public static void DisposeAPI()
        {
            try
            {
                if (disposed) return;
                if (disposing) return;
                disposing = true;
                Status.Status.Current.WaitReportAll().Wait(new TimeSpan(0, 5, 0));
                RdStationSharp.Data.OptOutDataBase.Default.Dispose();
                RdStationSharp.Data.DisengagedDataBase.RDStation.Dispose();
                RdStationSharp.Data.DisengagedDataBase.SendinBlue.Dispose();
                RDUserLog.Default.Dispose();
                RDLog.Default.Dispose();
                disposing = false;
                disposed = true;
            }
            catch { disposing = false; }
        }

        protected override void DoDispose()
        {
            DisposeAPI();
            base.DoDispose();
        }

    }
}