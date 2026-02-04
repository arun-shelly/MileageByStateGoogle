using CsvHelper;
using CsvHelper.Configuration;
using MileageByStateGoogle.Models;
using System.Globalization;

namespace MileageByStateGoogle.Services;



public class CsvService
{
    // ----------------------------------------------------------------------
    // GENERIC CSV LOADER
    // ----------------------------------------------------------------------
    public List<T> LoadCsv<T>(string path)
    {
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<T>().ToList();
    }

    // ----------------------------------------------------------------------
    // GENERIC CSV EXPORTER
    // ----------------------------------------------------------------------
    public void ExportCsv<T>(string path, List<T> rows)
    {
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // If you have custom maps, register here
        if (typeof(T) == typeof(OutputRecord))
            csv.Context.RegisterClassMap<OutputRecordMap>();

        if (typeof(T) == typeof(SummaryRecord))
            csv.Context.RegisterClassMap<SummaryRecordMap>();

        if (typeof(T) == typeof(ApiCallStatsRecord))
            csv.Context.RegisterClassMap<ApiCallStatsRecordMap>();

        csv.WriteRecords(rows);
    }

    // ----------------------------------------------------------------------
    // EXPORT API CALL STATISTICS + TOTAL ROW
    // ----------------------------------------------------------------------
    public void ExportApiCallStats(string path, List<ApiCallStatsRecord> stats)
    {
        int totalDirections = stats.Sum(s => s.DirectionsCalls);
        int totalGeocodes = stats.Sum(s => s.GeocodeCalls);
        int totalApi = stats.Sum(s => s.TotalApiCalls);

        // Create totals row
        var totalsRow = new ApiCallStatsRecord
        {
            travel_id = "TOTAL-ALL-TRIPS",
            DirectionsCalls = totalDirections,
            GeocodeCalls = totalGeocodes,
            TotalApiCalls = totalApi
        };

        // Append
        var fullList = stats.ToList();
        fullList.Add(totalsRow);

        ExportCsv(path, fullList);
    }
}


// ----------------------------------------------------------------------
// CSV ClassMaps (column ordering)
// ----------------------------------------------------------------------
public sealed class OutputRecordMap : ClassMap<OutputRecord>
{
    public OutputRecordMap()
    {
        Map(m => m.travel_id);
        Map(m => m.travel_dt);
        Map(m => m.State);
        Map(m => m.Rate);
        Map(m => m.Miles);
        Map(m => m.Deducted);
        Map(m => m.Final_Mile);
        Map(m => m.Reimbursement);
    }
}

public sealed class SummaryRecordMap : ClassMap<SummaryRecord>
{
    public SummaryRecordMap()
    {
        Map(m => m.travel_id);
        Map(m => m.travel_dt);
        Map(m => m.travel_distance);
        Map(m => m.actual_amount);
        Map(m => m.MilesByState);
        Map(m => m.adjusted_amount);
    }
}

public sealed class ApiCallStatsRecordMap : ClassMap<ApiCallStatsRecord>
{
    public ApiCallStatsRecordMap()
    {
        Map(m => m.travel_id);
        Map(m => m.DirectionsCalls);
        Map(m => m.GeocodeCalls);
        Map(m => m.TotalApiCalls);
    }
}