namespace CTSearch.Library.Models
{
    public sealed record EligibilityAssessment
    {
        public EligibilityAssessmentStatus Status { get; init; } = EligibilityAssessmentStatus.NotAssessed;
        public string StatusLabel { get; init; } = "Not assessed";
        public IReadOnlyList<string> Reasons { get; init; } = [];
        public string? NctId { get; init; }
        public string? EligibilityCriteria { get; init; }

        public bool HasReasons => Reasons.Count > 0;
    }

    public enum EligibilityAssessmentStatus
    {
        NotAssessed,
        NeedsReview,
        PotentialMatch,
        PotentiallyExcluded,
        MissingNctId,
        Unavailable
    }
}
