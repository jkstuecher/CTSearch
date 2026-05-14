using CTSearch.Library.Models;

namespace CTSearch.Library.Services
{
    public interface IClinicalTrialsGovService
    {
        IReadOnlyList<ClinicalFactorOption> ClinicalFactorOptions { get; }

        Task<ClinicalTrialsGovStudy?> GetStudyAsync(string nctId, CancellationToken cancellationToken = default);

        Task<EligibilityAssessment> AssessStudyAsync(
            string? nctId,
            EligibilityRefinementCriteria criteria,
            CancellationToken cancellationToken = default);
    }
}
