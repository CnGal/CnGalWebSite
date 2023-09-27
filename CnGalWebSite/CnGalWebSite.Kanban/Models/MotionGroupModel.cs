using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class MotionGroupModel
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public List<MotionModel> Motions { get; set; }=new List<MotionModel>();
    }

    public class MotionModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
