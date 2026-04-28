using CTSearch.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTSearch.Library.Services
{
    public interface IOnCoreService
    {
        Task<ProtocolSearchResult> SearchProtocolsAsync(
            string? keyword = null,
            string? ageGroup = null,
            string? phase = null,
            string? drug = null,
            string? therapy = null,
            string? diseaseSite = null,
            string? managementGroup = null,
            string? oncologyGroup = null,
            string? principalInvestigatorId = null,
            string? studySite = null);
        Task<ProtocolDetails?> GetProtocolDetailsAsync(string protocolNo);
        Task<List<string>> GetDiseaseSitesAsync();
    }
}
