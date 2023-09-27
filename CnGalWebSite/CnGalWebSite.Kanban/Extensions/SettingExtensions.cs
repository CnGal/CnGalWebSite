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
            return $"width: {dialog.Width}px;min-height: 125px;flex-direction: row;";
        }

        public static string GetFloatButtonStyles(this ButtonSettingModel button)
        {
            return $"height: {button.Size}px;width: {button.Size}px;";
        }
        public static string GetDialogKanbanImageStyles(this KanbanSettingModel kanban)
        {
            var radio =333.33/ kanban.Size.Height;
            var width = kanban.Size.Width * radio;
            var diff = (width - 200) / 2;

            return $"position: absolute;clip: rect(0px,{150+diff:0}px,100px,{50+diff:0}px);width: {width}px;margin-left: {-50-diff}px;";
        }

    }
}
