using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RdStationSharp
{
    public class RDLog : HL.MVVM.CSVLog
    {
        private string _Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "rdlog-" + DateTime.Today.ToShortDateString().Replace('/', '-') + "___" + DateTime.Now.ToLongTimeString().Replace(':', '-') + ".csv");

        public RDLog()
        {
            Init(_Path);
        }

        private static RDLog _Default = new RDLog();
        public static RDLog Default
        {
            get
            {
                return _Default;
            }
        }

        private int _sales = 0;
        private int _fails = 0;

        internal void LogSale(string mail)
        {
            lock (this)
            {
                _sales += 1;
                Log($"Registro de Venda para o e-mail: {mail}");
            }
        }

        internal void LogSaleFail(string mail)
        {
            lock (this)
            {
                _fails += 1;
                Log($"Falha no registro de venda do e-mail: {mail}");
            }
        }

        protected override void DoDispose()
        {
            WriteRow(DateTime.Now.ToShortTimeString(), "TOTAL DE VENDAS DO DIA", _sales.ToString());
            WriteRow(DateTime.Now.ToShortTimeString(), "TOTAL DE FALHAS NO REGISTRO DE VENDAS", _fails.ToString());
            base.DoDispose();
        }

    }
}