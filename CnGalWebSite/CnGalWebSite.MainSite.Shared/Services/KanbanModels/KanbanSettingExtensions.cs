using System;

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels;

public static class KanbanSettingExtensions
{
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

        var image_w = kanban.Size.Height * 240 / 500;
        var re_w = image_w * 150 / 240;
        if (re_w > kanban.Size.Width)
        {
            re_w = kanban.Size.Width;
        }

        return ((kanban.Size.Width - re_w) / 2, kanban.Size.Height - re_w, re_w, re_w);
    }
}
