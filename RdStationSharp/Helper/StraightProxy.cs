using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.Helper
{
    internal class StraightProxy : IWebProxy
    {
        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination)
        {
            return destination;
        }

        public bool IsBypassed(Uri host)
        {
            // if return true, service will be very slow.
            return false;
        }

        private static StraightProxy defaultProxy = new StraightProxy();
        public static StraightProxy Default
        {
            get
            {
                return defaultProxy;
            }
        }
    }
}
