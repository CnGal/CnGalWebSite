using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tables;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Console = System.Console;

namespace CnGalWebSite.APIServer.Application.PlayedGames
{
    public class PlayedGameService : IPlayedGameService
    {
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<GameScoreTableModel, long> _gameScoreTableRepository;
        private readonly IAppHelper _appHelper;

        public PlayedGameService(IAppHelper appHelper, IRepository<PlayedGame, long> playedGameRepository, IRepository<GameScoreTableModel, long> gameScoreTableRepository,
            IRepository<Entry, int> entryRepository)
        {
            _playedGameRepository = playedGameRepository;
            _appHelper = appHelper;
            _gameScoreTableRepository = gameScoreTableRepository;
            _entryRepository = entryRepository;
        }

        public async Task<QueryData<ListPlayedGameAloneModel>> GetPaginatedResult(DataModel.ViewModel.Search.QueryPageOptions options, ListPlayedGameAloneModel searchModel)
        {
            var items = _playedGameRepository.GetAll()
                .Include(s => s.ApplicationUser)
                .Include(s => s.Entry).AsNoTracking();

            // 处理高级搜索
            if (searchModel.GameId != 0)
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
                    GameId = item.EntryId ?? 0,
                    GameName = item.Entry?.Name,
                    UserName = item.ApplicationUser?.UserName,
                    UserId = item.ApplicationUserId
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
        public void UpdatePlayedGameData(PlayedGame playedGame, Examine examine)
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

        public async Task<GameScoreTableModel> GetGameScores(int id)
        {
            return await _gameScoreTableRepository.FirstOrDefaultAsync(s => s.GameId == id);
        }

        public async Task UpdateAllGameScore()
        {
            //获取所有评分
            var globalAllScores = await _playedGameRepository.GetAll().AsNoTracking()
                .Where(s => s.ShowPublicly)
                 .Where(s => s.CVSocre != 0 && s.ShowSocre != 0 && s.SystemSocre != 0 && s.MusicSocre != 0 && s.PaintSocre != 0 && s.TotalSocre != 0 && s.ScriptSocre != 0)
                .ToListAsync();

            //过滤评分
            var globalFilterScores = globalAllScores.Where(s => string.IsNullOrWhiteSpace(s.PlayImpressions) == false && s.PlayImpressions.Length > ToolHelper.MinValidPlayImpressionsLength);

            //获取所有有评分的游戏
            var games = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.PlayedGames.Any(s => s.ShowPublicly && s.CVSocre != 0 && s.ShowSocre != 0 && s.SystemSocre != 0 && s.MusicSocre != 0 && s.PaintSocre != 0 && s.TotalSocre != 0 && s.ScriptSocre != 0))
                .Select(s => new
                {
                    s.DisplayName,
                    s.Id
                })
                .ToListAsync();

            //获取全局分布
            var globalDistribution = GetGlobalDistribution(globalAllScores);
            var globalFilterDistribution = GetGlobalDistribution(globalFilterScores);

            //遍历计算每个游戏的分数并更新
            foreach (var item in games)
            {
                await UpdateGameScore(item.Id, item.DisplayName, globalAllScores, globalDistribution, globalFilterDistribution);
            }
        }

        /// <summary>
        /// 计算出全局分布D
        /// </summary>
        /// <param name="scores"></param>
        /// <returns></returns>
        public Dictionary<PlayedGameScoreType, long[]> GetGlobalDistribution(IEnumerable<PlayedGame> scores)
        {
            var globalDistribution = new Dictionary<PlayedGameScoreType, long[]>();

            globalDistribution.Add(PlayedGameScoreType.CV, GetDistribution(scores.Select(s => s.CVSocre)));
            globalDistribution.Add(PlayedGameScoreType.Show, GetDistribution(scores.Select(s => s.ShowSocre)));
            globalDistribution.Add(PlayedGameScoreType.System, GetDistribution(scores.Select(s => s.SystemSocre)));
            globalDistribution.Add(PlayedGameScoreType.Music, GetDistribution(scores.Select(s => s.MusicSocre)));
            globalDistribution.Add(PlayedGameScoreType.Paint, GetDistribution(scores.Select(s => s.PaintSocre)));
            globalDistribution.Add(PlayedGameScoreType.Total, GetDistribution(scores.Select(s => s.TotalSocre)));
            globalDistribution.Add(PlayedGameScoreType.Script, GetDistribution(scores.Select(s => s.ScriptSocre)));

            return globalDistribution;
        }

        /// <summary>
        /// 更新游戏评分
        /// </summary>
        /// <param name="id">游戏id</param>
        /// <param name="name">游戏名</param>
        /// <param name="globalAllScores">所有用户评分</param>
        /// <param name="globalDistribution">全局分布</param>
        /// <param name="globalFilterDistribution">过滤后的全局分布</param>
        /// <returns></returns>
        public async Task UpdateGameScore(int id, string name, IEnumerable<PlayedGame> globalAllScores, Dictionary<PlayedGameScoreType, long[]> globalDistribution ,Dictionary<PlayedGameScoreType, long[]> globalFilterDistribution)
        {
            //获取当前游戏的所有玩家评分
            var scores = globalAllScores.Where(s => s.EntryId == id);

            //获取当前评分缓存
            var model = await _gameScoreTableRepository.FirstOrDefaultAsync(s => s.GameId == id);

            if (model == null)
            {
                if (scores.Any() == false)
                {
                    return;
                }

                model = await _gameScoreTableRepository.InsertAsync(new GameScoreTableModel
                {
                    GameId = id,
                });
            }
            else
            {
                if (scores.Any() == false)
                {
                    await _gameScoreTableRepository.DeleteAsync(model);
                }
            }
            model.GameName = name;

            //依次计算
            model.AllCVSocre = CalculateScore(scores.Select(s => s.CVSocre), globalDistribution[ PlayedGameScoreType.CV]);
            model.AllShowSocre = CalculateScore(scores.Select(s => s.ShowSocre), globalDistribution[PlayedGameScoreType.Show]);
            model.AllSystemSocre = CalculateScore(scores.Select(s => s.SystemSocre), globalDistribution[PlayedGameScoreType.System]);
            model.AllMusicSocre = CalculateScore(scores.Select(s => s.MusicSocre), globalDistribution[PlayedGameScoreType.Music]);
            model.AllPaintSocre = CalculateScore(scores.Select(s => s.PaintSocre), globalDistribution[PlayedGameScoreType.Paint]);
            model.AllTotalSocre = CalculateScore(scores.Select(s => s.TotalSocre), globalDistribution[PlayedGameScoreType.Total]);
            model.AllScriptSocre = CalculateScore(scores.Select(s => s.ScriptSocre), globalDistribution[PlayedGameScoreType.Script]);

            var filterScores = scores.Where(s => string.IsNullOrWhiteSpace(s.PlayImpressions) == false && s.PlayImpressions.Length > ToolHelper.MinValidPlayImpressionsLength);
            if (filterScores != null && filterScores.Any())
            {
                //依次计算
                model.FilterCVSocre = CalculateScore(filterScores.Select(s => s.CVSocre), globalFilterDistribution[PlayedGameScoreType.CV]);
                model.FilterShowSocre = CalculateScore(filterScores.Select(s => s.ShowSocre), globalFilterDistribution[PlayedGameScoreType.Show]);
                model.FilterSystemSocre = CalculateScore(filterScores.Select(s => s.SystemSocre), globalFilterDistribution[PlayedGameScoreType.System]);
                model.FilterMusicSocre = CalculateScore(filterScores.Select(s => s.MusicSocre), globalFilterDistribution[PlayedGameScoreType.Music]);
                model.FilterPaintSocre = CalculateScore(filterScores.Select(s => s.PaintSocre), globalFilterDistribution[PlayedGameScoreType.Paint]);
                model.FilterTotalSocre = CalculateScore(filterScores.Select(s => s.TotalSocre), globalFilterDistribution[PlayedGameScoreType.Total]);
                model.FilterScriptSocre = CalculateScore(filterScores.Select(s => s.ScriptSocre), globalFilterDistribution[PlayedGameScoreType.Script]);

            }

            await _gameScoreTableRepository.UpdateAsync(model);

        }

        /// <summary>
        /// 计算评分
        /// </summary>
        /// <param name="scores">当前游戏的评分</param>
        /// <param name="g">全局分布</param>
        /// <returns></returns>
        public double CalculateScore(IEnumerable<int> scores, long[] g)
        {
            var s = GetDistribution(scores);

            return CalculateScoreByDirichletDistribution(g, s);
        }

        /// <summary>
        /// 获取评分的 n维分布
        /// </summary>
        /// <remarks>
        /// 如果我们允许用户评分为n档，那么，每一个用户的打分可以格式化为一个n维的0-1向量，这样，每一部电影的评分分布实际上为一个多元分布
        /// </remarks>
        /// <param name="scores">分数</param>
        /// <param name="n">n档 不包括0</param>
        /// <returns></returns>
        public long[] GetDistribution(IEnumerable<int> scores,int n=10)
        {
            var d = new long[n];

            for (int i = 0; i < n; i++)
            {
                d[i] = scores.Count(s => s == i + 1);
            }

            return d;
        }

        /// <summary>
        /// 使用 Dirichlet分布 计算分数
        /// </summary>
        /// <remarks>
        /// 1. 预估先验分布D(α1,α2,...,αn),可以采用领域知识，也可以直接用全局分布替代;
        /// 2. 计算每一产品的多元分布M(β1, β2,..., βn);
        /// 3. 用多元分布修正先验分布为D(α1+β1, α2+β2,..., αn+βn);
        /// 4. 计算分布D(α1+β1, α2+β2,..., αn+βn)的均值为(m^1，m^2,..., m^n);
        /// 5. 利用步骤4中得到的均值，采用加权平均的方法求取最终得分。
        /// 参考资料 http://xinsong.github.io/2014/10/13/rank_products/
        /// </remarks>
        /// <param name="globalDistribution">先验分布</param>
        /// <param name="singleMultivariateDistribution">多元分布</param>
        /// <returns></returns>
        public double CalculateScoreByDirichletDistribution(long[] globalDistribution, long[] singleMultivariateDistribution)
        {
            var n = globalDistribution.Count();

            long[] d = new long[n];
            long sum = 0;

            for (int i = 0; i < n; i++)
            {
                //修正先验分布
                d[i] = globalDistribution[i] + singleMultivariateDistribution[i];
                //求和
                sum += d[i];
            }

            double score = 0;

            for (int i = 0; i < n; i++)
            {
                //计算分布D的均值 加权平均
                score += (double)d[i] *  (i + 1) / sum;
            }

            return score;
        }

     
    }
}
