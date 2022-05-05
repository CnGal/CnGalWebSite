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
    }
}
