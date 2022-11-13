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


       
    }
}
