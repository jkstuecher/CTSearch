using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using CTSearch.Library.Services;    
using CTSearch.Library.Models;
using System.Security.Cryptography.X509Certificates;


namespace CTSearch.Cons
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IOnCoreService _onCoreService;

            var serviceProvider = new ServiceCollection()
           .AddLogging()
           .AddTransient<IOnCoreService, OnCoreService>()
           .BuildServiceProvider();

            

            //_onCoreService = serviceProvider.GetService<IOnCoreService>();

            // 1. Configuration
            string baseUrl = "https://osu-oncore-test.advarra.app/arp/api/";
            using HttpClient client = new HttpClient();

            Console.WriteLine("--- OnCore ARP Clinical Trial Search ---");
            Console.Write("Enter a search keyword (e.g., Cancer): ");
            string? keyword = Console.ReadLine();

            try
            {
                // 2. Execute Search
                // The ARP API uses GET /protocol/search?keyword=...
                string searchUrl = $"{baseUrl}/protocol/search?keyword={Uri.EscapeDataString(keyword ?? "")}";

                Console.WriteLine($"\nSearching at: {searchUrl}...");

                var searchResponse = await client.GetFromJsonAsync<ProtocolSearchResult>(searchUrl);

                if (searchResponse?.values == null || searchResponse.values.Count == 0)
                {
                    Console.WriteLine("No trials found matching that keyword.");
                    return;
                }

                Console.WriteLine($"Found {searchResponse.count} trials:\n");

                foreach (var trial in searchResponse.values)
                {
                    Console.WriteLine($"[{trial.protocolNo}] {trial.title}");
                    Console.WriteLine($"Status: {trial.status}\n");
                }

                // 3. Get Details for the first result
                string firstProtocolNo = searchResponse.values[0].protocolNo;
                Console.WriteLine($"--- Fetching Details for {firstProtocolNo} ---");

                string detailsUrl = $"{baseUrl}/protocol?protocolNo={firstProtocolNo}";
                var details = await client.GetFromJsonAsync<ProtocolDetails>(detailsUrl);

                if (details != null)
                {
                    Console.WriteLine($"Full Title: {details.title}");
                    Console.WriteLine($"Objective: {details.objective ?? "N/A"}");
                    Console.WriteLine($"NCT ID: {details.nctId ?? "N/A"}");
                    Console.WriteLine($"Target Accrual: {details.targetAccrual}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

// 4. Data Models (Based on ARP API Specification)
public record ProtocolSearchResult(int count, List<ProtocolItem> values);

        public record ProtocolItem(
            string protocolNo,
            string title,
            string status
        );

        public record ProtocolDetails(
            string protocolNo,
            string title,
            string? objective,
            string? nctId,
            int? targetAccrual,
            string? phase
        // Add more fields from the API Spec as needed (e.g., ageGroup, diseaseSite)
        );
        }
    }
