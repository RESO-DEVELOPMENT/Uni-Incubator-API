using NodaTime;

namespace Application.Domain
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Get current datetime in +0070 in any server
        /// </summary>
        /// <returns>DateTime in +0070</returns>
        public static DateTime Now()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var vietNamTz = DateTimeZoneProviders.Tzdb["Asia/Saigon"];
            var zonedDateTime = now.InZone(vietNamTz);

            return zonedDateTime.LocalDateTime.ToDateTimeUnspecified();
        }

        public static DateTime StartOfMonth(int month, int year)
        {
            var date = new DateTime(year, month, 1);
            return date;
        }

        public static DateTime EndOfMonth(int month, int year)
        {
            var date = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            return date;
        }
    }
}