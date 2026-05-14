namespace CTSearch.Library.Models
{
    public sealed record EligibilityRefinementCriteria
    {
        public int? AgeYears { get; init; }
        public string? Sex { get; init; }
        public bool? IsHealthyVolunteer { get; init; }
        public IReadOnlyList<string> ClinicalFactorValues { get; init; } = [];
    }
}
