namespace MileageByStateGoogle
{
    public class AppSettings
    {
        public string GoogleApiKey { get; set; }

        public static AppSettings Load()
        {
            return new AppSettings
            {
                GoogleApiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
            };
        }
    }
}