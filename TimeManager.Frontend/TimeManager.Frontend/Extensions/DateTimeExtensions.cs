namespace TimeManager.Frontend.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo CompanyTimeZone = OperatingSystem.IsWindows()
    ? TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time")
    : TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

        public static string ToHumanReadable(this DateTimeOffset dateTimeOffset, string format = "dddd, MMMM d, yyyy 'at' h:mm tt")
        {
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(dateTimeOffset, CompanyTimeZone);

            return localTime.ToString(format);
        }
    }
}
