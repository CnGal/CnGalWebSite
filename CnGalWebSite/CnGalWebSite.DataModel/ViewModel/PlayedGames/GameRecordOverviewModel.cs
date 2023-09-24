using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class GameRecordOverviewModel
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }
        /// <summary>
        /// 游戏Id
        /// </summary>
        public int GameId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 评语
        /// </summary>
        public string PlayImpressions { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public PlayedGameType Type { get; set; }
        /// <summary>
        /// 总分
        /// </summary>
        public int TotalSocre { get; set; }
        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// 是否公开
        /// </summary>
        public bool ShowPublicly { get; set; }
    }
}
