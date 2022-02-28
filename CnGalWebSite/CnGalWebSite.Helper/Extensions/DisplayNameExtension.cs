using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.Extensions
{
    public static class DisplayNameExtension
    {
        /// <summary>
        /// 获得枚举的displayName
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum eum)
        {
            if (eum == null)
            {
                return string.Empty;
            }
            var type = eum.GetType();
            var field = type.GetField(eum.ToString());
            var obj = (DisplayAttribute)field.GetCustomAttribute(typeof(DisplayAttribute));
            return obj.Name ?? "";
        }
    }
}
