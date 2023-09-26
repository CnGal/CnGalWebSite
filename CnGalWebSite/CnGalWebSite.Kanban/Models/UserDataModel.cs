using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class UserDataModel
    {
        /// <summary>
        /// 当前服装
        /// </summary>
        public ClothesDataModel Clothes { get; set; } = new ClothesDataModel();
    }

    public class ClothesDataModel
    {
        public string CurrentName { get; set; }
    }

}
