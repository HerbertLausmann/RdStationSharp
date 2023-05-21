using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rdstation_sync_runner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static readonly string pathIntegrador = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integrador ERP-RD STATION.exe");
        static RegistryKey onStartup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            SetOnStartup(true);

            var startTime = SyncTime;

            //Task responsável por startar o Integrador à noite para sincronizar a base do RD Station
            Task.Run(() =>
            {
                while (true)
                {
                    if (DateTime.Now.Hour == startTime.Hours)
                        if (DateTime.Now.Minute == startTime.Minutes)
                            break;
                    System.Threading.Thread.Sleep(1000);
                }
                System.Diagnostics.Process.Start(pathIntegrador);
            });

            //Task responsável por Re-autenticar o integrador com o Facebook e o RD Station e realizar a limpeza de base.
            Task.Run(() =>
            {
                while (true)
                {
                    if (DateTime.Now.Hour == 10)
                        if (DateTime.Now.Minute == 45)
                            break;
                    System.Threading.Thread.Sleep(1000);
                }
                System.Diagnostics.Process.Start(pathIntegrador, "cleanup");
            });
        }
        public static void SetOnStartup(bool run)
        {
            if (System.Reflection.Assembly.GetExecutingAssembly().Location.ToLower().Contains("debug"))
                return;
            if (run == true)
                onStartup.SetValue("integrador-erp", System.Reflection.Assembly.GetExecutingAssembly().Location);
            else
                onStartup.DeleteValue("integrador-erp", false);
        }

        /// <summary>
        /// Retorna o Horário em que o sincronizador será executadol, que é 18h50.
        /// No antepenúltimo e no penúltimo dia do mês, será executado às 21h.
        /// No último dia do mês, será executado apenas 23h50
        /// </summary>
        private TimeSpan SyncTime
        {
            get
            {
                DateTime date = DateTime.Today;
                // first generate all dates in the month of 'date'
                var dates = Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month)).Select(n => new DateTime(date.Year, date.Month, n));
                // then filter the only the start of weeks
                var weekdays = (from d in dates
                                where d.DayOfWeek != DayOfWeek.Sunday && d.DayOfWeek != DayOfWeek.Saturday
                                select d.Day).ToList();
                var dayIndex = weekdays.IndexOf(date.Day);
                if (dayIndex >= (weekdays.Count() - 4) && dayIndex < (weekdays.Count() - 1))
                {
                    return new TimeSpan(21, 0, 0);
                }
                else if (dayIndex == (weekdays.Count() - 1))
                {
                    return new TimeSpan(23, 50, 0);
                }
                else
                    return new TimeSpan(18, 50, 0);
            }
        }

    }
}
