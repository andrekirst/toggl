using System.Web;

namespace Libraries.Toggl;

public static class DateTimeExtensions
{
    public static string ToIso8601(this DateTime date)
    {
        var x = $"{date:s}+02:00";
        return HttpUtility.UrlEncode(x).ToUpper();
    }

    public static DateTime FirstDayOfMonth(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, 1);

    public static DateTime Yesterday(this DateTime dateTime) => dateTime.AddDays(-1);
}