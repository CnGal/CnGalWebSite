#nullable enable

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels
{
    public class KanbanSettingModel
    {
        public Position Position { get; set; } = new() { Left = 1444, Top = 648 };
        public Size Size { get; set; } = new() { Height = 617, Width = 341 };
    }

    public class ButtonSettingModel
    {
        public Position Position { get; set; } = new() { Left = 220, Top = 130 };
        public int Size { get; set; } = 30;
    }

    public class DialogBoxSettingModel
    {
        public Position Position { get; set; } = new() { Left = 112, Top = -89, Bottom = 626 };
        public int Width { get; set; } = 400;
        public bool Hide { get; set; }
    }

    public class ChatCardSettingModel
    {
        public Position Position { get; set; } = new() { Left = -110, Bottom = 525 };
        public int Width { get; set; } = 370;
        public int ContentHeight { get; set; } = 400;
    }

    public class Position
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
    }

    public class Size
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
