using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class ToolModel
    {
        public string Name { get; set; }

        public string Icon { get; set; }

        public static List<ToolModel> GetAllTools()
        {
            return new List<ToolModel>
            {
                new ToolModel
                {
                    Name="转载视频",
                    Icon="mdi-animation-play",
                },new ToolModel
                {
                    Name="转载文章",
                    Icon="mdi-script-text",
                },new ToolModel
                {
                    Name="合并词条",
                    Icon="mdi-archive",
                },
            };
        }
    }
}
