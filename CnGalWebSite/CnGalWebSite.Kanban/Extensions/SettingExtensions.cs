using CnGalWebSite.Kanban.Models;
using Masa.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Extensions
{
    public static class SettingExtensions
    {
        public static string GetStyles(this KanbanSettingModel kanban)
        {
            return $"left: {kanban.Position.Left}px;top: {kanban.Position.Top}px;width: {kanban.Size.Width}px;height: {kanban.Size.Height}px;";
        }

        public static string GetStyles(this ButtonSettingModel button)
        {
            return $"left: {button.Position.Left}px;bottom: {button.Position.Bottom}px;";
        }

        public static string GetStyles(this DialogBoxSettingModel dialog)
        {
            return $"left: {dialog.Position.Left}px;bottom: {dialog.Position.Bottom}px;";
        }

        public static string GetWidthStyles(this ChatCardSettingModel dialog)
        {
            return $"width:{dialog.Width}px;";
        }

        public static string GetStyles(this ChatCardSettingModel dialog)
        {
            return $"left: {dialog.Position.Left}px;bottom: {dialog.Position.Bottom}px;";
        }

        public static string GetMessageListStyles(this ChatCardSettingModel dialog)
        {
            return $"max-height: {dialog.ContentHeight}px;";
        }

        public static string GetContentStyles(this DialogBoxSettingModel dialog)
        {
            return $"width: {dialog.Width}px;min-height: 125px;flex-direction: row;";
        }

        public static string GetFloatButtonStyles(this ButtonSettingModel button)
        {
            return $"height: {button.Size}px;width: {button.Size}px;";
        }

        public static (int x, int y, int width, int height) GetCircleKanbanImageSize(this KanbanSettingModel kanban)
        {
            var radio_total = 2.0 / 3;

            var radio_width = 0.5;
            var radio_height = 0.3;

            var width_total = kanban.Size.Width * radio_total;
            var height_total = kanban.Size.Height * radio_total;

            var width_image = width_total * radio_width;
            var height_image = height_total * radio_height;


            var diff_width = (width_total - width_image) / 2;
            var diff_height = (height_total - height_image) / 2;

            // x     y
            // 边框大小
            // 300 * 500
            // 图像大小
            // 240 * 500
            // 头像大小
            // 150 *150
            // 正中心


            var image_w = kanban.Size.Height * 240 / 500;
            var re_w = image_w * 150 / 240;
            if (re_w > kanban.Size.Width)
            {
                re_w = kanban.Size.Width;
            }


            return ((kanban.Size.Width - re_w) / 2, kanban.Size.Height - re_w, re_w, re_w);
        }
    }
}
