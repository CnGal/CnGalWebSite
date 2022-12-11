using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class FavoriteFolderViewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }

        /// <summary>
        /// 相关性列表
        /// </summary>
        public List<FavoriteObjectAloneViewModel> Objects { get; set; } = new List<FavoriteObjectAloneViewModel>();

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInforViewModel UserInfor { get; set; } = new UserInforViewModel();
        /// <summary>
        /// 是否有权限编辑
        /// </summary>
        public bool Authority { get; set; }
        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool IsEdit { get; set; }
        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }

        public bool IsHidden { get; set; }

        public int TabIndex { get; set; }

        public EditState MainState { get; set; }
    }
}
