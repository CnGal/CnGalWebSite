using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Anniversaries;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.APIServer.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/anniversaries/[action]")]
    public class AnniversariesAPIController : ControllerBase
    {
        
        
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IUserService _userService;
        private readonly IEntryService _entryService;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;

        public AnniversariesAPIController(IUserService userService,  IAppHelper appHelper, IRepository<Entry, int> entryRepository,IRepository<PlayedGame, long> playedGameRepository)
        {
            
            _entryRepository = entryRepository;
            _appHelper = appHelper;            
            _userService = userService;
            _playedGameRepository = playedGameRepository;
        }


        static readonly DateTime before = new DateTime(2023, 5, 31);
        static readonly DateTime after = new DateTime(2022, 5, 31);

        [HttpGet]
        public async Task<ActionResult<List<JudgableGameViewModel>>> GetAllJudgableGamesAsync()
        {


            var games = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.PlayedGames)
                .Where(s => s.Type == EntryType.Game && s.PubulishTime != null && s.Id != 139 && s.Id != 3412 && s.Id != 3835 && s.IsHidden == false&& s.PubulishTime.Value.Date <= before.Date && s.PubulishTime.Value.Date >= after.Date)
                .ToListAsync();
            var model = new List<JudgableGameViewModel>();
            foreach (var item in games)
            {

                var temp = new JudgableGameViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.Name : item.DisplayName,
                    MainImage = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    BriefIntroduction = item.BriefIntroduction,
                    LastEditTime = item.LastEditTime,
                    ReaderCount = item.ReaderCount,
                    CommentCount = item.CommentCount,
                    PublishTime = item.PubulishTime.Value
                };

                //处理评分信息
                if (item.PlayedGames != null && item.PlayedGames.Any())
                {
                    temp.LastScoreTime = item.PlayedGames.Max(s => s.LastEditTime);
                    temp.ScoreCount = item.PlayedGames.Count(s => s.IsScored && s.ShowPublicly);
                }

                model.Add(temp);
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlayedGameUserScoreRandomModel>>> GetRandomUserScoresAsync()
        {
            var games = await _playedGameRepository.GetAll().AsNoTracking()
                .Include(s => s.ApplicationUser)
                .Include(s=>s.Entry).ThenInclude(s=>s.Tags)
                .Where(s =>  s.Entry.PubulishTime != null && s.Entry.Id != 139 && s.Entry.Id != 3412 && s.Entry.Id != 3835 && s.Entry.IsHidden==false && s.Entry.PubulishTime.Value.Date <= before.Date && s.Entry.PubulishTime.Value.Date >= after.Date)
                .Where(s =>s.ShowPublicly && s.MusicSocre != 0 && s.PaintSocre != 0 && s.CVSocre != 0 && s.SystemSocre != 0 && s.ScriptSocre != 0 && s.TotalSocre != 0 && s.CVSocre != 0 && string.IsNullOrWhiteSpace(s.PlayImpressions)==false && s.PlayImpressions.Length > ToolHelper.MinValidPlayImpressionsLength)
                .ToListAsync();
            var model = new List<PlayedGameUserScoreRandomModel>();
            foreach (var item in games)
            {

                var temp = new PlayedGameUserScoreRandomModel
                {
                    Socres = new PlayedGameScoreModel
                    {
                        ScriptSocre = item.ScriptSocre,
                        ShowSocre = item.ShowSocre,
                        MusicSocre = item.MusicSocre,
                        PaintSocre = item.PaintSocre,
                        TotalSocre = item.TotalSocre,
                        SystemSocre = item.SystemSocre,
                        CVSocre = item.CVSocre,

                    },
                    LastEditTime = item.LastEditTime,
                    PlayImpressions = item.PlayImpressions,
                    User = await _userService.GetUserInforViewModel(item.ApplicationUser, true),
                    GameId = item.Entry.Id,
                    GameName = item.Entry.Name,
                    IsDubbing = !(item.Entry.Tags != null && item.Entry.Tags.Any(s => s.Name == "无配音"))
                };

                model.Add(temp);
            }

            return model;
        }

        /// <summary>
        /// 获取参与的制作组列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryInforTipViewModel>>> GetGroupListAsync()
        {
            var games = await _entryRepository.GetAll().AsNoTracking()
                .Where(s=>s.Priority==1 && s.Type== EntryType.ProductionGroup)
                .OrderBy(s=>s.DisplayName)
                .ToListAsync();

            var model = new List<EntryInforTipViewModel>();
            foreach (var item in games)
            {
                var temp = _appHelper.GetEntryInforTipViewModel(item);

                model.Add(temp);
            }

            return model;
        }

    }
}
