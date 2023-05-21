using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Integrador_ERP_RD_STATION
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnLoadCompleted(NavigationEventArgs e)
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                RdStationSharp.RDLog.Default.Error(eventArgs.Exception.ToString());
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                RdStationSharp.RDLog.Default.Error(eventArgs.ExceptionObject.ToString());
            };
            base.OnLoadCompleted(e);
        }
    }
}
