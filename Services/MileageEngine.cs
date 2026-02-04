using MileageByStateGoogle.Models;
using MileageByStateGoogle.Utils;
using Serilog;
using System.Diagnostics;

namespace MileageByStateGoogle.Services;



public class MileageEngine
{
    private readonly GoogleApiService _google;

    private readonly Dictionary<string, double> _stateRates = new()
    {
        { "CA", 0.70 },
        { "IL", 0.70 },
        { "MA", 0.70 }
    };

    public MileageEngine(GoogleApiService google)
    {
        _google = google;
    }

    public async Task<MileageResult> CalculateMileageByState(
        List<TravelItem> travelItems,
        List<TravelDetail> travelDetails)
    {
        var swFull = Stopwatch.StartNew();

        var output = new List<OutputRecord>();
        var apiStatsList = new List<ApiCallStatsRecord>();

        var groups = travelDetails.GroupBy(d => d.travel_id);

        foreach (var group in groups)
        {
            string travelId = group.Key;
            Log.Information("Processing travel_id: {TravelId}", travelId);

            var sw = Stopwatch.StartNew();

            int directionsCalls = 0;
            int geocodeCalls = 0;

            try
            {
                var travelItem = travelItems.First(t => t.travel_id == travelId);
                var allSegments = new List<StateMileage>();

                foreach (var leg in group)
                {
                    try
                    {
                        var counts = await ProcessLeg(allSegments, leg);
                        directionsCalls += counts.Directions;
                        geocodeCalls += counts.Geocodes;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Leg error for travel_id {TravelId}. Continuing.", travelId);
                    }
                }

                ApplyDeductionWithLogging(travelItem.deduct_miles, allSegments, travelId);

                // Aggregate by state
                var stateAggregates = allSegments
                    .GroupBy(s => s.State)
                    .Select(g => new
                    {
                        State = g.Key,
                        Miles = g.Sum(x => x.Miles),
                        Deducted = g.Sum(x => x.Deducted)
                    })
                    .ToList();

                // Build state output rows
                foreach (var st in stateAggregates)
                {
                    double rate = _stateRates.ContainsKey(st.State) ? 0.70 : 0.30;
                    double finalMiles = st.Miles - st.Deducted;

                    output.Add(new OutputRecord
                    {
                        travel_id = travelId,
                        travel_dt = travelItem.travel_dt,
                        State = st.State,
                        Rate = rate,
                        Miles = st.Miles,
                        Deducted = st.Deducted,
                        Final_Mile = finalMiles,
                        Reimbursement = finalMiles * rate
                    });

                    Log.Information(
                        "State summary travel_id={TravelId} State={State} Miles={Miles:F2} Deducted={Deduct:F2} Final={Final:F2}",
                        travelId, st.State, st.Miles, st.Deducted, finalMiles
                    );
                }

                sw.Stop();
                Log.Information(
                    "Completed travel_id {TravelId} in {Seconds:F2}s (Directions={Dir}, Geocode={Geo}, Total={Tot})",
                    travelId, sw.Elapsed.TotalSeconds, directionsCalls, geocodeCalls,
                    directionsCalls + geocodeCalls
                );

                apiStatsList.Add(new ApiCallStatsRecord
                {
                    travel_id = travelId,
                    DirectionsCalls = directionsCalls,
                    GeocodeCalls = geocodeCalls,
                    TotalApiCalls = directionsCalls + geocodeCalls
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "FATAL travel_id error {TravelId}. Skipping.", travelId);
            }
        }

        swFull.Stop();
        Log.Information("Full run time: {Seconds:F2}s", swFull.Elapsed.TotalSeconds);

        return new MileageResult
        {
            OutputRecords = output,
            ApiCallStats = apiStatsList
        };
    }

    // ==========================================================
    // PROCESS ONE LEG — RETURNS API CALL COUNTS
    // ==========================================================
    private async Task<(int Directions, int Geocodes)> ProcessLeg(
        List<StateMileage> result,
        TravelDetail leg)
    {
        int directionsCalls = 0;
        int geocodeCalls = 0;

        var dir = await _google.GetDirections(
            leg.Start_latitude, leg.Start_longitude,
            leg.End_latitude, leg.End_longitude);

        directionsCalls++;

        if (dir == null || dir.routes == null || dir.routes.Count == 0)
        {
            Log.Warning("Google returned NO ROUTE for travel_id {TravelId}", leg.travel_id);
            return (directionsCalls, geocodeCalls);
        }

        if (dir.status == "OVER_QUERY_LIMIT")
        {
            Log.Warning("Google RATE LIMIT HIT for travel_id {TravelId}", leg.travel_id);
        }

        var poly = dir.routes[0].overview_polyline.points;
        var points = PolylineDecoder.Decode(poly);

        if (points.Count < 2)
        {
            Log.Warning("Polyline too short for travel_id {TravelId}", leg.travel_id);
            return (directionsCalls, geocodeCalls);
        }

        // ------- HIGH RESOLUTION SAMPLING -------
        int sampleCount = Math.Max(10, points.Count / 3);
        int interval = Math.Max(1, points.Count / sampleCount);

        var sampledStates = new List<string>();

        for (int i = 0; i < points.Count; i += interval)
        {
            sampledStates.Add(await _google.GetState(points[i].lat, points[i].lon));
            geocodeCalls++;
        }

        sampledStates.Add(await _google.GetState(points[^1].lat, points[^1].lon));
        geocodeCalls++;

        Log.Information("Detected route states for travel_id {TravelId}: {States}",
            leg.travel_id, string.Join(" → ", sampledStates.Distinct()));

        // ------- Assign segments to states -------
        for (int i = 0; i < points.Count - 1; i++)
        {
            double miles = Haversine.Calculate(
                points[i].lat, points[i].lon,
                points[i + 1].lat, points[i + 1].lon);

            int idx = Math.Min(i / interval, sampledStates.Count - 1);
            string st = sampledStates[idx];

            result.Add(new StateMileage
            {
                State = st,
                Miles = miles
            });
        }

        return (directionsCalls, geocodeCalls);
    }

    // ==========================================================
    // DEDUCTION — WITH STEP LOGGING
    // ==========================================================
    private void ApplyDeductionWithLogging(double deduct, List<StateMileage> segments, string travelId)
    {
        Log.Information(
            "Applying {Deduct:F2} miles deduction for travel_id {TravelId}",
            deduct, travelId
        );

        var ordered = segments
            .OrderByDescending(s => _stateRates.ContainsKey(s.State))
            .ToList();

        foreach (var sm in ordered)
        {
            if (deduct <= 0)
            {
                Log.Information("Deduction done for travel_id {TravelId}", travelId);
                break;
            }

            double take = Math.Min(sm.Miles, deduct);

            Log.Information(
                "Deduction step travel_id={TravelId}: State={State}, Miles={Miles:F2}, Taking={Take:F2}, RemainingBefore={Deduct:F2}",
                travelId, sm.State, sm.Miles, take, deduct
            );

            sm.Deducted = take;
            deduct -= take;
        }
    }
}