using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class SettingModel
    {
        /// <summary>
        /// 看板娘Live2D
        /// </summary>
        public KanbanSettingModel Kanban { get; set; }=new KanbanSettingModel();

        /// <summary>
        /// 按钮
        /// </summary>
        public ButtonSettingModel Button { get; set; } = new ButtonSettingModel();

        /// <summary>
        /// 对话框
        /// </summary>
        public DialogBoxSettingModel DialogBox { get; set; } = new DialogBoxSettingModel();

        /// <summary>
        /// 聊天框
        /// </summary>
        public ChatCardSettingModel Chat { get; set; } = new ChatCardSettingModel();

    }

    public class KanbanSettingModel
    {
        /// <summary>
        /// 坐标
        /// </summary>
        public Position Position { get; set; } = new Position();

        /// <summary>
        /// 大小
        /// </summary>
        public Size Size { get; set; } = new Size
        {
            Height = 500,
            Width = 300,
        };

    }

    public class ButtonSettingModel
    {
        public Position Position { get; set; } = new Position
        {
            Left = 220,
            Top = 130
        };

        public int Size { get; set; } = 30;
    }

    public class DialogBoxSettingModel
    {
        public Position Position { get; set; } = new Position
        {
            Left = -110,
            Bottom = 525
        };

        public int Width { get; set; } = 400;

        public bool Hide { get; set; }
    }

    public class ChatCardSettingModel
    {
        public Position Position { get; set; } = new Position
        {
            Left = -110,
            Bottom = 525
        };

        public int Width { get; set; } = 370;

        public int ContentHeight { get; set; } = 400;
    }

    public class Position
    {
        /// <summary>
        /// X
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// Y
        /// </summary>
        public int Top { get; set; }

        public int Bottom { get; set; }
    }

    public class Size
    {
        public int Height { get; set; }

        public int Width { get; set; }
    }
}
