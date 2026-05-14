using System.Text.Json;
using System.Text.RegularExpressions;
using CTSearch.Library.Models;

namespace CTSearch.Library.Services
{
    public sealed class ClinicalTrialsGovService : IClinicalTrialsGovService
    {
        private const string StudyFields = "NCTId,BriefTitle,EligibilityCriteria,HealthyVolunteers,Sex,MinimumAge,MaximumAge,StdAge";

        private static readonly Regex AgeRegex = new(
            @"(?<value>\d+(?:\.\d+)?)\s*(?<unit>year|month|week|day)s?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly IReadOnlyList<ClinicalFactorOption> FactorOptions =
        [
            new("prior-chemotherapy", "Prior chemotherapy", ["prior chemotherapy", "previous chemotherapy", "prior systemic therapy", "previous systemic therapy", "chemotherapy"]),
            new("prior-radiation", "Prior radiation therapy", ["prior radiation", "previous radiation", "radiation therapy", "radiotherapy"]),
            new("brain-metastases", "Brain metastases / CNS disease", ["brain metastases", "brain metastasis", "cns metastases", "central nervous system metastases", "leptomeningeal"]),
            new("pregnancy", "Pregnancy or breastfeeding", ["pregnant", "pregnancy", "breastfeeding", "lactating", "nursing"]),
            new("active-infection", "Active infection", ["active infection", "uncontrolled infection", "systemic infection", "chronic infection"]),
            new("hiv-immunodeficiency", "HIV / immunodeficiency", ["hiv", "immunodeficiency", "immunocompromised", "aids"]),
            new("anticoagulation", "Anticoagulation / bleeding risk", ["anticoagulation", "anticoagulant", "bleeding disorder", "coagulopathy", "hemorrhage"]),
            new("renal-impairment", "Renal impairment", ["renal impairment", "kidney disease", "renal dysfunction", "creatinine clearance", "dialysis"]),
            new("hepatic-impairment", "Hepatic impairment", ["hepatic impairment", "liver disease", "hepatic dysfunction", "bilirubin", "ast", "alt"]),
            new("cardiac-disease", "Cardiac disease", ["cardiac disease", "heart disease", "myocardial infarction", "congestive heart failure", "arrhythmia"]),
            new("diabetes", "Diabetes", ["diabetes", "diabetic"]),
            new("autoimmune-disease", "Autoimmune disease", ["autoimmune disease", "autoimmune disorder", "lupus", "rheumatoid arthritis"]),
            new("organ-transplant", "Prior organ transplant", ["organ transplant", "transplant recipient", "allogeneic transplant", "stem cell transplant"])
        ];

        private readonly HttpClient _httpClient;

        public ClinicalTrialsGovService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IReadOnlyList<ClinicalFactorOption> ClinicalFactorOptions => FactorOptions;

        public async Task<ClinicalTrialsGovStudy?> GetStudyAsync(
            string nctId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nctId))
            {
                return null;
            }

            var url = $"studies/{Uri.EscapeDataString(nctId)}?fields={Uri.EscapeDataString(StudyFields)}&format=json";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("protocolSection", out var protocolSection))
            {
                return null;
            }

            protocolSection.TryGetProperty("identificationModule", out var identificationModule);
            protocolSection.TryGetProperty("eligibilityModule", out var eligibilityModule);

            return new ClinicalTrialsGovStudy
            {
                NctId = GetString(identificationModule, "nctId") ?? nctId,
                BriefTitle = GetString(identificationModule, "briefTitle"),
                EligibilityCriteria = GetString(eligibilityModule, "eligibilityCriteria"),
                HealthyVolunteers = GetBoolean(eligibilityModule, "healthyVolunteers"),
                Sex = GetString(eligibilityModule, "sex"),
                MinimumAge = GetString(eligibilityModule, "minimumAge"),
                MaximumAge = GetString(eligibilityModule, "maximumAge"),
                StandardAges = GetStringArray(eligibilityModule, "stdAges")
            };
        }

        public async Task<EligibilityAssessment> AssessStudyAsync(
            string? nctId,
            EligibilityRefinementCriteria criteria,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nctId))
            {
                return new EligibilityAssessment
                {
                    Status = EligibilityAssessmentStatus.MissingNctId,
                    StatusLabel = "Needs review",
                    Reasons = ["No NCT ID is available for ClinicalTrials.gov eligibility refinement."]
                };
            }

            var study = await GetStudyAsync(nctId, cancellationToken);
            if (study is null)
            {
                return new EligibilityAssessment
                {
                    NctId = nctId,
                    Status = EligibilityAssessmentStatus.Unavailable,
                    StatusLabel = "Needs review",
                    Reasons = [$"ClinicalTrials.gov data could not be loaded for {nctId}."]
                };
            }

            var reasons = new List<string>();
            AddStructuredReasons(study, criteria, reasons);
            AddClinicalFactorReasons(study, criteria, reasons);

            var status = reasons.Count > 0
                ? EligibilityAssessmentStatus.PotentiallyExcluded
                : EligibilityAssessmentStatus.PotentialMatch;

            return new EligibilityAssessment
            {
                NctId = study.NctId,
                EligibilityCriteria = study.EligibilityCriteria,
                Status = status,
                StatusLabel = status == EligibilityAssessmentStatus.PotentiallyExcluded
                    ? "Potential exclusion"
                    : "No structured conflicts",
                Reasons = reasons
            };
        }

        private static void AddStructuredReasons(
            ClinicalTrialsGovStudy study,
            EligibilityRefinementCriteria criteria,
            List<string> reasons)
        {
            if (criteria.AgeYears is int ageYears)
            {
                var minimumAgeYears = ParseAgeInYears(study.MinimumAge);
                var maximumAgeYears = ParseAgeInYears(study.MaximumAge);

                if (minimumAgeYears is double minimum && ageYears < minimum)
                {
                    reasons.Add($"Age is below the minimum age: patient is {ageYears}, trial minimum is {study.MinimumAge}.");
                }

                if (maximumAgeYears is double maximum && ageYears > maximum)
                {
                    reasons.Add($"Age is above the maximum age: patient is {ageYears}, trial maximum is {study.MaximumAge}.");
                }
            }

            if (!string.IsNullOrWhiteSpace(criteria.Sex) &&
                !string.IsNullOrWhiteSpace(study.Sex) &&
                !string.Equals(study.Sex, "ALL", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(study.Sex, criteria.Sex, StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add($"Sex does not match: patient is {FormatSex(criteria.Sex)}, trial is limited to {FormatSex(study.Sex)}.");
            }

            if (criteria.IsHealthyVolunteer == true && study.HealthyVolunteers == false)
            {
                reasons.Add("Trial does not accept healthy volunteers.");
            }
        }

        private static void AddClinicalFactorReasons(
            ClinicalTrialsGovStudy study,
            EligibilityRefinementCriteria criteria,
            List<string> reasons)
        {
            if (criteria.ClinicalFactorValues.Count == 0 || string.IsNullOrWhiteSpace(study.EligibilityCriteria))
            {
                return;
            }

            var exclusionText = ExtractExclusionCriteria(study.EligibilityCriteria);
            if (string.IsNullOrWhiteSpace(exclusionText))
            {
                exclusionText = study.EligibilityCriteria;
            }

            var selectedValues = criteria.ClinicalFactorValues.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var factor in FactorOptions.Where(option => selectedValues.Contains(option.Value)))
            {
                if (factor.MatchTerms.Any(term => ContainsTerm(exclusionText, term)))
                {
                    reasons.Add($"Matched exclusion: {factor.Label}.");
                }
            }
        }

        private static string? ExtractExclusionCriteria(string eligibilityCriteria)
        {
            var exclusionIndex = eligibilityCriteria.IndexOf("Exclusion Criteria", StringComparison.OrdinalIgnoreCase);
            if (exclusionIndex < 0)
            {
                return null;
            }

            return eligibilityCriteria[exclusionIndex..];
        }

        private static double? ParseAgeInYears(string? age)
        {
            if (string.IsNullOrWhiteSpace(age) || age.Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var match = AgeRegex.Match(age);
            if (!match.Success || !double.TryParse(match.Groups["value"].Value, out var value))
            {
                return null;
            }

            return match.Groups["unit"].Value.ToLowerInvariant() switch
            {
                "year" => value,
                "month" => value / 12,
                "week" => value / 52,
                "day" => value / 365,
                _ => null
            };
        }

        private static bool ContainsTerm(string text, string term)
        {
            return text.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatSex(string? sex)
        {
            return sex?.ToUpperInvariant() switch
            {
                "FEMALE" => "female",
                "MALE" => "male",
                "ALL" => "all sexes",
                _ => sex ?? "unknown"
            };
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object &&
                element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.String
                    ? property.GetString()
                    : null;
        }

        private static bool? GetBoolean(JsonElement element, string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object &&
                element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? property.GetBoolean()
                    : null;
        }

        private static IReadOnlyList<string> GetStringArray(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty(propertyName, out var property) ||
                property.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return property
                .EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .OfType<string>()
                .ToArray();
        }
    }
}
