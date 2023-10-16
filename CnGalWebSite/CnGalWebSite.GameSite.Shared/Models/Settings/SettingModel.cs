using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.GameSite.Shared.Models.Settings
{
    public class SettingModel
    {
        public static string DefaultThemeColor = "pink";
        public static string[] AvailableColors = ["red", "pink", "purple", "deep-purple", "indigo", "blue", "light-blue", "cyan", "teal", "green", "light-green", "lime", "yellow", "amber", "orange", "deep-orange", "brown", "blue-grey", "grey"];

        /// <summary>
        /// 主题颜色
        /// </summary>
        public string ThemeColor { get; set; } = DefaultThemeColor;

        /// <summary>
        /// 主题模式
        /// </summary>
        public ThemeMode ThemeMode { get; set; }

        /// <summary>
        /// 最终是否为深色模式
        /// </summary>
        public bool IsDark;

        /// <summary>
        /// 字体
        /// </summary>
        public string FontFamily { get; set; }
    }

    public enum ThemeMode
    {
        [Display(Name = "跟随系统")]
        System,
        [Display(Name = "浅色")]
        Light,
        [Display(Name = "深色")]
        Dark,
    }
}
