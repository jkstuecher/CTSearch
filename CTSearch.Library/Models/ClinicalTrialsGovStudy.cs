namespace CTSearch.Library.Models
{
    public sealed record ClinicalTrialsGovStudy
    {
        public string NctId { get; init; } = string.Empty;
        public string? BriefTitle { get; init; }
        public string? EligibilityCriteria { get; init; }
        public bool? HealthyVolunteers { get; init; }
        public string? Sex { get; init; }
        public string? MinimumAge { get; init; }
        public string? MaximumAge { get; init; }
        public IReadOnlyList<string> StandardAges { get; init; } = [];
    }
}
