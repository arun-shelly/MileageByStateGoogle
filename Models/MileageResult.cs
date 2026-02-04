namespace MileageByStateGoogle.Models
{
    public class MileageResult
    {
        public List<OutputRecord> OutputRecords { get; set; }
        public List<OutputRecord> HighPayOnly { get; set; }
        public List<SummaryRecord> Summary { get; set; }
        

        public List<ApiCallStatsRecord> ApiCallStats { get; set; }

    }
}