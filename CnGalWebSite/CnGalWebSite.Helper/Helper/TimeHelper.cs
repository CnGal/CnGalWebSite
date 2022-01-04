using NodaTime;

namespace CnGalWebSite.DataModel.Helper
{
    public class TimeUtil
    {
        public static DateTime GetCstDateTime()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var shanghaiZone = DateTimeZoneProviders.Tzdb["Asia/Shanghai"];
            return now.InZone(shanghaiZone).ToDateTimeUnspecified();
        }

    }
    public static class DateTimeExtentions
    {
        public static DateTime ToCstTime(this DateTime time)
        {
            return TimeUtil.GetCstDateTime();
        }

        public static string ToTimeFromNowString(this DateTime time)
        {
            var timeSpan = DateTime.Now.ToCstTime() - time;

            if (timeSpan.TotalDays > 365)
            {
                return (int)(timeSpan.TotalDays / 365) + "年前";
            }
            else if (timeSpan.TotalDays > 30)
            {
                return (int)(timeSpan.TotalDays / 30) + "个月前";
            }
            else if (timeSpan.TotalHours > 24)
            {
                return (int)(timeSpan.TotalHours / 24) + "天前";
            }
            else if (timeSpan.TotalMinutes > 60)
            {
                return (int)(timeSpan.TotalMinutes / 60) + "小时前";
            }
            else if (timeSpan.TotalSeconds > 60)
            {
                return (int)(timeSpan.TotalSeconds / 60) + "分钟前";
            }
            else
            {
                return (int)(timeSpan.TotalSeconds) + "秒前";
            }
        }
    }
}
