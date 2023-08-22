using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CnGalWebSite.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// 获得枚举的displayName
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum eum)
        {
            try
            {
                if (eum == null)
                {
                    return string.Empty;
                }
                var type = eum.GetType();
                var field = type.GetField(eum.ToString());
                var obj = (DisplayAttribute)field.GetCustomAttribute(typeof(DisplayAttribute));
                return obj?.Name ?? "";
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// 将 字符串 转换为枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text"></param>
        /// <returns></returns>
        public static T ToEnumValue<T>(this string text) where T : Enum
        {
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                var temp = (T)item;
                if (text == temp.GetDisplayName()|| text == temp.ToString())
                {
                    return temp;
                }
            }
            return default;
        }
    }
}
