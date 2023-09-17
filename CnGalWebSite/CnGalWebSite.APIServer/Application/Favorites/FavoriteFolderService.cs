using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel.FavoriteFolders;
using CnGalWebSite.DataModel.ExamineModel.PlayedGames;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public class FavoriteFolderService : IFavoriteFolderService
    {
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IAppHelper _appHelper;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<FavoriteFolder>, string, SortOrder, IEnumerable<FavoriteFolder>>> SortLambdaCache = new();
       

        public FavoriteFolderService(IRepository<FavoriteFolder, long> favoriteFolderRepository, IAppHelper appHelper)
        {
            _favoriteFolderRepository = favoriteFolderRepository;
            _appHelper = appHelper;
        }

        public void UpdateMain(FavoriteFolder favoriteFolder, FavoriteFolderMain examine)
        {
            favoriteFolder.MainImage = examine.MainImage;
            favoriteFolder.Name = examine.Name;
            favoriteFolder.BriefIntroduction = examine.BriefIntroduction;
            favoriteFolder.ShowPublicly = examine.ShowPublicly;

            //更新最后编辑时间
            favoriteFolder.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateData(FavoriteFolder favoriteFolder, Examine examine)
        {
            switch (examine.Operation)
            {

                case Operation.EditFavoriteFolderMain:
                    FavoriteFolderMain favoriteFolderMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        favoriteFolderMain = (FavoriteFolderMain)serializer.Deserialize(str, typeof(FavoriteFolderMain));
                    }

                    UpdateMain(favoriteFolder, favoriteFolderMain);
                    break;

            }
        }

        /// <summary>
        /// 获取视图模型
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public FavoriteFolderViewModel GetViewModel(FavoriteFolder folder)
        {
            //建立视图模型
            var model = new FavoriteFolderViewModel
            {
                Id = folder.Id,
                Name = folder.Name,
                BriefIntroduction=folder.BriefIntroduction,
                CreateTime = folder.CreateTime,
                IsHidden = !folder.ShowPublicly,
                MainPicture=_appHelper.GetImagePath(folder.MainImage,"app.png"),
                ReaderCount=folder.ReaderCount,
                LastEditTime = folder.LastEditTime,             
            };

            //初始化图片
            model.MainPicture = _appHelper.GetImagePath(folder.MainImage, "app.png");



            foreach (var item in folder.FavoriteObjects)
            {
                if (item.Type == FavoriteObjectType.Article)
                {
                    model.Objects.Add(new FavoriteObjectAloneViewModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(item.Article)
                    });
                }
                else if (item.Type == FavoriteObjectType.Entry)
                {
                    model.Objects.Add(new FavoriteObjectAloneViewModel
                    {
                        entry = _appHelper.GetEntryInforTipViewModel(item.Entry)
                    });
                }
                else if (item.Type == FavoriteObjectType.Periphery)
                {
                    model.Objects.Add(new FavoriteObjectAloneViewModel
                    {
                        periphery = _appHelper.GetPeripheryInforTipViewModel(item.Periphery)
                    });
                }
                else if (item.Type == FavoriteObjectType.Video)
                {
                    model.Objects.Add(new FavoriteObjectAloneViewModel
                    {
                        Video = _appHelper.GetVideoInforTipViewModel(item.Video)
                    });
                }
                else if (item.Type == FavoriteObjectType.Tag)
                {
                    model.Objects.Add(new FavoriteObjectAloneViewModel
                    {
                        Tag = _appHelper.GetTagInforTipViewModel(item.Tag)
                    });
                }
            }

            return model;
        }
    }
}
