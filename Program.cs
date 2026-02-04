using MileageByStateGoogle.Services;
using MileageByStateGoogle.Models;
using Microsoft.Extensions.Configuration;
using Serilog;



internal class Program
{
    static readonly string ApiKey;

    static Program()
    {
        // LOAD CONFIG
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();

        ApiKey = config["GApiKey"]
            ?? throw new InvalidOperationException("Google API Key not found in secrets.");

        // SETUP SERILOG
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("Logs/mileage.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Program initialization completed.");
    }

    static async Task Main()
    {
        Log.Information("Mileage calculation started.");

        try
        {
            var google = new GoogleApiService(ApiKey);
            var csv = new CsvService();
            var engine = new MileageEngine(google);

            // LOAD INPUT CSVs
            var travelItems = csv.LoadCsv<TravelItem>("Data/Input/TravelItems.csv");
            var travelDetails = csv.LoadCsv<TravelDetail>("Data/Input/TravelItemDetails.csv");

            // RUN MILEAGE ENGINE
            var results = await engine.CalculateMileageByState(travelItems, travelDetails);

            // EXPORT MAIN OUTPUT
            csv.ExportCsv("Data/Output/AllStateMileageOutput.csv", results.OutputRecords);

            // ----------------------------------------------------------
            // HIGH-PAY TRAVEL ONLY (UNCHANGED)
            // ----------------------------------------------------------
            var highPayStates = new HashSet<string> { "CA", "IL", "MA" };

            var highOnly = results.OutputRecords
                .Where(r => highPayStates.Contains(r.State))
                .ToList();

            csv.ExportCsv("Data/Output/HighPayTravelOnly.csv", highOnly);

            // ----------------------------------------------------------
            // CORRECTED SUMMARY LOGIC
            //
            // *Include ALL states mileage for ANY travel_id that
            //  contains a HIGH-PAY state*
            //
            // This now matches your business requirement.
            // ----------------------------------------------------------

            var summary = results.OutputRecords
                .GroupBy(r => r.travel_id)
                .Where(g => g.Any(r => highPayStates.Contains(r.State))) // include full trip if ANY high-pay state exists
                .Select(g =>
                {
                    string travelId = g.Key;
                    var travelItemRows = travelItems.Where(t => t.travel_id == travelId).ToList();

                    return new SummaryRecord
                    {
                        travel_id = travelId,
                        travel_dt = travelItemRows.First().travel_dt,
                        travel_distance = travelItemRows.Sum(t => t.travel_distance),
                        actual_amount = travelItemRows.Sum(t => t.actual_amount),

                        // ✔ FIXED — includes IN + IL (ALL states)
                        MilesByState = g.Sum(r => r.Final_Mile),

                        // ✔ FIXED — includes reimbursement for ALL states
                        adjusted_amount = g.Sum(r => r.Reimbursement)
                    };
                })
                .ToList();

            csv.ExportCsv("Data/Output/TravelSummaryComparison.csv", summary);

            // API CALL STATISTICS
            csv.ExportApiCallStats("Data/Output/ApiCallStats.csv", results.ApiCallStats);

            Log.Information("Mileage calculation completed successfully.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "FATAL ERROR during mileage processing.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}