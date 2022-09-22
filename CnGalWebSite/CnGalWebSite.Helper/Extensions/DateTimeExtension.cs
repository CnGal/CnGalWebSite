namespace CnGalWebSite.Helper.Extensions
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// 获得零时
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static DateTime GetTimeOfDayMin(this DateTime now)
        {
            return DateTime.ParseExact("2022-02-22 00:00:00", "yyyy-MM-dd HH:mm:ss", null);
        }

        /// <summary>
        /// 获得24时
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static DateTime GetTimeOfDayMax(this DateTime now)
        {
            return DateTime.ParseExact("2022-02-22 23:59:59", "yyyy-MM-dd HH:mm:ss", null);
        }

        /// <summary>
        /// 获得24时
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static string ToOmitString(this TimeSpan now)
        {
            if(now.TotalSeconds<1)
            {
                return $"{now.TotalMilliseconds:0.0} ms";
            }
            else if (now.TotalMinutes < 10)
            {
                return $"{now.TotalSeconds:0.0} s";
            }
            else if (now.TotalHours < 3)
            {
                return $"{now.TotalMinutes:0.0} min";
            }
            else if (now.TotalDays < 2)
            {
                return $"{now.TotalHours:0.0} h";
            }
            else 
            {
                return $"{now.Days:0.0} day";
            }
        }
    }
}
