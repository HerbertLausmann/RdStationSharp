using HL.MVVM;
using RdStationSharp.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Representa o namespace RdStationSharp.Status que contém a classe Status.
/// </summary>
namespace RdStationSharp.Status
{
    /// <summary>
    /// Representa um mecanismo de atualização de status para relatar progresso e mensagens de maneira segura em relação a threads, para que seja possível acompanhar o processo de comunicação e sincronização em diferentes níveis.
    /// Esta classe segue o padrão MVVM e possui uma propriedade estática que pode ser referenciada em uma UI XAML, por exemplo, para apresentar as informações atualizadas de forma automática, através de Data Binding.
    /// </summary>
    public class Status : ModelBase
    {
        private static Status _Current = new Status();

        /// <summary>
        /// Obtém a instância atual da classe Status, na qual é reportado o progresso e status das chamadas da API RDStationSharp.
        /// </summary>
        public static Status Current
        {
            get
            {
                return _Current;
            }
        }

        /// <summary>
        /// Obtém a mensagem atual.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Obtém o progresso atual.
        /// </summary>
        public double Progress { get; private set; }

        /// <summary>
        /// Obtém o progresso total.
        /// </summary>
        public double TotalProgress { get; private set; }

        private Queue<Action> _actionQueue = new Queue<Action>();

        /// <summary>
        /// Reporta o progresso atual.
        /// </summary>
        /// <param name="progress">Valor do progresso atual.</param>
        internal void ReportProgress(double progress)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(() =>
                {
                    Progress = progress;
                    OnPropertyChanged(nameof(Progress));
                });
            }
            _ = ProcessQueue();
        }

        /// <summary>
        /// Reporta o progresso e o progresso total.
        /// </summary>
        /// <param name="progress">Valor do progresso atual.</param>
        /// <param name="totalProgress">Valor do progresso total.</param>
        internal void ReportProgress(double progress, double totalProgress)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(() =>
                {
                    Progress = progress;
                    TotalProgress = totalProgress;
                    OnPropertyChanged(nameof(Progress));
                    OnPropertyChanged(nameof(TotalProgress));
                });
            }
            _ = ProcessQueue();
        }

        /// <summary>
        /// Reporta a mensagem, o progresso e o progresso total.
        /// </summary>
        /// <param name="message">Mensagem a ser relatada.</param>
        /// <param name="progress">Valor do progresso atual.</param>
        /// <param name="totalProgress">Valor do progresso total.</param>
        /// <param name="MessageType">Tipo da mensagem.</param>
        internal void ReportProgress(string message, double progress, double totalProgress, CSVLog.MessageType MessageType = CSVLog.MessageType.Message)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(() =>
                {
                    Message = message;
                    Progress = progress;
                    TotalProgress = totalProgress;
                    OnPropertyChanged(nameof(Progress));
                    OnPropertyChanged(nameof(TotalProgress));
                    OnPropertyChanged(nameof(Message));
                    RDUserLog.Default.Log(message, MessageType);
                });
            }
            _ = ProcessQueue();
        }

        /// <summary>
        /// Reporta a mensagem.
        /// </summary>
        /// <param name="message">Mensagem a ser relatada.</param>
        /// <param name="MessageType">Tipo da mensagem.</param>
        internal void ReportProgress(string message, CSVLog.MessageType MessageType = CSVLog.MessageType.Message)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(() =>
                {
                    Message = message;
                    OnPropertyChanged(nameof(Message));
                    RDUserLog.Default.Log(message, MessageType);
                });
            }
            _ = ProcessQueue();
        }

        /// <summary>
        /// Processa a fila de ações, executando-as na ordem em que foram adicionadas.
        /// </summary>
        /// <returns>Uma tarefa que representa a operação assíncrona.</returns>
        private async Task ProcessQueue()
        {
            IsReporting = true;
            while (_actionQueue.Count > 0)
            {
                Action action;
                lock (_actionQueue)
                {
                    action = _actionQueue.Dequeue();
                }
                await Task.Run(action);
            }
            IsReporting = false;
        }

        private bool _IsReporting;
        public bool IsReporting
        {
            get { return _IsReporting; }
            private set { SetField(ref _IsReporting, value, "IsReporting"); }
        }

        /// <summary>
        /// Aguarda todas as notificações de status serem processadas, de forma assíncrona.
        /// </summary>
        /// <returns>Retorna uma tarefa aguardável da ação que está sendo executada.</returns>
        public Task WaitReportAll()
        {
            return Task.Run(() =>
            {
                while (IsReporting) { Thread.Sleep(10); }
                return;
            });
        }

    }
}
