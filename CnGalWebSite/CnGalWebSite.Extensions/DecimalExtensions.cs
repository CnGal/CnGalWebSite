using System;

namespace CnGalWebSite.Extensions
{
    /// <summary>
    /// 数值扩展方法
    /// </summary>
    public static class DecimalExtensions
    {
        /// <summary>
        /// 格式化数值显示，整数不显示小数点，小数移除尾随零
        /// </summary>
        /// <param name="value">要格式化的数值</param>
        /// <returns>格式化后的字符串</returns>
        public static string ToFormattedString(this decimal value)
        {
            // 考虑浮点数误差，如果与整数的差值小于0.000001，则认为是整数
            if (Math.Abs(value - Math.Round(value)) < 0.000001m)
            {
                return Math.Round(value).ToString("F0");
            }
            else
            {
                // 移除尾随的零
                return value.ToString("G29");
            }
        }

        /// <summary>
        /// 格式化数值显示，保留指定精度但移除尾随零（主要用于平均值等统计数据）
        /// </summary>
        /// <param name="value">要格式化的数值</param>
        /// <param name="maxDecimalPlaces">最大小数位数，默认2位</param>
        /// <returns>格式化后的字符串</returns>
        public static string ToFormattedStringWithPrecision(this decimal value, int maxDecimalPlaces = 2)
        {
            // 考虑浮点数误差，如果与整数的差值小于0.000001，则认为是整数
            if (Math.Abs(value - Math.Round(value)) < 0.000001m)
            {
                return Math.Round(value).ToString("F0");
            }
            else
            {
                // 保留指定精度，但移除尾随的零
                var rounded = Math.Round(value, maxDecimalPlaces);
                return rounded.ToString("G29");
            }
        }

        /// <summary>
        /// 格式化可空数值显示
        /// </summary>
        /// <param name="value">要格式化的可空数值</param>
        /// <param name="defaultValue">当值为null时返回的默认字符串，默认为"-"</param>
        /// <returns>格式化后的字符串</returns>
        public static string ToFormattedString(this decimal? value, string defaultValue = "-")
        {
            return value.HasValue ? value.Value.ToFormattedString() : defaultValue;
        }

        /// <summary>
        /// 格式化可空数值显示，保留指定精度
        /// </summary>
        /// <param name="value">要格式化的可空数值</param>
        /// <param name="maxDecimalPlaces">最大小数位数，默认2位</param>
        /// <param name="defaultValue">当值为null时返回的默认字符串，默认为"-"</param>
        /// <returns>格式化后的字符串</returns>
        public static string ToFormattedStringWithPrecision(this decimal? value, int maxDecimalPlaces = 2, string defaultValue = "-")
        {
            return value.HasValue ? value.Value.ToFormattedStringWithPrecision(maxDecimalPlaces) : defaultValue;
        }

        /// <summary>
        /// 将double转换为decimal并格式化显示
        /// </summary>
        /// <param name="value">要格式化的double值</param>
        /// <returns>格式化后的字符串</returns>
        public static string ToFormattedString(this double value)
        {
            return ((decimal)value).ToFormattedString();
        }

        /// <summary>
        /// 将float转换为decimal并格式化显示
        /// </summary>
        /// <param name="value">要格式化的float值</param>
        /// <returns>格式化后的字符串</returns>
        public static string ToFormattedString(this float value)
        {
            return ((decimal)value).ToFormattedString();
        }
    }
}
