using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.PlayedGames
{
    public class PlayedGameService : IPlayedGameService
    {
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IAppHelper _appHelper;

        public PlayedGameService(IAppHelper appHelper, IRepository<PlayedGame, long> playedGameRepository)
        {
            _playedGameRepository = playedGameRepository;
            _appHelper = appHelper;
        }

        public async Task<QueryData<ListPlayedGameAloneModel>> GetPaginatedResult(DataModel.ViewModel.Search.QueryPageOptions options, ListPlayedGameAloneModel searchModel)
        {
            var items = _playedGameRepository.GetAll()
                .Include(s => s.ApplicationUser)
                .Include(s => s.Entry).AsNoTracking();

            // 处理高级搜索
            if (searchModel.GameId!=0)
            {
                items = items.Where(item => item.EntryId.ToString().Contains(searchModel.GameId.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchModel.UserId))
            {
                items = items.Where(item => item.ApplicationUserId.Contains(searchModel.UserId, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.UserName))
            {
                items = items.Where(item => item.ApplicationUser.UserName.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.GameName))
            {
                items = items.Where(item => item.Entry.Name.Contains(searchModel.GameName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.PlayImpressions))
            {
                items = items.Where(item => item.PlayImpressions.Contains(searchModel.PlayImpressions, StringComparison.OrdinalIgnoreCase));
            }
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.EntryId.ToString().Contains(options.SearchText)
                             || item.ApplicationUserId.Contains(options.SearchText)
                             || item.PlayImpressions.Contains(options.SearchText));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {

                items = items.OrderBy(s => s.Id).Sort(options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var itemsReal = await items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToListAsync();

            //复制数据
            var resultItems = new List<ListPlayedGameAloneModel>();
            foreach (var item in itemsReal)
            {
                resultItems.Add(new ListPlayedGameAloneModel
                {
                    Id = item.Id,
                    ScriptSocre = item.ScriptSocre,
                    ShowPublicly = item.ShowPublicly,
                    ShowSocre = item.ShowSocre,
                    SystemSocre = item.SystemSocre,
                    CVSocre = item.CVSocre,
                    IsInSteam = item.IsInSteam,
                    MusicSocre = item.MusicSocre,
                    PaintSocre = item.PaintSocre,
                    TotalSocre = item.TotalSocre,
                    IsHidden = item.IsHidden,
                    LastEditTime = item.LastEditTime,
                    PlayDuration = item.PlayDuration,
                    PlayImpressions = item.PlayImpressions,
                    Type = item.Type,
                    GameId=item.EntryId??0,
                    GameName=item.Entry?.Name,
                    UserName=item.ApplicationUser?.UserName,
                    UserId=item.ApplicationUserId
                });
            }

            return new QueryData<ListPlayedGameAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }

        public void UpdatePlayedGameDataMain(PlayedGame playedGame, PlayedGameMain examine)
        {
            playedGame.PlayImpressions = examine.PlayImpressions;
            playedGame.ShowPublicly = examine.ShowPublicly;

            //更新最后编辑时间
            playedGame.LastEditTime = DateTime.Now.ToCstTime();

        }
        public  void UpdateArticleData(PlayedGame playedGame, Examine examine)
        {
            switch (examine.Operation)
            {
               
                case Operation.EditPlayedGameMain:
                    PlayedGameMain playedGameMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        playedGameMain = (PlayedGameMain)serializer.Deserialize(str, typeof(PlayedGameMain));
                    }

                     UpdatePlayedGameDataMain(playedGame, playedGameMain);
                    break;
             
            }
        }
    }
}
