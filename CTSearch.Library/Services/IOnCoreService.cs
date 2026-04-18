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
        Task<ProtocolSearchResult> SearchProtocolsAsync(string? keyword = null, string? ageGroup = null, string? phase = null);
        Task<ProtocolDetails?> GetProtocolDetailsAsync(string protocolNo);
        Task<List<string>> GetDiseaseSitesAsync();
    }
}
