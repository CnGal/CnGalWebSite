using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries.Dtos;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Models;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.Helper.Extensions;
using Microsoft.EntityFrameworkCore;
using Nest;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Tag = CnGalWebSite.DataModel.Model.Tag;

namespace CnGalWebSite.APIServer.Application.Entries
{
    public class EntryService : IEntryService
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, int> _articleRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<DataModel.Model.Tag, int> _tagRepository;
        private readonly IAppHelper _appHelper;
        private readonly IArticleService _articleService;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<RoleBirthday, long> _roleBirthdayRepository;
        private readonly IRepository<BookingUser, long> _bookingUserRepository;
        private readonly ILogger<EntryService> _logger;
        private readonly IEmailService _emailService;
        private readonly IViewRenderService _viewRenderService;
        private readonly ISteamInforService _steamInforService;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Entry>, string, BootstrapBlazor.Components.SortOrder, IEnumerable<Entry>>> SortLambdaCacheEntry = new();

        public EntryService(IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<DataModel.Model.Tag, int> tagRepository, IRepository<Article, int> articleRepository, IRepository<PlayedGame, long> playedGameRepository, ISteamInforService steamInforService,
        IRepository<Examine, long> examineRepository, IArticleService articleService, IRepository<Video, long> videoRepository, IRepository<RoleBirthday, long> roleBirthdayRepository, ILogger<EntryService> logger, IRepository<Lottery, long> lotteryRepository,
         IEmailService emailService, IRepository<BookingUser, long> bookingUserRepository, IViewRenderService viewRenderService)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _tagRepository = tagRepository;
            _examineRepository = examineRepository;
            _articleRepository = articleRepository;
            _articleService = articleService;
            _playedGameRepository = playedGameRepository;
            _videoRepository = videoRepository;
            _roleBirthdayRepository = roleBirthdayRepository;
            _logger = logger;
            _lotteryRepository = lotteryRepository;
            _emailService = emailService;
            _bookingUserRepository = bookingUserRepository;
            _viewRenderService = viewRenderService;
            _steamInforService = steamInforService;
        }

        public async Task<PagedResultDto<Entry>> GetPaginatedResult(GetEntryInput input)
        {
            var query = _entryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "游戏":
                        query = query.Where(s => s.Type == EntryType.Game);
                        break;
                    case "角色":
                        query = query.Where(s => s.Type == EntryType.Role);
                        break;
                    case "STAFF":
                        query = query.Where(s => s.Type == EntryType.Staff);
                        break;
                    case "制作组":
                        query = query.Where(s => s.Type == EntryType.ProductionGroup);
                        break;

                }
            }
            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.Name.Contains(input.FilterText)
                  || s.BriefIntroduction.Contains(input.FilterText)
                  || s.MainPage.Contains(input.FilterText));
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<Entry> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.Examines).ToListAsync();
            }
            else
            {
                models = new List<Entry>();
            }


            var dtos = new PagedResultDto<Entry>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };

            return dtos;
        }

        public Task<QueryData<ListEntryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListEntryAloneModel searchModel)
        {
            IEnumerable<Entry> items = _entryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                             || (item.BriefIntroduction?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheEntry.GetOrAdd(typeof(Entry), key => LambdaExtensions.GetSortLambda<Entry>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListEntryAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListEntryAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    IsHidden = item.IsHidden,
                    CanComment = item.CanComment ?? true,
                    BriefIntroduction = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20),
                    Priority = item.Priority,
                    Type = item.Type,
                    IsHideOutlink = item.IsHideOutlink,
                });
            }

            return Task.FromResult(new QueryData<ListEntryAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task<PagedResultDto<EntryInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input)
        {
            var query = _entryRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "游戏":
                        query = query.Where(s => s.Type == EntryType.Game);
                        break;
                    case "角色":
                        query = query.Where(s => s.Type == EntryType.Role);
                        break;
                    case "制作组":
                        query = query.Where(s => s.Type == EntryType.ProductionGroup);
                        break;
                    case "STAFF":
                        query = query.Where(s => s.Type == EntryType.Staff);
                        break;

                }
            }
            //判断输入的查询名称是否为空
            /*  if (!string.IsNullOrWhiteSpace(input.FilterText))
              {
                  query = query.Where(s => s.CreateUserId == input.FilterText);
              }*/
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<Entry> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking()
                    .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .ToListAsync();
            }
            else
            {
                models = new List<Entry>();
            }

            var dtos = new List<EntryInforTipViewModel>();
            foreach (var item in models)
            {
                dtos.Add(_appHelper.GetEntryInforTipViewModel(item));
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

        public async Task<List<int>> GetEntryIdsFromNames(List<string> names)
        {
            //判断关联是否存在
            var entryId = new List<int>();

            foreach (var item in names)
            {
                var infor = await _entryRepository.GetAll().AsNoTracking().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    throw new Exception("词条 " + item + " 不存在");
                }
                else
                {
                    entryId.Add(infor);
                }
            }
            //删除重复数据
            entryId = entryId.Distinct().ToList();

            return entryId;
        }

        public void UpdateEntryDataMain(Entry entry, ExamineMain examine)
        {
            ToolHelper.ModifyDataAccordingToEditingRecord(entry, examine.Items);

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateEntryDataMain(Entry entry, EntryMain_1_0 examine)
        {
            entry.Name = examine.Name;
            entry.BriefIntroduction = examine.BriefIntroduction;
            entry.MainPicture = examine.MainPicture;
            entry.Thumbnail = examine.Thumbnail;
            entry.BackgroundPicture = examine.BackgroundPicture;
            entry.Type = examine.Type;
            entry.DisplayName = examine.DisplayName;
            entry.SmallBackgroundPicture = examine.SmallBackgroundPicture;
            entry.AnotherName = examine.AnotherName;

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public async Task UpdateEntryDataAddInforAsync(Entry entry, EntryAddInfor examine)
        {
            //附加信息
            foreach (var item in examine.Information)
            {
                var entryInformation = entry.Information.FirstOrDefault(s => s.Modifier == item.Modifier && s.DisplayValue == item.DisplayValue && s.DisplayName == item.DisplayName);
                if (entryInformation != null)
                {
                    if (item.IsDelete)
                    {
                        entry.Information.Remove(entryInformation);
                        continue;
                    }
                    if (item.Additional == null)
                    {
                        continue;
                    }
                    foreach (var temp in item.Additional)
                    {
                        var entryAdditional = entryInformation.Additional.FirstOrDefault(s => s.DisplayName == temp.DisplayName);
                        if (entryAdditional != null)
                        {
                            if (temp.IsDelete)
                            {
                                entryInformation.Additional.Remove(entryAdditional);
                                continue;
                            }

                            entryAdditional.DisplayValue = temp.DisplayValue;
                        }
                        else
                        {
                            if (temp.IsDelete == false)
                            {
                                entryInformation.Additional.Add(new BasicEntryInformationAdditional
                                {
                                    DisplayName = temp.DisplayName,
                                    DisplayValue = temp.DisplayValue,
                                });
                            }

                        }
                    }
                }
                else
                {
                    if (item.IsDelete == false)
                    {
                        entryInformation = new BasicEntryInformation
                        {
                            Modifier = item.Modifier,
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue,
                        };
                        entry.Information.Add(entryInformation);

                        if (item.Additional == null)
                        {
                            continue;
                        }
                        foreach (var temp in item.Additional)
                        {
                            if (temp.IsDelete == false)
                            {
                                entryInformation.Additional.Add(new BasicEntryInformationAdditional
                                {
                                    DisplayName = temp.DisplayName,
                                    DisplayValue = temp.DisplayValue,
                                });
                            }
                        }
                    }
                }


            }
            //处理Staff
            foreach (var infor in examine.Staffs)
            {
                var isSame = false;
                foreach (var item in entry.EntryStaffFromEntryNavigation)
                {
                    if (item.ToEntry == infor.StaffId && item.Name == infor.Name && item.PositionOfficial == infor.PositionOfficial && item.Modifier == infor.Modifier)
                    {
                        if (infor.IsDelete == true)
                        {
                            entry.EntryStaffFromEntryNavigation.Remove(item);
                        }
                        else
                        {
                            item.SubordinateOrganization = infor.SubordinateOrganization;
                            item.CustomName = infor.CustomName;
                            item.PositionGeneral = infor.PositionGeneral;
                        }
                        isSame = true;
                        break;

                    }
                }
                if (isSame == false)
                {
                    var entryNew = await _entryRepository.FirstOrDefaultAsync(s => s.Id == infor.StaffId);
                    entry.EntryStaffFromEntryNavigation.Add(new EntryStaff
                    {
                        Modifier = infor.Modifier,
                        FromEntry = entry.Id,
                        FromEntryNavigation = entry,
                        ToEntry = infor.StaffId,
                        ToEntryNavigation = entryNew,
                        SubordinateOrganization = infor.SubordinateOrganization,
                        CustomName = infor.CustomName,
                        Name = infor.Name,
                        PositionGeneral = infor.PositionGeneral,
                        PositionOfficial = infor.PositionOfficial,
                    });
                }
            }
            //处理游戏发行列表
            foreach (var infor in examine.Releases)
            {
                var isSame = false;
                foreach (var item in entry.Releases)
                {
                    if (item.PublishPlatformType == infor.PublishPlatformType && item.PublishPlatformName == infor.PublishPlatformName && item.Link == infor.Link && item.Name == infor.Name)
                    {
                        if (infor.IsDelete == true)
                        {
                            entry.Releases.Remove(item);
                        }
                        else
                        {
                            item.Type = infor.Type;
                            item.Time = infor.Time;
                            item.TimeNote = infor.TimeNote;
                            item.Engine = infor.Engine;
                            item.GamePlatformTypes = infor.GamePlatformTypes;
                        }
                        isSame = true;
                        break;

                    }
                }
                if (isSame == false)
                {
                    entry.Releases.Add(new GameRelease
                    {
                        PublishPlatformType = infor.PublishPlatformType,
                        PublishPlatformName = infor.PublishPlatformName,
                        Link = infor.Link,
                        Name = infor.Name,
                        Type = infor.Type,
                        Time = infor.Time,
                        TimeNote = infor.TimeNote,
                        Engine = infor.Engine,
                        GamePlatformTypes = infor.GamePlatformTypes,
                    });
                }
            }
            //提取发行时间
            var release = entry.Releases.OrderBy(s=>s.Time).FirstOrDefault(s =>s.Time!=null&& s.Type == GameReleaseType.Official);
            if (release != null)
            {
                entry.PubulishTime = release.Time;
            }
            else
            {
                entry.PubulishTime = null;
            }
            //预约
            if (examine.Booking.Goals.Any() || examine.Booking.MainInfor.Any())
            {
                if (entry.Booking == null)
                {
                    entry.Booking = new Booking();
                }

                var goals = entry.Booking.Goals;

                //更新主要信息
                ToolHelper.ModifyDataAccordingToEditingRecord(entry.Booking, examine.Booking.MainInfor);
                //更新目标
                foreach (var item in examine.Booking.Goals)
                {
                    var isAdd = false;

                    //遍历信息列表寻找关键词
                    foreach (var infor in goals)
                    {

                        if (infor.Name == item.Name)
                        {
                            //查看是否为删除操作
                            if (item.IsDelete == true)
                            {
                                goals.Remove(infor);
                                isAdd = true;
                                break;
                            }
                            else
                            {
                                infor.Target = item.Target;
                                isAdd = true;
                                break;
                            }
                        }
                    }
                    if (isAdd == false && item.IsDelete == false)
                    {
                        //没有找到关键词 则新建关键词
                        var temp = new BookingGoal
                        {
                            Name = item.Name,
                            Target = item.Target,
                        };
                        goals.Add(temp);
                    }
                }

            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateEntryDataImages(Entry entry, EntryImages examine)
        {
            //序列化图片列表
            //先读取词条信息
            var pictures = entry.Pictures;

            foreach (var item in examine.Images)
            {
                var isAdd = false;
                foreach (var pic in pictures)
                {
                    if (pic.Url == item.Url)
                    {
                        if (item.IsDelete == true)
                        {
                            pictures.Remove(pic);

                        }
                        else
                        {
                            pic.Modifier = item.Modifier;
                            pic.Note = item.Note;
                            pic.Priority = item.Priority;
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    pictures.Add(new EntryPicture
                    {
                        Url = item.Url,
                        Note = item.Note,
                        Modifier = item.Modifier,
                        Priority = item.Priority
                    });
                }
            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateEntryDataAudio(Entry entry, EntryAudioExamineModel examine)
        {
            //先读取词条信息

            foreach (var item in examine.Audio)
            {
                var isAdd = false;
                foreach (var infor in entry.Audio)
                {
                    if (infor.Url == item.Url)
                    {
                        if (item.IsDelete == true)
                        {
                            entry.Audio.Remove(infor);

                        }
                        else
                        {
                            infor.BriefIntroduction = item.BriefIntroduction;
                            infor.Name = item.Name;
                            infor.Priority = item.Priority;
                            infor.Duration = item.Duration;
                            infor.Thumbnail = item.Thumbnail;
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    entry.Audio.Add(new EntryAudio
                    {
                        Url = item.Url,
                        BriefIntroduction = item.BriefIntroduction,
                        Name = item.Name,
                        Priority = item.Priority,
                        Duration = item.Duration,
                        Thumbnail = item.Thumbnail
                    });
                }
            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public async Task UpdateEntryDataRelevances(Entry entry, EntryRelevances examine)
        {
            UpdateEntryDataOutlinks(entry, examine);
            await UpdateEntryDataRelatedEntriesAsync(entry, examine);
            await UpdateEntryDataRelatedArticles(entry, examine);
            await UpdateEntryDataRelatedVideos(entry, examine);
        }


        public void UpdateEntryDataOutlinks(Entry entry, EntryRelevances examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = entry.Outlinks;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Outlink))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Name == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                            isAdd = true;
                            break;
                        }
                        else
                        {
                            infor.BriefIntroduction = item.DisplayValue;
                            infor.Name = item.DisplayName;
                            infor.Link = item.Link;
                            isAdd = true;
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new Outlink
                    {
                        Name = item.DisplayName,
                        BriefIntroduction = item.DisplayValue,
                        Link = item.Link
                    };
                    relevances.Add(temp);
                }
            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public async Task UpdateEntryDataRelatedEntriesAsync(Entry entry, EntryRelevances examine)
        {

            //序列化相关性列表 From
            //先读取周边信息
            var relevances = entry.EntryRelationFromEntryNavigation;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Entry))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.ToEntry.ToString() == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    var entryNew = await _entryRepository.FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                    if (entryNew != null)
                    {
                        relevances.Add(new EntryRelation
                        {
                            FromEntry = entry.Id,
                            FromEntryNavigation = entry,
                            ToEntry = entryNew.Id,
                            ToEntryNavigation = entryNew
                        });
                    }
                }
            }
            entry.EntryRelationFromEntryNavigation = relevances;



            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateEntryDataRelatedArticles(Entry entry, EntryRelevances examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = entry.Articles;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Article))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Id.ToString() == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);

                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                    if (article != null)
                    {
                        relevances.Add(article);
                    }

                }
            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task UpdateEntryDataRelatedVideos(Entry entry, EntryRelevances examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = entry.Videos;

            foreach (var item in examine.Relevances.Where(s => s.Type == RelevancesType.Video))
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Id.ToString() == item.DisplayName)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);

                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var video = await _videoRepository.FirstOrDefaultAsync(s => s.Id.ToString() == item.DisplayName);
                    if (video != null)
                    {
                        relevances.Add(video);
                    }

                }
            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();
        }


        public async Task UpdateEntryDataTagsAsync(Entry entry, EntryTags examine)
        {

            //序列化相关性列表
            //先读取词条信息
            var relevances = entry.Tags;

            foreach (var item in examine.Tags)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.Id == item.TagId)
                    {
                        //查看是否为删除操作
                        if (item.IsDelete == true)
                        {
                            relevances.Remove(infor);
                            isAdd = true;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //查找Tag
                    var tagNew = await _tagRepository.FirstOrDefaultAsync(s => s.Id == item.TagId);
                    if (tagNew != null)
                    {
                        relevances.Add(tagNew);
                    }
                }
            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateEntryDataMainPage(Entry entry, string examine)
        {
            entry.MainPage = examine;

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateEntryDataWebsiteImages(Entry entry, EntryWebsiteExamineModel examine)
        {
            if (entry.WebsiteAddInfor == null)
            {
                entry.WebsiteAddInfor = new EntryWebsite();
            }
            var pictures = entry.WebsiteAddInfor.Images;

            foreach (var item in examine.Images)
            {
                var isAdd = false;
                foreach (var pic in pictures)
                {
                    if (pic.Url == item.Url&&pic.Type== item.Type)
                    {
                        if (item.IsDelete == true)
                        {
                            pictures.Remove(pic);

                        }
                        else
                        {
                            pic.Size = item.Size;
                            pic.Priority = item.Priority;
                            pic.Note = item.Note;
                        }
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    pictures.Add(new EntryWebsiteImage
                    {
                        Url = item.Url,
                        Type = item.Type,
                        Size = item.Size,
                        Note = item.Note,
                        Priority = item.Priority,
                    });
                }
            }

            //更新最后编辑时间
            entry.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateEntryDataWebsite(Entry entry, EntryWebsiteExamineModel examine)
        {
            UpdateEntryDataWebsiteImages(entry, examine);
            ToolHelper.ModifyDataAccordingToEditingRecord(entry.WebsiteAddInfor, examine.MainInfor);
        }


        public async Task UpdateEntryDataAsync(Entry entry, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.EstablishMain:
                    ExamineMain examineMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        examineMain = (ExamineMain)serializer.Deserialize(str, typeof(ExamineMain));
                    }

                    UpdateEntryDataMain(entry, examineMain);
                    break;
                case Operation.EstablishAddInfor:
                    EntryAddInfor entryAddInfor = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryAddInfor = (EntryAddInfor)serializer.Deserialize(str, typeof(EntryAddInfor));
                    }

                    await UpdateEntryDataAddInforAsync(entry, entryAddInfor);
                    break;
                case Operation.EstablishImages:
                    EntryImages entryImages = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryImages = (EntryImages)serializer.Deserialize(str, typeof(EntryImages));
                    }

                    UpdateEntryDataImages(entry, entryImages);
                    break;
                case Operation.EstablishRelevances:
                    EntryRelevances entryRelevances = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryRelevances = (EntryRelevances)serializer.Deserialize(str, typeof(EntryRelevances));
                    }

                    await UpdateEntryDataRelevances(entry, entryRelevances);
                    break;
                case Operation.EstablishTags:
                    EntryTags entryTags = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryTags = (EntryTags)serializer.Deserialize(str, typeof(EntryTags));
                    }

                    await UpdateEntryDataTagsAsync(entry, entryTags);
                    break;
                case Operation.EstablishMainPage:
                    var mainPage = examine.Context;
                    UpdateEntryDataMainPage(entry, mainPage);
                    break;
                case Operation.EstablishAudio:
                    EntryAudioExamineModel entryAudioExamineModel = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryAudioExamineModel = (EntryAudioExamineModel)serializer.Deserialize(str, typeof(EntryAudioExamineModel));
                    }

                    UpdateEntryDataAudio(entry, entryAudioExamineModel);
                    break;
                case Operation.EstablishWebsite:
                    EntryWebsiteExamineModel entryWebsiteExamineModel = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryWebsiteExamineModel = (EntryWebsiteExamineModel)serializer.Deserialize(str, typeof(EntryWebsiteExamineModel));
                    }

                    UpdateEntryDataWebsite(entry, entryWebsiteExamineModel);
                    break;
                default:
                    throw new InvalidOperationException("不支持的操作");
            }
        }

        /// <summary>
        /// 获取编辑状态
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entryId"></param>
        /// <returns></returns>
        public async Task<EntryEditState> GetEntryEditState(ApplicationUser user, int entryId)
        {
            var model = new EntryEditState();
            //获取该词条的各部分编辑状态
            //读取审核信息
            List<Examine> examineQuery = null;
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.EntryId == entryId && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EstablishMain || s.Operation == Operation.EstablishMainPage || s.Operation == Operation.EstablishAddInfor || s.Operation == Operation.EstablishImages
                               || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags || s.Operation == Operation.EstablishAudio || s.Operation == Operation.EstablishWebsite))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation,
                                   Context = s.Context
                               })
                               .ToListAsync();
            }

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EstablishMain))
                {
                    model.MainState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishMainPage))
                {
                    model.MainPageState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishAddInfor))
                {
                    model.InforState = EditState.Preview;
                }

                if (examineQuery.Any(s => s.Operation == Operation.EstablishImages))
                {
                    model.ImagesState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishRelevances))
                {
                    model.RelevancesState = EditState.Preview;
                }

                if (examineQuery.Any(s => s.Operation == Operation.EstablishTags))
                {
                    model.TagState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishAudio))
                {
                    model.AudioState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EstablishWebsite))
                {
                    model.WebsiteState = EditState.Preview;
                }
            }
            //获取各部分状态
            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.EntryId == entryId && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishMain))
                    {
                        model.MainState = EditState.Locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.InforState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishAddInfor))
                    {
                        model.InforState = EditState.Locked;
                    }
                    else
                    {
                        model.InforState = EditState.Normal;
                    }
                }
                if (model.MainPageState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishMainPage))
                    {
                        model.MainPageState = EditState.Locked;
                    }
                    else
                    {
                        model.MainPageState = EditState.Normal;
                    }
                }
                if (model.ImagesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishImages))
                    {
                        model.ImagesState = EditState.Locked;
                    }
                    else
                    {
                        model.ImagesState = EditState.Normal;
                    }
                }
                if (model.RelevancesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EstablishRelevances))
                    {
                        model.RelevancesState = EditState.Locked;
                    }
                    else
                    {
                        model.RelevancesState = EditState.Normal;
                    }
                }
                if (model.TagState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishTags))
                    {
                        model.TagState = EditState.Locked;
                    }
                    else
                    {
                        model.TagState = EditState.Normal;
                    }
                }
                if (model.AudioState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishAudio))
                    {
                        model.AudioState = EditState.Locked;
                    }
                    else
                    {
                        model.AudioState = EditState.Normal;
                    }
                }
                if (model.WebsiteState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EstablishAudio))
                    {
                        model.WebsiteState = EditState.Locked;
                    }
                    else
                    {
                        model.WebsiteState = EditState.Normal;
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// 获取视图模型数据
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task<EntryIndexViewModel> GetEntryIndexViewModelAsync(Entry entry)
        {
            //建立视图模型
            var model = new EntryIndexViewModel
            {
                Id = entry.Id,
                Name = entry.DisplayName ?? entry.Name,
                BriefIntroduction = entry.BriefIntroduction,
                Type = entry.Type,
                CanComment = entry.CanComment ?? true,
                AnotherName = entry.AnotherName,
                IsHidden = entry.IsHidden,
                IsHideOutlink = entry.IsHideOutlink,
                Template=entry.Template
            };

            //查看是否有配音
            if (entry.Tags != null && entry.Tags.Any(s => s.Name == "无配音"))
            {
                model.IsDubbing = false;
            }
            else
            {
                model.IsDubbing = true;
            }

            //初始化图片链接
            model.MainPicture = _appHelper.GetImagePath(entry.MainPicture, (entry.Type == EntryType.Staff || entry.Type == EntryType.Role) ? "" : "app.png");
            model.BackgroundPicture = _appHelper.GetImagePath(entry.BackgroundPicture, "");
            model.Thumbnail = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
            model.SmallBackgroundPicture = _appHelper.GetImagePath(entry.SmallBackgroundPicture, "");


            //初始化主页Html代码
            model.MainPage = _appHelper.MarkdownToHtml(entry.MainPage ?? "");

            //读取词条信息
            //添加别称到附加信息
            if (string.IsNullOrWhiteSpace(entry.AnotherName) == false)
            {
                model.Information.Add(new InformationsModel
                {
                    Informations=new List<KeyValueModel>
                    {
                        new KeyValueModel
                        {
                            DisplayName="别称",
                            DisplayValue=model.AnotherName,
                        }
                    },
                    Modifier = "基本信息"
                });
            }

            //添加角色CV
            if (model.Type == EntryType.Role)
            {
                var cvs = new StringBuilder();
                foreach (var item in entry.EntryStaffFromEntryNavigation.Where(s => s.PositionGeneral == PositionGeneralType.CV))
                {
                    if (cvs.Length > 0)
                    {
                        cvs.Append("、");
                    }
                    cvs.Append(string.IsNullOrWhiteSpace(item.CustomName) ? (item.ToEntryNavigation?.Name ?? item.Name) : item.CustomName);
                }
                if (cvs.Length > 0)
                {
                    model.Information.Add(new InformationsModel
                    {
                        Modifier = "基本信息",
                        Informations = new List<KeyValueModel>
                        {
                            new KeyValueModel()
                            {
                                DisplayName = "声优",
                                DisplayValue = cvs.ToString()
                            }

                        }
                    });
                }

            }

            foreach (var item in entry.Information)
            {
                //判断
                if (item.DisplayName == "性别")
                {
                    if (Enum.TryParse(typeof(GenderType), item.DisplayValue, true, out object gender))
                    {
                        item.DisplayValue = ((GenderType)gender).GetDisplayName();
                    }
                }
                else if (item.Modifier=="相关网站"|| item.DisplayName == "Steam平台Id"|| item.DisplayName == "昵称（官方称呼）"
                    || item.DisplayName == "发行时间" || item.DisplayName == "游戏平台"
                    || item.DisplayName == "引擎" || item.DisplayName == "发行方式")
                {
                    continue;
                }

                var isAdd = false;
                //如果信息值为空 则不显示
                if (string.IsNullOrWhiteSpace(item?.DisplayValue) == true)
                {
                    continue;
                }
                //遍历信息列表寻找关键词
                foreach (var infor in model.Information)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        //关键词相同则添加
                        infor.Informations.Add(new KeyValueModel
                        {
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue
                        });
                        isAdd = true;
                        break;
                    }
                }
                if (isAdd == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new InformationsModel
                    {
                        Modifier = item.Modifier,
                        Informations = new List<KeyValueModel>()
                    };
                    temp.Informations.Add(new KeyValueModel
                    {
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue
                    });
                    model.Information.Add(temp);
                }
            }

            //添加发行列表
           foreach(var item in entry.Releases)
            {
                var infor = new GameReleaseViewModel
                {
                    Engine = item.Engine,
                    GamePlatformTypes = item.GamePlatformTypes,
                    Link = item.Link,
                    Name = item.Name,
                    PublishPlatformName = item.PublishPlatformName,
                    PublishPlatformType = item.PublishPlatformType,
                    Time = item.Time,
                    TimeNote = item.TimeNote,
                    Type = item.Type,
                };
                if(item.PublishPlatformType== PublishPlatformType.Steam&&int.TryParse(item.Link,out int steamId))
                {
                    infor.StoreInfor = await _steamInforService.GetSteamInforAsync(steamId, entry.Id);
                }
                model.Releases.Add(infor);
            }

            //序列化 STAFF
            //先读取词条信息
            if (model.Type == EntryType.Game)
            {
                model.Staffs = new List<StaffInforModel>
                {
                    new StaffInforModel
                    {
                        Modifier =null,
                        StaffList = new List<StaffValue>()
                    }
                };
                foreach (var item in entry.EntryStaffFromEntryNavigation.OrderBy(s => s.PositionGeneral).ThenBy(s => s.PositionOfficial))
                {

                    var isAdd = false;

                    //尝试获取staff的显示名称
                    var displayName = string.IsNullOrWhiteSpace(item.CustomName) ? (item.ToEntryNavigation?.DisplayName ?? item.Name) : item.CustomName;
                    var mainModifier = string.IsNullOrWhiteSpace(item.Modifier) ? null : item.Modifier;
                    var secordModifier = item.PositionOfficial;
                    var staffId = (item.ToEntryNavigation?.IsHidden == false && string.IsNullOrWhiteSpace(item.ToEntryNavigation.Name) == false) ? item.ToEntry ?? 0 : 0;

                    //检测是否为制作组发行商
                    if (item.PositionGeneral == PositionGeneralType.ProductionGroup)
                    {
                        model.ProductionGroups.Add(new StaffNameModel
                        {
                            DisplayName = displayName,
                            Id = staffId
                        });
                        continue;
                    }
                    else if (item.PositionGeneral == PositionGeneralType.Publisher)
                    {
                        model.Publishers.Add(new StaffNameModel
                        {
                            DisplayName = displayName,
                            Id = staffId
                        });
                        continue;
                    }

                    //遍历信息列表寻找 主关键词
                    foreach (var infor in model.Staffs)
                    {
                        if (infor.Modifier == mainModifier)
                        {
                            //寻找次要关键词
                            foreach (var temp in infor.StaffList)
                            {
                                if (temp.Modifier == secordModifier)
                                {
                                    //关键词相同则添加
                                    temp.Names.Add(new StaffNameModel
                                    {
                                        DisplayName = displayName,
                                        Id = staffId,
                                    });
                                    isAdd = true;
                                    break;
                                }
                            }
                            //没有找到次要关键词 则新建次要关键词
                            if (isAdd == false)
                            {
                                //没有找到关键词 则新建关键词
                                var temp = new StaffValue
                                {
                                    Modifier = secordModifier,
                                    Names = new List<StaffNameModel>()
                                };
                                temp.Names.Add(new StaffNameModel
                                {
                                    DisplayName = displayName,
                                    Id = staffId,
                                });
                                infor.StaffList.Add(temp);
                                isAdd = true;
                            }
                            break;
                        }
                    }
                    if (isAdd == false)
                    {
                        //没有找到主关键词 则新建关键词
                        var temp = new StaffInforModel
                        {
                            Modifier = mainModifier,
                            StaffList = new List<StaffValue>()
                        };
                        temp.StaffList.Add(new StaffValue
                        {
                            Modifier = secordModifier,
                            Names = new List<StaffNameModel>()
                        });
                        temp.StaffList[0].Names.Add(new StaffNameModel
                        {
                            DisplayName = displayName,
                            Id = staffId,
                        });
                        model.Staffs.Add(temp);
                    }
                }


                //如果所有staff都有分组 则删除默认空分组
                if (model.Staffs[0].StaffList.Count == 0)
                {
                    model.Staffs.RemoveAt(0);
                }

            }

            //预约信息
            if (entry.Type == EntryType.Game && entry.Booking != null && (entry.PubulishTime == null || entry.PubulishTime.Value.Date > DateTime.Now.ToCstTime()))
            {
                model.Booking = new BookingViewModel
                {
                    BookingCount = entry.Booking.BookingCount,
                    Open = entry.Booking.Open,
                    Goals = entry.Booking.Goals.Select(s => new BookingGoalViewModel
                    {
                        Name = s.Name,
                        Target = s.Target
                    }).ToList()
                };
            }

            //序列化图片列表

            //读取词条信息
            var pictures = new List<EntryPicture>();
            foreach (var item in entry.Pictures.OrderByDescending(s => s.Priority))
            {
                pictures.Add(new EntryPicture
                {
                    Url = item.Url,
                    Note = item.Note,
                    Modifier = item.Modifier
                });
            }

            //根据分类来重新排列图片
            var picturesViewModels = new List<PicturesViewModel>
            {
                new PicturesViewModel
                {
                    Modifier=null,
                    Pictures=new List<PicturesAloneViewModel>()
                }
            };

            foreach (var item in pictures)
            {
                var isAdd = false;
                foreach (var infor in picturesViewModels)
                {
                    if (infor.Modifier == item.Modifier)
                    {
                        infor.Pictures.Add(new PicturesAloneViewModel
                        {
                            Note = item.Note,
                            Url = _appHelper.GetImagePath(item.Url, "")
                        });
                        isAdd = true;
                        break;
                    }
                }

                if (isAdd == false)
                {
                    picturesViewModels.Add(new PicturesViewModel
                    {
                        Modifier = item.Modifier,
                        Pictures = new List<PicturesAloneViewModel> {
                            new PicturesAloneViewModel
                            {
                                Note=item.Note,
                                Url=_appHelper.GetImagePath(item.Url, "")
                            }
                        }
                    });
                }
            }

            //如果所有图片都有分组 则删除默认空分组
            if (picturesViewModels[0].Pictures.Count == 0)
            {
                picturesViewModels.RemoveAt(0);
            }

            //读取音频信息
            var audioImage = entry.Audio.OrderByDescending(s => s.Priority).FirstOrDefault(s => string.IsNullOrWhiteSpace(s.Thumbnail) == false)?.Thumbnail;
            model.Audio.AddRange(entry.Audio.OrderByDescending(s => s.Priority).Select(s => new AudioViewModel
            {
                BriefIntroduction = s.BriefIntroduction,
                Name = s.Name,
                Priority = s.Priority,
                Url = s.Url,
                Duration = s.Duration,
                Thumbnail = _appHelper.GetImagePath(string.IsNullOrWhiteSpace(s.Thumbnail) ? audioImage : s.Thumbnail, "AudioThumbnail.png")
            }).ToList());


            //序列化标签列表

            //读取词条信息
            var tags = new List<TagsViewModel>();
            foreach (var item in entry.Tags)
            {
                tags.Add(new TagsViewModel { Name = item.Name, Id = item.Id });
            }

            //序列化相关性列表
            //加载附加信息 关联词条获取
            var roleInforModel = new List<EntryRoleViewModel>();
            var newsModel = new List<NewsModel>();
            var staffGames = new List<EntryInforTipViewModel>();
            var relevancesEntry = new List<EntryInforTipViewModel>();
            var relevanceArticle = new List<ArticleInforTipViewModel>();
            var relevanceOther = new List<RelevancesKeyValueModel>();

            //文章
            foreach (var item in entry.Articles.Where(s => s.IsHidden == false))
            {
                if (item.Type == ArticleType.News)
                {
                    newsModel.Add(await _articleService.GetNewsModelAsync(item));
                }
                else
                {
                    relevanceArticle.Add(_appHelper.GetArticleInforTipViewModel(item));

                }
            }
            //视频
            foreach (var item in entry.Videos.Where(s => s.IsHidden == false))
            {

                model.VideoRelevances.Add(_appHelper.GetVideoInforTipViewModel(item));


            }
            //词条
            foreach (var nav in entry.EntryRelationFromEntryNavigation.Where(s => s.ToEntryNavigation.IsHidden == false))
            {
                var item = nav.ToEntryNavigation;
                if (item.Type == EntryType.Role)
                {
                    if (entry.Type != EntryType.Game)
                    {
                        var role = _appHelper.GetEntryInforTipViewModel(item);
                        if (entry.Type == EntryType.Staff)
                        {
                            role.AddInfors.RemoveAll(s => s.Modifier == "配音");
                        }
                        relevancesEntry.Add(role);
                    }
                    else
                    {
                        //获取角色词条
                        roleInforModel.Add(GetRoleInfor(item));
                    }

                }
                else if (item.Type == EntryType.Game)
                {
                    if (entry.Type == EntryType.Staff || entry.Type == EntryType.ProductionGroup)
                    {
                        var staffGame = _appHelper.GetEntryInforTipViewModel(item);
                        staffGame.AddInfors.Clear();
                        //查找担任过的职位
                        var tempStaffs = item.EntryStaffFromEntryNavigation.Where(s => s.ToEntry == entry.Id);
                        if (tempStaffs.Any())
                        {

                            staffGame.AddInfors.Add(new EntryInforTipAddInforModel
                            {
                                Modifier = "职位",
                                Contents = tempStaffs.Select(s => new StaffNameModel
                                {
                                    DisplayName = (string.IsNullOrWhiteSpace(s.Modifier) ? "" : s.Modifier + " - ") + (s.PositionOfficial ?? s.PositionGeneral.GetDisplayName()),
                                    Id = -1
                                }).ToList()
                            });
                        }

                        staffGames.Add(staffGame);

                    }
                    else
                    {
                        relevancesEntry.Add(_appHelper.GetEntryInforTipViewModel(item));
                    }
                }
                else if (item.Type == EntryType.Staff)
                {
                    if (entry.Type != EntryType.Game)
                    {
                        relevancesEntry.Add(_appHelper.GetEntryInforTipViewModel(item));
                    }
                }
                else if (item.Type == EntryType.ProductionGroup)
                {
                    relevancesEntry.Add(_appHelper.GetEntryInforTipViewModel(item));
                }
            }

            foreach (var item in entry.Outlinks)
            {
                relevanceOther.Add(new RelevancesKeyValueModel
                {
                    DisplayName = item.Name,
                    DisplayValue = item.BriefIntroduction,
                    Link = item.Link,
                });
            }

            //官网补充信息
            if(entry.WebsiteAddInfor!=null)
            {
                model.WebsiteAddInfor = new EntryWebsiteViewModel
                {
                    Images = entry.WebsiteAddInfor.Images.Select(s => new EntryWebsiteImageViewModel
                    {
                        Note = s.Note,
                        Priority = s.Priority,
                        Type = s.Type,
                        Url = s.Url,
                        Size=s.Size
                    }).ToList(),
                    Html = entry.WebsiteAddInfor.Html,
                    Introduction = entry.WebsiteAddInfor.Introduction,
                    SubTitle = entry.WebsiteAddInfor.SubTitle,
                    Color = entry.WebsiteAddInfor.Color,
                    FirstPage = entry.WebsiteAddInfor.FirstPage,
                    Impressions = entry.WebsiteAddInfor.Impressions,
                    Logo = entry.WebsiteAddInfor.Logo,
                };
            }

            //赋值
            model.Pictures = picturesViewModels;
            model.ArticleRelevances = relevanceArticle;
            model.EntryRelevances = relevancesEntry;
            model.OtherRelevances = relevanceOther;
            model.Tags = tags;
            model.Roles = roleInforModel;
            model.StaffGames = staffGames;
            model.NewsOfEntry = newsModel;


            //获取是否评分
            if (model.Type == EntryType.Game)
            {
                model.IsScored = await _playedGameRepository.GetAll().AnyAsync(s => s.EntryId == model.Id && s.ShowPublicly && s.MusicSocre != 0 && s.ShowSocre != 0 && s.TotalSocre != 0 && s.PaintSocre != 0 && s.ScriptSocre != 0);
            }

            return model;
        }

        /// <summary>
        /// 对比新旧词条生成编辑记录
        /// </summary>
        /// <param name="currentEntry"></param>
        /// <param name="newEntry"></param>
        /// <returns></returns>
        public List<KeyValuePair<object, Operation>> ExaminesCompletion(Entry currentEntry, Entry newEntry)
        {
            var examines = new List<KeyValuePair<object, Operation>>();
            //第一部分 主要信息

            //添加修改记录
            //新建审核数据对象
            var examineMain = new ExamineMain
            {
                Items = ToolHelper.GetEditingRecordFromContrastData(currentEntry, newEntry)
            };
            examineMain.Items.RemoveAll(s => s.Key == "PubulishTime");
            if (examineMain.Items.Count > 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(examineMain, Operation.EstablishMain));

            }


            //第二部分 附加信息
            var entryAddInfor = new EntryAddInfor();

            //先将所有信息打上删除标签
            foreach (var item in currentEntry.Information)
            {
                var additional_s = new List<BasicEntryInformationAdditional_>();
                foreach (var temp in item.Additional)
                {
                    additional_s.Add(new BasicEntryInformationAdditional_ { DisplayName = temp.DisplayName, DisplayValue = temp.DisplayValue, IsDelete = true });
                }
                entryAddInfor.Information.Add(new BasicEntryInformation_ { Modifier = item.Modifier, DisplayName = item.DisplayName, DisplayValue = item.DisplayValue, IsDelete = true, Additional = additional_s });
            }
            //再对比当前
            foreach (var item in newEntry.Information.ToList().Purge())
            {
                var isSame = false;
                foreach (var infor in entryAddInfor.Information)
                {
                    if (item.DisplayName == infor.DisplayName && item.DisplayValue == infor.DisplayValue && item.Modifier == infor.Modifier)
                    {
                        isSame = true;
                        //如果两次一致 删除上一步中的项目
                        foreach (var temp1 in item.Additional.ToList().Purge())
                        {
                            var isSameIn = false;
                            foreach (var temp in infor.Additional)
                            {
                                if (temp.DisplayName == temp1.DisplayName)
                                {
                                    if (temp.DisplayValue == temp1.DisplayValue)
                                    {
                                        infor.Additional.Remove(temp);

                                    }
                                    else
                                    {
                                        temp.DisplayValue = temp1.DisplayValue;
                                        temp.IsDelete = false;
                                    }
                                    isSameIn = true;
                                    break;
                                }
                            }
                            if (isSameIn == false)
                            {
                                infor.Additional.Add(new BasicEntryInformationAdditional_
                                {
                                    DisplayName = temp1.DisplayName,
                                    DisplayValue = temp1.DisplayValue,
                                    IsDelete = false,
                                });
                            }
                        }
                        if (infor.Additional.Any() == false)
                        {
                            entryAddInfor.Information.Remove(infor);
                        }
                        else
                        {
                            infor.IsDelete = false;
                        }

                        break;
                    }
                }
                if (isSame == false)
                {
                    var staffs = new List<BasicEntryInformationAdditional_>();
                    entryAddInfor.Information.Add(new BasicEntryInformation_
                    {
                        Modifier = item.Modifier,
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        Additional = item.Additional.Select(s => new BasicEntryInformationAdditional_
                        {
                            DisplayName = s.DisplayName,
                            DisplayValue = s.DisplayValue,
                            IsDelete = false,
                        }).ToList(),
                        IsDelete = false
                    });
                }
            }

            //预约
            if (newEntry.Booking != null)
            {
                if (currentEntry.Booking == null)
                {
                    currentEntry.Booking = new Booking();
                }

                //主要信息
                entryAddInfor.Booking.MainInfor = ToolHelper.GetEditingRecordFromContrastData(currentEntry.Booking, newEntry.Booking);
                //目标
                //先把 当前预约中的目标 都 打上删除标签
                foreach (var item in currentEntry.Booking.Goals)
                {
                    entryAddInfor.Booking.Goals.Add(new EditBookingGoal
                    {
                        Name = item.Name,
                        Target = item.Target,
                        IsDelete = true
                    });
                }
                //再对比当前
                foreach (var infor in newEntry.Booking.Goals.ToList().Purge())
                {
                    var isSame = false;
                    foreach (var item in entryAddInfor.Booking.Goals)
                    {
                        if (item.Name == infor.Name)
                        {
                            if (item.Target != infor.Target)
                            {
                                item.Target = infor.Target;
                                item.IsDelete = false;
                            }
                            else
                            {
                                entryAddInfor.Booking.Goals.Remove(item);
                            }
                            isSame = true;
                            break;

                        }
                    }
                    if (isSame == false)
                    {
                        entryAddInfor.Booking.Goals.Add(new EditBookingGoal
                        {
                            Target = infor.Target,
                            Name=infor.Name,
                            IsDelete = false
                        });
                    }
                }
            }



            //Staff
            //遍历当前词条数据 打上删除标签
            foreach (var item in currentEntry.EntryStaffFromEntryNavigation)
            {
                entryAddInfor.Staffs.Add(new EntryStaffExamineModel
                {
                    Modifier = item.Modifier,
                    StaffId = item.ToEntry,
                    SubordinateOrganization = item.SubordinateOrganization,
                    CustomName = item.CustomName,
                    Name = item.Name,
                    PositionGeneral = item.PositionGeneral,
                    PositionOfficial = item.PositionOfficial,
                    IsDelete = true,
                });
            }


            //再遍历视图 对应修改

            foreach (var infor in newEntry.EntryStaffFromEntryNavigation.ToList().Purge())
            {
                var isSame = false;
                foreach (var item in entryAddInfor.Staffs)
                {
                    if (item.StaffId == infor.ToEntry && item.Name == infor.Name && item.PositionOfficial == infor.PositionOfficial && item.Modifier == infor.Modifier)
                    {
                        if (item.SubordinateOrganization != infor.SubordinateOrganization || item.CustomName != infor.CustomName || item.PositionGeneral != infor.PositionGeneral)
                        {
                            item.SubordinateOrganization = infor.SubordinateOrganization;
                            item.CustomName = infor.CustomName;
                            item.PositionGeneral = infor.PositionGeneral;
                            item.IsDelete = false;
                        }
                        else
                        {
                            entryAddInfor.Staffs.Remove(item);
                        }
                        isSame = true;
                        break;

                    }
                }
                if (isSame == false)
                {
                    entryAddInfor.Staffs.Add(new EntryStaffExamineModel
                    {
                        Modifier = infor.Modifier,
                        StaffId = infor.ToEntry,
                        SubordinateOrganization = infor.SubordinateOrganization,
                        CustomName = infor.CustomName,
                        Name = infor.Name,
                        PositionGeneral = infor.PositionGeneral,
                        PositionOfficial = infor.PositionOfficial,
                        IsDelete = false
                    });
                }
            }

            //发行列表
            //遍历当前词条数据 打上删除标签
            foreach (var item in currentEntry.Releases)
            {
                entryAddInfor.Releases.Add(new GameReleaseExamineModel
                {
                    Engine = item.Engine,
                    Link = item.Link,
                    Name = item.Name,
                    GamePlatformTypes = item.GamePlatformTypes.ToArray(),
                    PublishPlatformName = item.PublishPlatformName,
                    Time = item.Time,
                    TimeNote = item.TimeNote,
                    Type = item.Type,
                    PublishPlatformType = item.PublishPlatformType,
                    IsDelete = true,
                });
            }


            //再遍历视图 对应修改

            foreach (var infor in newEntry.Releases.ToList().Purge())
            {
                var isSame = false;
                foreach (var item in entryAddInfor.Releases)
                {
                    if (item.PublishPlatformName == infor.PublishPlatformName && item.PublishPlatformType == infor.PublishPlatformType && item.Link == infor.Link && item.Name == infor.Name)
                    {
                        if ( item.Type != infor.Type || item.Time != infor.Time || item.TimeNote != infor.TimeNote || item.Engine != infor.Engine || item.GamePlatformTypes.SequenceEqual(infor.GamePlatformTypes)==false)
                        {
                            item.Type = infor.Type;
                            item.Time = infor.Time;
                            item.TimeNote = infor.TimeNote;
                            item.Engine = infor.Engine;
                            item.GamePlatformTypes = infor.GamePlatformTypes;
                            item.IsDelete = false;
                        }
                        else
                        {
                            entryAddInfor.Releases.Remove(item);
                        }
                        isSame = true;
                        break;

                    }
                }
                if (isSame == false)
                {
                    entryAddInfor.Releases.Add(new GameReleaseExamineModel
                    {
                        Engine = infor.Engine,
                        Link = infor.Link,
                        Name = infor.Name,
                        GamePlatformTypes = infor.GamePlatformTypes,
                        PublishPlatformName = infor.PublishPlatformName,
                        Time = infor.Time,
                        TimeNote = infor.TimeNote,
                        Type = infor.Type,
                        PublishPlatformType = infor.PublishPlatformType,
                        IsDelete = false
                    });
                }
            }

            //检测是否有修改
            if (entryAddInfor.Information.Any() || entryAddInfor.Staffs.Any() || entryAddInfor.Booking.MainInfor.Any() || entryAddInfor.Booking.Goals.Any() || entryAddInfor.Releases.Any())
            {
                examines.Add(new KeyValuePair<object, Operation>(entryAddInfor, Operation.EstablishAddInfor));

            }
            //第三部分 图片
            var entryImages = new EntryImages();
            //先把 当前词条中的图片 都 打上删除标签
            foreach (var item in currentEntry.Pictures)
            {
                entryImages.Images.Add(new EditRecordImage
                {
                    Url = item.Url,
                    Note = item.Note,
                    Priority = item.Priority,
                    Modifier = item.Modifier,
                    IsDelete = true
                });
            }
            //再对比当前
            foreach (var infor in newEntry.Pictures.ToList().Purge())
            {
                var isSame = false;
                foreach (var item in entryImages.Images)
                {
                    if (item.Url == infor.Url)
                    {
                        if (item.Note != infor.Note || item.Modifier != infor.Modifier || item.Priority != infor.Priority)
                        {
                            item.Modifier = infor.Modifier;
                            item.IsDelete = false;
                            item.Note = infor.Note;
                            item.Priority = infor.Priority;
                        }
                        else
                        {
                            entryImages.Images.Remove(item);
                        }
                        isSame = true;
                        break;

                    }
                }
                if (isSame == false)
                {
                    entryImages.Images.Add(new EditRecordImage
                    {
                        Url = infor.Url,
                        Modifier = infor.Modifier,
                        Priority = infor.Priority,
                        Note = infor.Note,
                        IsDelete = false
                    });
                }
            }

            if (entryImages.Images.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(entryImages, Operation.EstablishImages));

            }

            //第四部分 关联信息
            //创建审核数据模型
            var entryRelevances = new EntryRelevances();

            //处理关联词条

            //遍历当前词条数据 打上删除标签
            foreach (var item in currentEntry.EntryRelationFromEntryNavigation.Select(s => s.ToEntryNavigation))
            {
                entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Id.ToString(),
                    DisplayValue = item.Name,
                    Type = RelevancesType.Entry,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newEntry.EntryRelationFromEntryNavigation.ToList().Purge())
            {
                var temp = entryRelevances.Relevances.FirstOrDefault(s => s.Type == RelevancesType.Entry && s.DisplayName == item.ToEntry.ToString());
                if (temp != null)
                {
                    entryRelevances.Relevances.Remove(temp);
                }
                else
                {
                    entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = item.ToEntry.ToString(),
                        DisplayValue = item.ToEntryNavigation.Name,
                        Type = RelevancesType.Entry,
                        IsDelete = false
                    });
                }
            }

            //处理关联文章
            //遍历当前文章数据 打上删除标签
            foreach (var item in currentEntry.Articles)
            {
                entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Id.ToString(),
                    DisplayValue = item.Name,
                    Type = RelevancesType.Article,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newEntry.Articles)
            {
                var temp = entryRelevances.Relevances.FirstOrDefault(s => s.Type == RelevancesType.Article && s.DisplayName == item.Id.ToString());
                if (temp != null)
                {
                    entryRelevances.Relevances.Remove(temp);
                }
                else
                {
                    entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = item.Id.ToString(),
                        DisplayValue = item.Name,
                        Type = RelevancesType.Article,
                        IsDelete = false
                    });
                }
            }

            //处理关联视频
            //遍历当前数据 打上删除标签
            foreach (var item in currentEntry.Videos)
            {
                entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Id.ToString(),
                    DisplayValue = item.Name,
                    Type = RelevancesType.Video,
                    IsDelete = true,
                });
            }

            //再遍历视图 对应修改

            //添加新建项目
            foreach (var item in newEntry.Videos)
            {
                var temp = entryRelevances.Relevances.FirstOrDefault(s => s.Type == RelevancesType.Video && s.DisplayName == item.Id.ToString());
                if (temp != null)
                {
                    entryRelevances.Relevances.Remove(temp);
                }
                else
                {
                    entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = item.Id.ToString(),
                        DisplayValue = item.Name,
                        Type = RelevancesType.Video,
                        IsDelete = false
                    });
                }
            }

            //处理外部链接

            //遍历当前词条外部链接 打上删除标签
            foreach (var item in currentEntry.Outlinks)
            {
                entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                {
                    DisplayName = item.Name,
                    DisplayValue = item.BriefIntroduction,
                    IsDelete = true,
                    Type = RelevancesType.Outlink,
                    Link = item.Link
                });
            }


            //循环查找外部链接是否相同
            foreach (var infor in newEntry.Outlinks.ToList().Purge())
            {
                var isSame = false;
                foreach (var item in entryRelevances.Relevances.Where(s => s.Type == RelevancesType.Outlink))
                {
                    if (item.DisplayName == infor.Name)
                    {
                        if (item.DisplayValue != infor.BriefIntroduction || item.Link != infor.Link)
                        {
                            item.DisplayValue = infor.BriefIntroduction;
                            item.IsDelete = false;
                            item.Link = infor.Link;
                        }
                        else
                        {
                            entryRelevances.Relevances.Remove(item);
                        }
                        isSame = true;

                        break;

                    }
                }
                if (isSame == false)
                {
                    entryRelevances.Relevances.Add(new EditRecordRelevancesAloneModel
                    {
                        DisplayName = infor.Name,
                        DisplayValue = infor.BriefIntroduction,
                        Link = infor.Link,
                        Type = RelevancesType.Outlink,
                        IsDelete = false
                    });
                }
            }

            if (entryRelevances.Relevances.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(entryRelevances, Operation.EstablishRelevances));
            }

            //第五部分 主页
            if (newEntry.MainPage != currentEntry.MainPage)
            {
                if (string.IsNullOrWhiteSpace(newEntry.MainPage) && string.IsNullOrWhiteSpace(currentEntry.MainPage))
                {

                }
                else
                {
                    //序列化
                    var resulte = newEntry.MainPage;
                    examines.Add(new KeyValuePair<object, Operation>(resulte, Operation.EstablishMainPage));

                }
            }

            //第六部分 标签
            var entryTags = new EntryTags();

            //遍历当前数据 打上删除标签
            foreach (var item in currentEntry.Tags)
            {
                entryTags.Tags.Add(new EntryTagsAloneModel
                {
                    TagId = item.Id,
                    IsDelete = true,
                });
            }

            //添加新建项目
            foreach (var item in newEntry.Tags)
            {
                var temp = entryTags.Tags.FirstOrDefault(s => s.TagId == item.Id);
                if (temp != null)
                {
                    entryTags.Tags.Remove(temp);
                }
                else
                {
                    entryTags.Tags.Add(new EntryTagsAloneModel
                    {
                        TagId = item.Id,
                        IsDelete = false
                    });
                }
            }

            if (entryTags.Tags.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(entryTags, Operation.EstablishTags));
            }

            //第七部分 音频
            var entryAudio = new EntryAudioExamineModel();
            //先把 当前词条中的图片 都 打上删除标签
            foreach (var item in currentEntry.Audio)
            {
                entryAudio.Audio.Add(new EntryAudioExamineAloneModel
                {
                    Url = item.Url,
                    BriefIntroduction = item.BriefIntroduction,
                    Duration = item.Duration,
                    Name = item.Name,
                    Priority = item.Priority,
                    Thumbnail=item.Thumbnail,
                    IsDelete = true
                });
            }
            //再对比当前
            foreach (var infor in newEntry.Audio.ToList().Purge())
            {
                var isSame = false;
                foreach (var item in entryAudio.Audio)
                {
                    if (item.Url == infor.Url)
                    {
                        if (item.BriefIntroduction != infor.BriefIntroduction || item.Name != infor.Name || item.Duration != infor.Duration || item.Priority != infor.Priority || item.Thumbnail != infor.Thumbnail)
                        {
                            item.BriefIntroduction = infor.BriefIntroduction;
                            item.Name = infor.Name;
                            item.IsDelete = false;
                            item.Priority = infor.Priority;
                            item.Duration = infor.Duration;
                            item.Thumbnail = infor.Thumbnail;
                        }
                        else
                        {
                            entryAudio.Audio.Remove(item);
                        }
                        isSame = true;
                        break;

                    }
                }
                if (isSame == false)
                {
                    entryAudio.Audio.Add(new EntryAudioExamineAloneModel
                    {
                        Url = infor.Url,
                        BriefIntroduction = infor.BriefIntroduction,
                        Name = infor.Name,
                        Priority = infor.Priority,
                        Duration = infor.Duration,
                        IsDelete = false,
                        Thumbnail = infor.Thumbnail
                    });
                }
            }

            if (entryAudio.Audio.Any())
            {
                examines.Add(new KeyValuePair<object, Operation>(entryAudio, Operation.EstablishAudio));
            }

            //第八部分 官网模板补充信息
            var entryWebsite = new EntryWebsiteExamineModel();
            if (newEntry.WebsiteAddInfor != null)
            {
                if (currentEntry.WebsiteAddInfor == null)
                {
                    currentEntry.WebsiteAddInfor = new EntryWebsite();
                }

                //背景图
                foreach (var item in currentEntry.WebsiteAddInfor.Images)
                {
                    entryWebsite.Images.Add(new EditWebsiteImage
                    {
                        Type = item.Type,
                        Note = item.Note,
                        Url = item.Url,
                        Size = item.Size,
                        Priority = item.Priority,
                        IsDelete = true
                    });
                }
                //再对比当前
                foreach (var infor in newEntry.WebsiteAddInfor.Images.ToList().Purge())
                {
                    var isSame = false;
                    foreach (var item in entryWebsite.Images)
                    {
                        if (item.Url == infor.Url&&item.Type==infor.Type)
                        {
                            if (item.Size != infor.Size || item.Priority != infor.Priority || item.Note != infor.Note)
                            {
                                item.Size = infor.Size;
                                item.Note = infor.Note;
                                item.Priority = infor.Priority;
                                item.IsDelete = false;
                            }
                            else
                            {
                                entryWebsite.Images.Remove(item);
                            }
                            isSame = true;
                            break;

                        }
                    }
                    if (isSame == false)
                    {
                        entryWebsite.Images.Add(new EditWebsiteImage
                        {
                            Priority = infor.Priority,
                            Note = infor.Note,
                            Type = infor.Type,
                            Url = infor.Url,
                            Size=infor.Size,
                            IsDelete = false
                        });
                    }
                }

                //主要信息
                entryWebsite.MainInfor = ToolHelper.GetEditingRecordFromContrastData(currentEntry.WebsiteAddInfor, newEntry.WebsiteAddInfor);
            }
            if (entryWebsite.Images.Any() || entryWebsite.MainInfor.Any() )
            {
                examines.Add(new KeyValuePair<object, Operation>(entryWebsite, Operation.EstablishWebsite));
            }

            return examines;
        }

        public async Task<List<EntryIndexViewModel>> ConcompareAndGenerateModel(Entry currentEntry, Entry newEntry)
        {
            var model = new List<EntryIndexViewModel>
            {
                await GetEntryIndexViewModelAsync(currentEntry),
                await GetEntryIndexViewModelAsync(newEntry)
            };



            return model;
        }

        public EditMainViewModel GetEditMainViewModel(Entry entry)
        {
            var model = new EditMainViewModel
            {
                Thumbnail = entry.Thumbnail,
                MainPicture = entry.MainPicture,
                BackgroundPicture = entry.BackgroundPicture,

                Name = entry.Name,
                BriefIntroduction = entry.BriefIntroduction,
                Type = entry.Type,
                DisplayName = entry.DisplayName,
                AnotherName = entry.AnotherName,
                SmallBackgroundPicture = entry.SmallBackgroundPicture,
                Template=entry.Template,
                Id = entry.Id
            };

            return model;
        }

        public async Task<EditAddInforViewModel> GetEditAddInforViewModel(Entry entry)
        {

            var model = new EditAddInforViewModel
            {
                Type = entry.Type,
                Id = entry.Id,
                Name = entry.Name
            };

            //根据类别进行序列化
            switch (entry.Type)
            {
                case EntryType.Game:
                    model.Staffs = new List<StaffModel>();
                    //遍历基本信息
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "原作":
                                    model.Original = item.DisplayValue;
                                    break;
                                case "官网":
                                    model.OfficialWebsite = item.DisplayValue;
                                    break;
                                case "QQ群":
                                    model.QQgroupGame = item.DisplayValue;
                                    break;
                            }
                        }
                    }

                    //处理Staff信息
                    foreach (var item in entry.EntryStaffFromEntryNavigation.Where(s => s.PositionGeneral != PositionGeneralType.Publisher && s.PositionGeneral != PositionGeneralType.ProductionGroup))
                    {
                        model.Staffs.Add(new StaffModel
                        {
                            SubordinateOrganization = item.SubordinateOrganization,
                            CustomName = item.CustomName,
                            Modifier = item.Modifier,
                            Name = item.ToEntryNavigation?.Name ?? item.Name,
                            PositionGeneral = item.PositionGeneral,
                            PositionOfficial = item.PositionOfficial,
                            Id = item.EntryStaffId
                        });
                    }
                    //处理制作组发行商信息
                    model.Publisher = GetStringFromStaffs(entry, PositionGeneralType.Publisher);
                    model.ProductionGroup = GetStringFromStaffs(entry, PositionGeneralType.ProductionGroup);
                    //处理游戏发行列表
                    foreach (var item in entry.Releases)
                    {
                        model.Releases.Add(new EditReleaseModel
                        {
                            Engine = item.Engine,
                            Link = item.Link,
                            Name = item.Name,
                            GamePlatformTypes = item.GamePlatformTypes.ToList(),
                            PublishPlatformName = item.PublishPlatformName,
                            Time = item.Time,
                            TimeNote = item.TimeNote,
                            Type = item.Type,
                            PublishPlatformType = item.PublishPlatformType,
                        });
                    }
                    break;
                case EntryType.ProductionGroup:
                    //遍历基本信息
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "QQ群":
                                    model.QQgroupGroup = item.DisplayValue;
                                    break;
                            }
                        }
                    }
                    break;
                case EntryType.Role:
                    //遍历基本信息
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "性别":
                                    if (Enum.TryParse(typeof(GenderType), item.DisplayValue, true, out object gender))
                                    {
                                        model.Gender = (GenderType)gender;
                                    }
                                    break;
                                case "身材数据":
                                    model.FigureData = item.DisplayValue;
                                    break;
                                case "身材(主观)":
                                    model.FigureSubjective = item.DisplayValue;
                                    break;
                                case "生日":
                                    try
                                    {
                                        model.Birthday = DateTime.ParseExact(item.DisplayValue, "M月d日", null);
                                    }
                                    catch
                                    {

                                        model.Birthday = null;

                                    }
                                    break;
                                case "发色":
                                    model.Haircolor = item.DisplayValue;
                                    break;
                                case "瞳色":
                                    model.Pupilcolor = item.DisplayValue;
                                    break;
                                case "服饰":
                                    model.ClothesAccessories = item.DisplayValue;
                                    break;
                                case "性格":
                                    model.Character = item.DisplayValue;
                                    break;
                                case "角色身份":
                                    model.RoleIdentity = item.DisplayValue;
                                    break;
                                case "血型":
                                    model.BloodType = item.DisplayValue;
                                    break;
                                case "身高":
                                    model.RoleHeight = item.DisplayValue;
                                    break;
                                case "兴趣":
                                    model.RoleTaste = item.DisplayValue;
                                    break;
                                case "年龄":
                                    model.RoleAge = item.DisplayValue;
                                    break;
                            }


                        }
                    }

                    //处理声优信息
                    model.CV = GetStringFromStaffs(entry, PositionGeneralType.CV);

                    break;
                case EntryType.Staff:
                    //遍历基本信息
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "姓名":
                                    model.RealName = item.DisplayValue;
                                    break;
                            }
                        }
                    }
                    break;
            }

            //预约
            if (entry.Booking != null)
            {
                model.Booking.IsNeedNotification = entry.Booking.IsNeedNotification;
                model.Booking.Open = entry.Booking.Open;
                if (entry.Booking.LotteryId == 0)
                {
                    model.Booking.LotteryName = null;
                }
                else
                {
                    model.Booking.LotteryName = await _lotteryRepository.GetAll().AsNoTracking().Where(s => s.Id == entry.Booking.LotteryId).Select(s => s.Name).FirstOrDefaultAsync();
                }

                foreach (var item in entry.Booking.Goals)
                {
                    model.Booking.Goals.Add(new EditBookingGoalModel
                    {
                        Name = item.Name,
                        Target = item.Target
                    });
                }

            }

            return model;
        }

        /// <summary>
        /// 将Staffs转换成String
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetStringFromStaffs(Entry entry, PositionGeneralType type)
        {
            StringBuilder text = new StringBuilder();
            var cvs = entry.EntryStaffFromEntryNavigation.Where(s => s.PositionGeneral == type).ToList();
            foreach (var item in cvs)
            {
                if (string.IsNullOrWhiteSpace(item.CustomName))
                {
                    text.Append(item?.ToEntryNavigation?.DisplayName ?? item.Name);
                }
                else
                {
                    text.Append(item.CustomName);
                }
                if(cvs.IndexOf(item)!= cvs.Count-1)
                {
                    text.Append('、');
                }
            }

            return text.ToString();
        }

        public EditImagesViewModel GetEditImagesViewModel(Entry entry)
        {
            //根据类别生成首个视图模型
            var model = new EditImagesViewModel
            {
                Name = entry.Name,
                Id = entry.Id,
            };
            //处理图片
            var Images = new List<EditImageAloneModel>();
            foreach (var item in entry.Pictures)
            {
                Images.Add(new EditImageAloneModel
                {
                    Image = _appHelper.GetImagePath(item.Url, ""),
                    Modifier = item.Modifier,
                    Note = item.Note,
                    Priority = item.Priority,
                });
            }

            model.Images = Images;
            return model;
        }

        public EditRelevancesViewModel GetEditRelevancesViewModel(Entry entry)
        {
            var model = new EditRelevancesViewModel
            {
                Id = entry.Id,
                Name = entry.Name,
                Type = entry.Type
            };
            //处理附加信息
            var roles = new List<RelevancesModel>();
            var staffs = new List<RelevancesModel>();
            var articles = new List<RelevancesModel>();
            var groups = new List<RelevancesModel>();
            var games = new List<RelevancesModel>();
            var news = new List<RelevancesModel>();
            var videos = new List<RelevancesModel>();
            var others = new List<RelevancesModel>();
            foreach (var nav in entry.EntryRelationFromEntryNavigation)
            {
                var item = nav.ToEntryNavigation;
                switch (item.Type)
                {
                    case EntryType.Role:
                        roles.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.Staff:
                        staffs.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.ProductionGroup:
                        groups.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    case EntryType.Game:
                        games.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                }
            }
            foreach (var item in entry.Articles)
            {
                switch (item.Type)
                {
                    case ArticleType.News:
                        news.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                    default:
                        articles.Add(new RelevancesModel
                        {
                            DisplayName = item.Name
                        });
                        break;
                }
            }
            foreach (var item in entry.Videos)
            {
                videos.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }
            foreach (var item in entry.Outlinks)
            {
                others.Add(new RelevancesModel
                {
                    DisplayName = item.Name,
                    DisPlayValue = item.BriefIntroduction,
                    Link = item.Link
                });
            }

            model.Roles = roles;
            model.staffs = staffs;
            model.articles = articles;
            model.Groups = groups;
            model.Games = games;
            model.others = others;
            model.news = news;

            return model;
        }

        public EditEntryTagViewModel GetEditTagsViewModel(Entry entry)
        {
            var model = new EditEntryTagViewModel
            {
                Id = entry.Id,
                Name = entry.Name,
                Type = entry.Type
            };
            //处理标签
            var tags = new List<RelevancesModel>();
            foreach (var item in entry.Tags)
            {
                tags.Add(new RelevancesModel
                {
                    DisplayName = item.Name
                });
            }

            model.Tags = tags;

            return model;
        }

        public EditMainPageViewModel GetEditMainPageViewModel(Entry entry)
        {

            var model = new EditMainPageViewModel
            {
                Context = entry.MainPage,
                Id = entry.Id,
                Type = entry.Type,
                Name = entry.Name
            };

            return model;

        }

        public EditAudioViewModel GetEditAudioViewModel(Entry entry)
        {
            var model = new EditAudioViewModel
            {
                Name = entry.Name,
                Id = entry.Id,
            };
            //处理音频
            foreach (var item in entry.Audio)
            {
                model.Audio.Add(new EditAudioAloneModel
                {
                    BriefIntroduction=item.BriefIntroduction,
                    Name=item.Name,
                    Priority=item.Priority,
                    Url=item.Url,
                    Duration=item.Duration,
                    Thumbnail=item.Thumbnail
                });
            }

            return model;
        }

        public EditEntryWebsiteViewModel GetEditWebsitViewModel(Entry entry)
        {
            var model = new EditEntryWebsiteViewModel
            {
                Name = entry.Name,
                Id = entry.Id,
            };

            if(entry.WebsiteAddInfor==null)
            {
                return model;
            }

            model.Html = entry.WebsiteAddInfor.Html;
            model.Introduction = entry.WebsiteAddInfor.Introduction;
            model.SubTitle = entry.WebsiteAddInfor.SubTitle;
            model.FirstPage = entry.WebsiteAddInfor.FirstPage;
            model.Logo = entry.WebsiteAddInfor.Logo;
            model.Color = entry.WebsiteAddInfor.Color;
            model.Impressions = entry.WebsiteAddInfor.Impressions;

            foreach (var item in entry.WebsiteAddInfor.Images)
            {
                model.Images.Add(new EditWebsiteImageModel
                {
                    Image = item.Url,
                    Note = item.Note,
                    Priority = item.Priority,
                    Type = item.Type,
                    Size=item.Size
                });
            }

            return model;
        }


        public void SetDataFromEditMainViewModel(Entry newEntry, EditMainViewModel model)
        {
            newEntry.Name = model.Name;
            newEntry.BriefIntroduction = model.BriefIntroduction;
            newEntry.MainPicture = model.MainPicture;
            newEntry.Thumbnail = model.Thumbnail;
            newEntry.BackgroundPicture = model.BackgroundPicture;
            newEntry.Type = model.Type;
            newEntry.DisplayName = model.DisplayName;
            newEntry.SmallBackgroundPicture = model.SmallBackgroundPicture;
            newEntry.AnotherName = model.AnotherName;
            newEntry.Template = model.Template;
        }

        public async Task SetDataFromEditAddInforViewModelAsync(Entry newEntry, EditAddInforViewModel model,int lotteryId)
        {
            newEntry.Information.Clear();
            newEntry.Releases.Clear();
            newEntry.EntryStaffFromEntryNavigation.Clear();
            //根据类别进行序列化操作
            switch (model.Type)
            {
                case EntryType.Game:
                    //遍历一遍当前视图中staffs
                    var staffNames = model.Staffs.Select(s => s.Name);
                    var staffEntries = await _entryRepository.GetAll().Where(s => staffNames.Contains(s.Name)).ToListAsync();

                    foreach (var item in model.Staffs)
                    {
                        var temp = new EntryStaff
                        {
                            SubordinateOrganization = item.SubordinateOrganization,
                            CustomName = item.CustomName,
                            ToEntryNavigation = staffEntries.FirstOrDefault(s => s.Name == item.Name),
                            Modifier = item.Modifier,
                            PositionGeneral = item.PositionGeneral,
                            PositionOfficial = item.PositionOfficial,
                        };
                        temp.ToEntry = temp.ToEntryNavigation?.Id;
                        temp.Name = temp.ToEntryNavigation == null ? item.Name : null;

                        newEntry.EntryStaffFromEntryNavigation.Add(temp);
                    }
                    //游戏发行列表
                    foreach (var item in model.Releases)
                    {
                        var temp = new GameRelease
                        {
                            Engine = item.Engine,
                            Link = item.Link,
                            Name = item.Name,
                            GamePlatformTypes = item.GamePlatformTypes.ToArray(),
                            PublishPlatformName = item.PublishPlatformName,
                            Time = item.Time,
                            TimeNote = item.TimeNote,
                            Type = item.Type,
                            PublishPlatformType = item.PublishPlatformType,
                        };
                        newEntry.Releases.Add(temp);
                    }
                    //添加制作组发行商
                    await SetStaffsFromString(newEntry, model.Publisher, PositionGeneralType.Publisher);
                    await SetStaffsFromString(newEntry, model.ProductionGroup, PositionGeneralType.ProductionGroup);
                    //添加基本信息
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "原作", DisplayValue = model.Original });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "官网", DisplayValue = model.OfficialWebsite });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGame });
                    break;
                case EntryType.ProductionGroup:
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGroup });
                    break;
                case EntryType.Role:
                    //添加CV
                    await SetStaffsFromString(newEntry, model.CV, PositionGeneralType.CV);

                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性别", DisplayValue = model.Gender.ToString() });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材数据", DisplayValue = model.FigureData });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身材(主观)", DisplayValue = model.FigureSubjective });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "生日", DisplayValue = model.Birthday?.ToString("M月d日") });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发色", DisplayValue = model.Haircolor });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "瞳色", DisplayValue = model.Pupilcolor });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "性格", DisplayValue = model.Character });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "角色身份", DisplayValue = model.RoleIdentity });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "血型", DisplayValue = model.BloodType });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "身高", DisplayValue = model.RoleHeight });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "兴趣", DisplayValue = model.RoleTaste });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "年龄", DisplayValue = model.RoleAge });


                    break;
                case EntryType.Staff:
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "姓名", DisplayValue = model.RealName });
                    break;
            }

            var tempList = newEntry.Information.ToList();
            tempList.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayValue));
            newEntry.Information = tempList;

            //预约
            newEntry.Booking ??= new Booking();
            newEntry.Booking.Goals.Clear();
            foreach(var item in model.Booking.Goals)
            {
                newEntry.Booking.Goals.Add(new BookingGoal
                {
                    Name = item.Name,
                    Target = item.Target,
                });
            }
            newEntry.Booking.IsNeedNotification = model.Booking.IsNeedNotification;
            newEntry.Booking.LotteryId = lotteryId;
            newEntry.Booking.Open = model.Booking.Open;
        }

        public void SetDataFromEditImagesViewModel(Entry newEntry, EditImagesViewModel model)
        {
            //再遍历视图模型中的图片 对应修改
            newEntry.Pictures.Clear();

            foreach (var item in model.Images)
            {

                newEntry.Pictures.Add(new EntryPicture
                {
                    Url = item.Image,
                    Modifier = item.Modifier,
                    Note = item.Note,
                    Priority = item.Priority,
                });

            }
        }

        public void SetDataFromEditRelevancesViewModel(Entry newEntry, EditRelevancesViewModel model, List<Entry> entries, List<Article> articles, List<Video> videos)
        {
            newEntry.Outlinks.Clear();
            newEntry.Articles = articles;
            newEntry.Videos = videos;
            newEntry.EntryRelationFromEntryNavigation = entries.Select(s => new EntryRelation
            {
                ToEntry = s.Id,
                ToEntryNavigation = s
            }).ToList();

            foreach (var item in model.others)
            {
                newEntry.Outlinks.Add(new Outlink
                {
                    Name = item.DisplayName,
                    BriefIntroduction = item.DisPlayValue,
                    Link = item.Link,
                });
            }


        }

        public void SetDataFromEditMainPageViewModel(Entry newEntry, EditMainPageViewModel model)
        {
            newEntry.MainPage = model.Context;
        }

        public void SetDataFromEditTagsViewModel(Entry newEntry, EditEntryTagViewModel model, List<Tag> tags)
        {
            newEntry.Tags = tags;
        }

        public void SetDataFromEditAudioViewModel(Entry newEntry, EditAudioViewModel model)
        {
            //再遍历视图模型中的图片 对应修改
            newEntry.Audio.Clear();

            foreach (var item in model.Audio)
            {

                newEntry.Audio.Add(new EntryAudio
                {
                    BriefIntroduction = item.BriefIntroduction,
                    Name = item.Name,
                    Priority = item.Priority,
                    Url = item.Url,
                    Duration = item.Duration,
                    Thumbnail=item.Thumbnail
                });

            }
        }

        public void SetDataFromEditWebsiteViewModel(Entry newEntry, EditEntryWebsiteViewModel model)
        {
            newEntry.WebsiteAddInfor ??= new EntryWebsite();
            newEntry.WebsiteAddInfor.Introduction = model.Introduction;
            newEntry.WebsiteAddInfor.FirstPage = model.FirstPage;
            newEntry.WebsiteAddInfor.Html = model.Html;
            newEntry.WebsiteAddInfor.Logo = model.Logo;
            newEntry.WebsiteAddInfor.Impressions = model.Impressions;
            newEntry.WebsiteAddInfor.Color = model.Color;
            newEntry.WebsiteAddInfor.SubTitle = model.SubTitle;
            //再遍历视图模型中的图片 对应修改
            newEntry.WebsiteAddInfor.Images.Clear();

            foreach (var item in model.Images)
            {
                newEntry.WebsiteAddInfor.Images.Add(new EntryWebsiteImage
                {
                    Priority = item.Priority,
                    Url = item.Image,
                    Note = item.Note,
                    Type = item.Type,
                    Size=item.Size
                });
            }
        }


        /// <summary>
        /// 从字符串中设置Staff
        /// </summary>
        /// <param name="newEntry"></param>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task SetStaffsFromString(Entry newEntry, string text, PositionGeneralType type)
        {
            if (string.IsNullOrWhiteSpace(text) == false)
            {
                var publishers = text.Replace("，", ",").Replace("、", ",").Split(',').ToList();
                publishers.RemoveAll(s => string.IsNullOrWhiteSpace(s));

                var publisherEntries = await _entryRepository.GetAll().Where(s => publishers.Contains(s.Name)).ToListAsync();
                foreach (var publisher in publishers)
                {
                    var publisherEntry = publisherEntries.FirstOrDefault(s => s.Name == publisher);

                    if (newEntry.EntryStaffFromEntryNavigation.Any(s => s.ToEntry == publisherEntry?.Id && s.Name == (publisherEntry == null ? publisher : null)&&s.PositionGeneral==type))
                    {
                        continue;
                    }
                    newEntry.EntryStaffFromEntryNavigation.Add(new EntryStaff
                    {
                        Name = publisherEntry == null ? publisher : null,
                        ToEntry = publisherEntry?.Id,
                        ToEntryNavigation = publisherEntry,
                        PositionGeneral = type,
                        PositionOfficial = type.GetDisplayName()
                    });
                }

            }
        }

        /// <summary>
        /// 更新角色生日到缓存
        /// </summary>
        /// <returns></returns>
        public async Task UpdateRoleBrithday()
        {
            var roles = await _entryRepository.GetAll().AsNoTracking()
               .Include(s => s.Information)
               .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
               .Where(s => s.Type == EntryType.Role && s.Information.Any(s => s.DisplayName == "生日"&&string.IsNullOrWhiteSpace(s.DisplayValue)==false))
               .Select(s => new
               {
                   s.Id,
                   s.Name,
                   Brithday = s.Information.FirstOrDefault(s => s.DisplayName == "生日").DisplayValue
               })
               .ToListAsync();

            foreach(var item in roles)
            {
                DateTime day;
                try
                {
                    var temp = item.Brithday.Replace("日", "").Split("月");
                    day = new DateTime(2020, int.Parse(temp[0]), int.Parse(temp[1]), 0, 0, 0, DateTimeKind.Utc);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex,"更新角色 - {name}({id}) 生日时，转换日期（{day}）失败", item.Name, item.Id, item.Brithday);
                    continue;
                }
               
                var brithday = await _roleBirthdayRepository.GetAll().FirstOrDefaultAsync(s => s.RoleId == item.Id);
                if(brithday==null)
                {
                    brithday = new RoleBirthday
                    {
                        RoleId = item.Id,
                        Birthday = day
                    };

                  
                    await _roleBirthdayRepository.InsertAsync(brithday);
                    _logger.LogError("添加角色 - {name}({id}) 生日：{brithday}", item.Name, item.Id, item.Brithday);
                }
                else if(brithday.Birthday != day)
                {
                    brithday.Birthday = day;
                    await _roleBirthdayRepository.UpdateAsync(brithday);
                    _logger.LogError("更新角色 - {name}({id}) 生日：{brithday}", item.Name, item.Id, item.Brithday);
                }
            }

        }

        /// <summary>
        /// 获取今天生日的角色
        /// </summary>
        /// <returns></returns>
        public async Task<List<RoleBrithdayViewModel>> GetBirthdayRoles(int month, int day=0)
        {
            List<RoleBrithdayViewModel> model = new List<RoleBrithdayViewModel>();
            var date = DateTime.UtcNow;

            var roles = await _roleBirthdayRepository.GetAll().AsNoTracking()
                .Include(s => s.Role).ThenInclude(s=>s.EntryRelationFromEntryNavigation).ThenInclude(s=>s.ToEntryNavigation)
                .Where(s=>s.Birthday.Date.Month==month&&(day==0||s.Birthday.Date.Day==day))
                .ToListAsync();

            foreach (var role in roles)
            {
                var entry = new RoleBrithdayViewModel();
                entry.SynchronizationProperties(_appHelper.GetEntryInforTipViewModel(role.Role));
                entry.Brithday = role.Birthday;

                model.Add(entry);
                 
            }

            return model;
        }

        public EntryRoleViewModel GetRoleInfor(Entry entry)
        {
            EntryRoleViewModel roleModel = new EntryRoleViewModel();
            roleModel.SynchronizationProperties(_appHelper.GetEntryInforTipViewModel(entry));

            roleModel.AddInfors.RemoveAll(s => s.Modifier == "登场游戏");
            if (roleModel.AddInfors.Any(s => s.Modifier == "配音" && s.Contents.Any()))
            {
                roleModel.CV = string.Join("、", roleModel.AddInfors.FirstOrDefault(s => s.Modifier == "配音").Contents.Select(s => s.DisplayName).ToArray());
            }

            //查找基础信息
            roleModel.StandingPainting =_appHelper.GetImagePath(entry.MainPicture, "");
            if (entry.Information != null && entry.Information.Any())
            {
                roleModel.Age = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "年龄")?.DisplayValue;
                roleModel.Birthday = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "生日")?.DisplayValue;
                roleModel.Height = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "身高")?.DisplayValue;
                roleModel.RoleIdentity = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "角色身份")?.DisplayValue;
            }

            return roleModel;
        }

        public async Task PostAllBookingNotice(int max)
        {
            var now = DateTime.Now.ToCstTime();
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Booking).ThenInclude(s => s.Users).ThenInclude(s => s.ApplicationUser)
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.PubulishTime != null && s.PubulishTime.Value.Date <= now.Date)
                .Where(s => s.Booking != null && s.Booking.Open && s.Booking.Users.Any(s => s.IsNotified == false))
                .Select(s => new
                {
                    s.Id,
                    s.DisplayName,
                    s.MainPicture,
                    s.BriefIntroduction,
                    s.Pictures,
                    BookingId = s.Booking.Id,
                    Users = s.Booking.Users.Where(s => s.IsNotified == false).Select(s => new
                    {
                        s.ApplicationUser.Id,
                        s.ApplicationUser.Email

                    }).Take(max).ToList()
                }).ToListAsync();

            List<string> userIds = new List<string>();
            foreach(var item in entries)
            {
                var htmlContent = _viewRenderService.Render("~/Models/BookingMsgView.cshtml", new BookingMsgViewModel
                {
                    DisplayName = item.DisplayName,
                    BriefIntroduction = item.BriefIntroduction,
                    Link = $"https://www.cngal.org/entries/index/{item.Id}",
                    MainPicture = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    Pictures = item.Pictures.OrderByDescending(s => s.Priority).Select(s => s.Url).Take(4).ToList()
                });

                foreach (var infor in item.Users)
                {
                    _emailService.Send(infor.Email, $"《{item.DisplayName}》已发布", htmlContent,true);
                    userIds.Add(infor.Id);
                    if(userIds.Count> max)
                    {
                        break;
                    }
                }
                if (userIds.Count > max)
                {
                    break;
                }
            }

            await _bookingUserRepository.GetAll().Where(s => userIds.Contains(s.ApplicationUserId) && s.BookingId != null && entries.Select(s => s.BookingId).Contains(s.BookingId.Value)).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsNotified, b => true));
        }
    }
}
