using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Extensions
{
    public static class ObjectExtensions
    {
        static public void SynchronizationProperties(this object des, object src)
        {
            if (des == null || src == null)
            {
                return;
            }
            Type srcType = src.GetType();
            Type desType = des.GetType();
            object val;
            foreach (var item in srcType.GetProperties().Where(s => desType.GetProperties().Select(s => s.Name).Contains(s.Name)))
            {
                val = item.GetValue(src);
                item.SetValue(des, val);
            }
        }

        public static bool DeepEquals<T>(this T obj1, T obj2)
        {
            return CompareProperties(obj1, obj2, typeof(T));
        }

        /// <summary>
        /// 判断两个相同引用类型的对象的属性值是否相等
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj1">对象1</param>
        /// <param name="obj2">对象2</param>
        /// <param name="type">按type类型中的属性进行比较</param>
        /// <returns></returns>
        public static bool CompareProperties<T>(T obj1, T obj2, Type type)
        {
            //为空判断
            if (obj1 == null && obj2 == null)
                return true;
            else if (obj1 == null || obj2 == null)
                return false;

            Type t = type;
            PropertyInfo[] props = t.GetProperties();
            foreach (var po in props)
            {
                if (IsCanCompare(po.PropertyType))
                {
                    var a = po.GetValue(obj1);
                    var b = po.GetValue(obj2);
                    if (a == null || b == null)
                    {
                        if (a != null || b != null)
                        {
                            return false;
                        }
                    }
                    else if (!a.Equals(b))
                    {
                        return false;
                    }
                }
                else
                {
                    var b = CompareProperties(po.GetValue(obj1), po.GetValue(obj2), po.PropertyType);
                    if (!b) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 该类型是否可直接进行值的比较
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsCanCompare(Type t)
        {
            if (t.IsValueType)
            {
                return true;
            }
            else
            {
                //String是特殊的引用类型，它可以直接进行值的比较
                if (t.FullName == typeof(String).FullName)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取字段名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        private static string GetDisplayName(Type type, string property)
        {
            var pName = type.GetProperty(property);

            //4.0或以上版本
            var displayName = pName.GetCustomAttribute<DisplayAttribute>();

            return displayName?.Name;
        }


        /// <summary>
        /// Using a bit of reflection to build up the strings.
        /// </summary>
        public static string ToCsvHeader(this object obj)
        {
            Type type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.DeclaredOnly |
                                           BindingFlags.Public |
                                           BindingFlags.Instance);

            string result = string.Empty;
            Array.ForEach(properties, prop =>
            {
                result += "\"" + (GetDisplayName(type, prop.Name) ?? prop.Name) + "\"" + ",";
            });

            return (!string.IsNullOrEmpty(result) ? result.Substring(0, result.Length - 1) : result);
        }

        /// <summary>
        /// 转换csv
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToCsvRow(this object obj)
        {
            Type type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.DeclaredOnly |
                                           BindingFlags.Public |
                                           BindingFlags.Instance);

            string result = string.Empty;
            Array.ForEach(properties, prop =>
            {
                var value = prop.GetValue(obj, null);

                if (prop.PropertyType == typeof(string))
                {
                    value = "\"" + value + "\"";
                }
                else if (prop.PropertyType.IsEnum)
                {
                    value = "\"" + ((Enum)value).GetDisplayName() + "\"";
                }
                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    if (value != null)
                    {
                        if (prop.Name == "Birthday")
                        {
                            value = "\"" + ((DateTime)value).ToString("M") + "\"";
                        }
                        else
                        {
                            value = "\"" + ((DateTime)value).ToString("yyyy-MM-dd") + "\"";
                        }
                    }
                }

                result += value + ",";

            });

            return (!string.IsNullOrEmpty(result) ? result.Substring(0, result.Length - 1) : result);
        }
    }
}
