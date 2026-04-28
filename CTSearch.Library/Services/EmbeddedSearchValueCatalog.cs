using System.Reflection;
using System.Text.Json;
using CTSearch.Library.Models;

namespace CTSearch.Library.Services
{
    public sealed class EmbeddedSearchValueCatalog : ISearchValueCatalog
    {
        private const string ResourcePrefix = "CTSearch.Library.RefDocs.Values.";
        private static readonly IReadOnlyList<SearchValueOption> AgeGroupOptions =
        [
            new("Adults", "Adults"),
            new("Children", "Children"),
            new("Both", "Both")
        ];

        private readonly Assembly _assembly = typeof(EmbeddedSearchValueCatalog).Assembly;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _phaseOptions;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _drugOptions;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _therapyOptions;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _diseaseSiteOptions;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _managementGroupOptions;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _oncologyGroupOptions;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _principalInvestigatorOptions;
        private readonly Lazy<IReadOnlyList<SearchValueOption>> _studySiteOptions;

        public EmbeddedSearchValueCatalog()
        {
            _phaseOptions = new(() => LoadCodeDescriptionOptions("Phase.txt", useDescriptionAsValue: true));
            _drugOptions = new(() => LoadCodeDescriptionOptions("drug.txt"));
            _therapyOptions = new(() => LoadCodeDescriptionOptions("therapy.txt"));
            _diseaseSiteOptions = new(() => LoadCodeDescriptionOptions("Disease sites.txt", useDescriptionWhenCodeMissing: true));
            _managementGroupOptions = new(() => LoadCodeDescriptionOptions("Management Group.txt"));
            _oncologyGroupOptions = new(() => LoadCodeDescriptionOptions("Oncology Group.txt"));
            _principalInvestigatorOptions = new(LoadPrincipalInvestigatorOptions);
            _studySiteOptions = new(() => LoadCodeDescriptionOptions("Study Stie.txt"));
        }

        public Task<IReadOnlyList<SearchValueOption>> GetAgeGroupOptionsAsync() => Task.FromResult(AgeGroupOptions);

        public Task<IReadOnlyList<SearchValueOption>> GetPhaseOptionsAsync() => Task.FromResult(_phaseOptions.Value);

        public Task<IReadOnlyList<SearchValueOption>> GetDrugOptionsAsync() => Task.FromResult(_drugOptions.Value);

        public Task<IReadOnlyList<SearchValueOption>> GetTherapyOptionsAsync() => Task.FromResult(_therapyOptions.Value);

        public Task<IReadOnlyList<SearchValueOption>> GetDiseaseSiteOptionsAsync() => Task.FromResult(_diseaseSiteOptions.Value);

        public Task<IReadOnlyList<SearchValueOption>> GetManagementGroupOptionsAsync() => Task.FromResult(_managementGroupOptions.Value);

        public Task<IReadOnlyList<SearchValueOption>> GetOncologyGroupOptionsAsync() => Task.FromResult(_oncologyGroupOptions.Value);

        public Task<IReadOnlyList<SearchValueOption>> GetPrincipalInvestigatorOptionsAsync() => Task.FromResult(_principalInvestigatorOptions.Value);

        public Task<IReadOnlyList<SearchValueOption>> GetStudySiteOptionsAsync() => Task.FromResult(_studySiteOptions.Value);

        private IReadOnlyList<SearchValueOption> LoadCodeDescriptionOptions(
            string fileName,
            bool useDescriptionWhenCodeMissing = false,
            bool useDescriptionAsValue = false)
        {
            using var document = LoadJsonDocument(fileName);
            var values = document.RootElement.GetProperty("values");
            var options = new List<SearchValueOption>(values.GetArrayLength());

            foreach (var item in values.EnumerateArray())
            {
                var label = item.GetProperty("itemDescription").GetString();
                var code = item.TryGetProperty("itemCode", out var codeElement)
                    ? codeElement.GetString()
                    : null;

                var value = useDescriptionAsValue
                    ? label
                    : !string.IsNullOrWhiteSpace(code)
                        ? code
                        : useDescriptionWhenCodeMissing ? label : null;

                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(label))
                {
                    options.Add(new SearchValueOption(value, label));
                }
            }

            return options;
        }

        private IReadOnlyList<SearchValueOption> LoadPrincipalInvestigatorOptions()
        {
            using var document = LoadJsonDocument("PIs.txt");
            var values = document.RootElement.GetProperty("values");
            var options = new List<SearchValueOption>(values.GetArrayLength());

            foreach (var item in values.EnumerateArray())
            {
                var staffId = item.GetProperty("staffId").GetInt32().ToString();
                var firstName = item.GetProperty("firstName").GetString();
                var lastName = item.GetProperty("lastName").GetString();
                var homeOrganization = item.TryGetProperty("homeOrganization", out var organizationElement)
                    ? organizationElement.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                {
                    continue;
                }

                var label = string.IsNullOrWhiteSpace(homeOrganization)
                    ? $"{lastName}, {firstName}"
                    : $"{lastName}, {firstName} ({homeOrganization})";

                options.Add(new SearchValueOption(staffId, label));
            }

            return options;
        }

        private JsonDocument LoadJsonDocument(string fileName)
        {
            var resourceName = ResourcePrefix + fileName;
            using var stream = _assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded search values resource '{resourceName}' was not found.");

            return JsonDocument.Parse(stream);
        }
    }
}
