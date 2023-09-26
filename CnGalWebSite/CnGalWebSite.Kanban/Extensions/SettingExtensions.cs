using CnGalWebSite.Kanban.Models;
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

        public static string GetContentStyles(this DialogBoxSettingModel dialog)
        {
            return $"width: {dialog.Width}px;";
        }

        public static string GetFloatButtonStyles(this ButtonSettingModel button)
        {
            return $"height: {button.Size}px;width: {button.Size}px;";
        }

    }
}
