using System.Threading.Tasks;

namespace RdStationSharp
{
    public interface IRdStationApiClient
    {
        Task<bool> SendLead(ILead lead);
        bool SendLeadSync(ILead lead);
        bool ChangeLeadStatusSync(string email, LeadStatusRoot leadStatusRoot);
        Task<bool> ChangeLeadStatus(string email, LeadStatusRoot leadStatusRoot);
    }
}