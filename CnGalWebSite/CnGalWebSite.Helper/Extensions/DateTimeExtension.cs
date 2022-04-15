using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.DataModel.Helper;

namespace CnGalWebSite.Helper.Extensions
{
   static public  class DateTimeExtension
    {
        /// <summary>
        /// 获得零时
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static DateTime GetTimeOfDayMin(this DateTime now)
        {
            return DateTime.ParseExact("2022-2-22 00:00:00", "yyyy-MM-dd HH:mm:ss", null);
        }

        /// <summary>
        /// 获得24时
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static DateTime GetTimeOfDayMax(this DateTime now)
        {
            return DateTime.ParseExact("2022-2-22 23:59:59", "yyyy-MM-dd HH:mm:ss", null);
        }
    }
}
