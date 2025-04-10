namespace Bank.BL.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ToUtcKind(this DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}
