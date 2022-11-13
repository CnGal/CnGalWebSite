using System;

namespace CnGalWebSite.PublicToolbox.Extentions
{
    public static class TimeExtensions
    {
        /// <summary>
        /// 历史遗留 为配合API端 不删除
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ToCstTime(this DateTime time)
        {
            return time;
        }

        /// <summary>
        /// 获取距离当前时间长度的字符串
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
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
