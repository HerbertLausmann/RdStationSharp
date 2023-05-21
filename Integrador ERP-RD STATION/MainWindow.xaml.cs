using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RdStationSharp;

namespace Integrador_ERP_RD_STATION
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool mustclose = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!mustclose)
                if (MessageBox.Show("Você quer mesmo sair?", "ATENÇÃO", MessageBoxButton.YesNo) == MessageBoxResult.No) e.Cancel = true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Contains("cleanup"))
            {
                ((RDStationSharp)this.DataContext).CleanUpAndSyncFB();
                ((RDStationSharp)this.DataContext).CleanUpCommand.CanExecuteChanged += SyncCommand_CanExecuteChanged;
            }
            else
            {
                ((RDStationSharp)this.DataContext).Sync();
                ((RDStationSharp)this.DataContext).SyncCommand.CanExecuteChanged += SyncCommand_CanExecuteChanged;
            }

        }

        private void SyncCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            var rdsharp = ((RDStationSharp)this.DataContext);
            if (!rdsharp.IsBusy)
            {
                mustclose = true;
            }
            else
            {
                mustclose = false;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            RDStationSharp.DisposeAPI();
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        }
    }
}
