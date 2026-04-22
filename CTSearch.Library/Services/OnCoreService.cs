using CTSearch.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CTSearch.Library.Services
{
    public class OnCoreService : IOnCoreService
    {
        private readonly HttpClient _httpClient;

        public OnCoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Base address should be set during DI registration, 
            // e.g., "https://your-oncore-domain.edu/arp/api/"
        }

        public async Task<ProtocolSearchResult> SearchProtocolsAsync(
            string? keyword = null,
            string? phase = null,
            string? drug = null,
            string? therapy = null,
            string? diseaseSite = null,
            string? managementGroup = null,
            string? oncologyGroup = null,
            string? principalInvestigatorId = null,
            string? studySite = null)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(keyword)) query["keyword"] = keyword;
            if (!string.IsNullOrWhiteSpace(phase)) query["phase"] = phase;
            if (!string.IsNullOrWhiteSpace(drug)) query["drug"] = drug;
            if (!string.IsNullOrWhiteSpace(therapy)) query["therapy"] = therapy;
            if (!string.IsNullOrWhiteSpace(diseaseSite)) query["diseaseSite"] = diseaseSite;
            if (!string.IsNullOrWhiteSpace(managementGroup)) query["managementGroup"] = managementGroup;
            if (!string.IsNullOrWhiteSpace(oncologyGroup)) query["oncologyGroup"] = oncologyGroup;
            if (!string.IsNullOrWhiteSpace(principalInvestigatorId)) query["principalInvestigatorId"] = principalInvestigatorId;
            if (!string.IsNullOrWhiteSpace(studySite)) query["studySite"] = studySite;

            string url = $"protocol/search?{query}";

            var result = await _httpClient.GetFromJsonAsync<ProtocolSearchResult>(url);
            return result ?? new ProtocolSearchResult(0, new List<ProtocolItem>());
        }

        public async Task<ProtocolDetails?> GetProtocolDetailsAsync(string protocolNo)
        {
            if (string.IsNullOrWhiteSpace(protocolNo)) return null;

            string url = $"protocol?protocolNo={HttpUtility.UrlEncode(protocolNo)}";
            return await _httpClient.GetFromJsonAsync<ProtocolDetails>(url);
        }

        public async Task<List<string>> GetDiseaseSitesAsync()
        {
            // The ARP API returns a simple array of strings for browse endpoints
            var sites = await _httpClient.GetFromJsonAsync<List<string>>("browse/diseaseSite");
            return sites ?? new List<string>();
        }
    }
}
