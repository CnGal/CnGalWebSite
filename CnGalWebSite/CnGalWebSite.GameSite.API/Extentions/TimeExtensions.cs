using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.GameSite.API.Extentions
{
    public static class TimeExtensions
    {
        /// <summary>
        /// 获取当前时间转换成UTC时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ToCstTime(this DateTime time)
        {
            return time.ToUniversalTime();
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
