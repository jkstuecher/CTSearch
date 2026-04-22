using CTSearch.Library.Models;

namespace CTSearch.Library.Services
{
    public interface ISearchValueCatalog
    {
        Task<IReadOnlyList<SearchValueOption>> GetPhaseOptionsAsync();
        Task<IReadOnlyList<SearchValueOption>> GetDrugOptionsAsync();
        Task<IReadOnlyList<SearchValueOption>> GetTherapyOptionsAsync();
        Task<IReadOnlyList<SearchValueOption>> GetDiseaseSiteOptionsAsync();
        Task<IReadOnlyList<SearchValueOption>> GetManagementGroupOptionsAsync();
        Task<IReadOnlyList<SearchValueOption>> GetOncologyGroupOptionsAsync();
        Task<IReadOnlyList<SearchValueOption>> GetPrincipalInvestigatorOptionsAsync();
        Task<IReadOnlyList<SearchValueOption>> GetStudySiteOptionsAsync();
    }
}
