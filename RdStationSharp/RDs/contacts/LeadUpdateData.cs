using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdStationSharp.RDs.contacts
{

    public class LeadUpdateData : RD_DefaultSyncFields
    {
        public LeadUpdateData()
        {
            LegalBases = new List<LegalBas>();
            LegalBases.Add(new LegalBas() { Category = "communications", Type = "consent", Status = "granted" });
        }
        public static LeadUpdateData FromLead(LeadConversionData lead)
        {
            var l = lead.Payload.CopyTo<LeadUpdateData>();
            l.LegalBases = new List<LegalBas>();
            l.LegalBases.Add(new LegalBas() { Category = "communications", Type = "consent", Status = "granted", });
            return l;
        }
    }
}
