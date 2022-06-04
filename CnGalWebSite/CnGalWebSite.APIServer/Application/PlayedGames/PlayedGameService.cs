using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
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

        public async Task<PagedResultDto<EntryInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input)
        {
            var query = _playedGameRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == input.FilterText);

            //统计查询数据的总条数
            var count = query.Count(s => s.EntryId != 0);
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<Entry> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.Entry).Where(s => s.EntryId != 0).Select(s => s.Entry).ToListAsync();
            }
            else
            {
                models = new List<Entry>();
            }

            var dtos = new List<EntryInforTipViewModel>();
            foreach (var item in models)
            {
                if (item == null)
                {
                    continue;
                }

                dtos.Add(new EntryInforTipViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.Name : item.DisplayName,
                    MainImage = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    BriefIntroduction = item.BriefIntroduction,
                    LastEditTime = item.LastEditTime,
                    ReaderCount = item.ReaderCount,
                    CommentCount = item.CommentCount
                });

            }
            var dtos_ = new PagedResultDto<EntryInforTipViewModel>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = dtos,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };
            return dtos_;
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
