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
        public string ClothesName { get; set; }
        public string StockingsName { get; set; } = "stocking2";
        public string ShoesName { get; set; } = "shoes1";
    }
}
