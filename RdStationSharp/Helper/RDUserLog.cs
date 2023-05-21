using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.Helper
{
    class RDUserLog : HL.MVVM.CSVLog
    {
        private string _Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserLog", "userlog-" + DateTime.Today.ToShortDateString().Replace('/', '-') + "___" + DateTime.Now.ToLongTimeString().Replace(':', '-') + ".csv");
        private string _BadMailsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserLog", "badmails.txt");
        private MemoryStream _BadMails;

        public RDUserLog()
        {
            _BadMails = new MemoryStream();
            Init(_Path);
        }

        protected override void DoDispose()
        {
            SaveLocalBadMailsFile();
            base.Dispose();
        }

        private void WriteBadMailAddress(string mail, string cod = "N/A")
        {
            byte[] data = System.Text.UTF8Encoding.Default.GetBytes("Código de Cliente: " + cod + " | Endereço de E-mail: " + mail + Environment.NewLine);
            _BadMails.Write(data, 0, data.Length);
            _BadMails.Flush();
        }

        public void LogBadMail(string mail, string cod = "N/A")
        {
            WriteBadMailAddress(mail, cod);
            this.Fail("Bad mail: " + mail);
        }

        private void SaveLocalBadMailsFile()
        {
            FileStream fs = new FileStream(_BadMailsPath, FileMode.Create);
            _BadMails.Position = 0;
            _BadMails.WriteTo(fs);
            fs.Flush();

            fs.Close();
        }

        private static RDUserLog _Default = new RDUserLog();

        public static RDUserLog Default
        {
            get
            {
                return _Default;
            }
        }
    }
}
