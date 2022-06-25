using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries.Dtos;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.Helper.Extensions;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Entry>, string, BootstrapBlazor.Components.SortOrder, IEnumerable<Entry>>> SortLambdaCacheEntry = new();

        public EntryService(IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<DataModel.Model.Tag, int> tagRepository, IRepository<Article, int> articleRepository, IRepository<PlayedGame, long> playedGameRepository,
        IRepository<Examine, long> examineRepository, IArticleService articleService)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _tagRepository = tagRepository;
            _examineRepository = examineRepository;
            _articleRepository = articleRepository;
            _articleService = articleService;
            _playedGameRepository = playedGameRepository;

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
                models = await query.AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .ToListAsync();
            }
            else
            {
                models = new List<Entry>();
            }

            var dtos = new List<EntryInforTipViewModel>();
            foreach (var item in models)
            {
                dtos.Add(await _appHelper.GetEntryInforTipViewModel(item));
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

        public void UpdateEntryDataAddInfor(Entry entry, EntryAddInfor examine)
        {

            foreach (var item in examine.Information)
            {
                //预处理
                //更新部分重要信息缓存
                if (item.IsDelete == false)
                {
                    if (entry.Type == EntryType.Game)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            //查找是否修改发行时间 对下文不影响 只是更新字段缓存
                            if (item.DisplayName == "发行时间")
                            {
                                if (item.DisplayValue == null)
                                {
                                    entry.PubulishTime = null;
                                }
                                else
                                {
                                    try
                                    {
                                        entry.PubulishTime = DateTime.ParseExact(item.DisplayValue, "yyyy年M月d日", null);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            entry.PubulishTime = DateTime.ParseExact(item.DisplayValue, "yyyy/M/d", null);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                            }
                            else if (item.DisplayName == "Steam平台Id")
                            {
                                if (string.IsNullOrWhiteSpace(item.DisplayValue) == false && string.IsNullOrWhiteSpace(entry.MainPicture))
                                {
                                    entry.MainPicture = "https://media.st.dl.eccdnx.com/steam/apps/" + item.DisplayValue + "/header.jpg";
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (entry.Type == EntryType.Game)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            //查找是否修改发行时间 对下文不影响 只是更新字段缓存
                            if (item.DisplayName == "发行时间")
                            {
                                entry.PubulishTime = null;
                            }
                        }
                    }
                }



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
                        Modifier = item.Modifier
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

                    UpdateEntryDataAddInfor(entry, entryAddInfor);
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
                default:
                    throw new InvalidOperationException("不支持的操作");
            }
        }

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
                               || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags))
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
                        model.MainState = EditState.locked;
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
                        model.InforState = EditState.locked;
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
                        model.MainPageState = EditState.locked;
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
                        model.ImagesState = EditState.locked;
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
                        model.RelevancesState = EditState.locked;
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
                        model.TagState = EditState.locked;
                    }
                    else
                    {
                        model.TagState = EditState.Normal;
                    }
                }
            }

            return model;
        }

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
                DisambigId = entry.DisambigId ?? 0,
                DisambigName = entry.Disambig?.Name,
                AnotherName = entry.AnotherName,
                IsHidden = entry.IsHidden,
                IsHideOutlink = entry.IsHideOutlink,
            };


            //初始化图片链接
            model.MainPicture = _appHelper.GetImagePath(entry.MainPicture, (entry.Type == EntryType.Staff || entry.Type == EntryType.Role) ? "" : "app.png");
            model.BackgroundPicture = _appHelper.GetImagePath(entry.BackgroundPicture, "");
            model.Thumbnail = _appHelper.GetImagePath(entry.Thumbnail, "user.png");
            model.SmallBackgroundPicture = _appHelper.GetImagePath(entry.SmallBackgroundPicture, "");


            //初始化主页Html代码
            model.MainPage = _appHelper.MarkdownToHtml(entry.MainPage ?? "");




            //读取词条信息
            var tempInformation = entry.Information.Where(s => s.Modifier != "STAFF").ToList();
            //添加别称到附加信息
            if (string.IsNullOrWhiteSpace(entry.AnotherName) == false)
            {
                tempInformation.Add(new BasicEntryInformation
                {
                    DisplayName = "别称",
                    DisplayValue = model.AnotherName,
                    Modifier = "基本信息"
                });
            }

            var information = new List<InformationsModel>();
            var issuleTime = "";
            var issuleTimeString = "";
            if (tempInformation.Count > 0)
            {
                var Publisher = tempInformation.FirstOrDefault(s => s.DisplayName == "发行商")?.DisplayValue;
                if (string.IsNullOrWhiteSpace(Publisher) == false)
                {
                    var temp = Publisher.Replace("，", ",").Replace("、", ",").Split(',');

                    foreach (var item in temp)
                    {
                        model.Publishers.Add(new StaffNameModel
                        {
                            DisplayName = item,
                        });
                    }
                }
                var GroupVaule = tempInformation.FirstOrDefault(s => s.DisplayName == "制作组")?.DisplayValue;
                if (string.IsNullOrWhiteSpace(GroupVaule) == false)
                {
                    var temp = GroupVaule.Replace("，", ",").Replace("、", ",").Split(',');

                    foreach (var item in temp)
                    {
                        model.ProductionGroups.Add(new StaffNameModel
                        {
                            DisplayName = item,
                        });
                    }
                }

            }
            foreach (var item in tempInformation)
            {
                //判断
                if (item.DisplayName == "性别")
                {
                    if (Enum.TryParse(typeof(GenderType), item.DisplayValue, true, out object gender))
                    {
                        item.DisplayValue = ((GenderType)gender).GetDisplayName();
                    }
                }
                else if (item.DisplayName == "Steam平台Id")
                {
                    if (string.IsNullOrWhiteSpace(item.DisplayValue) == false)
                    {
                        try
                        {
                            model.SteamId = int.Parse(item.DisplayValue);
                            continue;
                        }
                        catch { }
                    }

                }
                else if (item.DisplayName == "发行时间")
                {
                    issuleTime = item.DisplayValue;
                }
                else if (item.DisplayName == "发行时间备注")
                {
                    issuleTimeString = item.DisplayValue;
                }
                else if (item.DisplayName == "制作组" || item.DisplayName == "发行商")
                {
                    continue;
                }

                var isAdd = false;
                //如果信息值为空 则不显示
                if (string.IsNullOrWhiteSpace(item.DisplayValue) == true)
                {
                    continue;
                }
                //遍历信息列表寻找关键词
                foreach (var infor in information)
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
                    information.Add(temp);
                }
            }

            //查找发行时间相关
            if (entry.Type == EntryType.Game)
            {
                for (var i = 0; i < information.Count; i++)
                {
                    if (information[i].Modifier == "基本信息")
                    {
                        for (var k = 0; k < information[i].Informations.Count; k++)
                        {
                            var item = information[i].Informations[k];

                            if (item.DisplayName == "发行时间")
                            {
                                if (string.IsNullOrWhiteSpace(issuleTimeString) == false)
                                {
                                    information[i].Informations.Remove(item);
                                    k--;
                                    continue;
                                }
                            }
                            else if (item.DisplayName == "发行时间备注")
                            {
                                if (string.IsNullOrWhiteSpace(issuleTimeString) == false)
                                {
                                    item.DisplayName = "发行时间";
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            //序列化 STAFF
            //先读取词条信息
            var staffInforModel = new List<StaffInforModel>
            {
                new StaffInforModel
                {
                    Modifier = "",
                    StaffList = new List<StaffValue>()
                }
            };
            tempInformation = entry.Information.Where(s => s.Modifier == "STAFF").OrderBy(s => s.Id).ToList();
            foreach (var item in tempInformation)
            {

                var isAdd = false;
                //如果信息值为空 则不显示
                if (string.IsNullOrWhiteSpace(item.DisplayValue) == true)
                {
                    continue;
                }
                //尝试获取staff的显示名称
                var displayName = item.Additional.FirstOrDefault(s => s.DisplayName == "昵称（官方称呼）")?.DisplayValue;//(await _entryRepository.FirstOrDefaultAsync(s => s.Name == item.DisplayValue && s.Type == EntryType.Staff))?.DisplayName ?? item.DisplayValue;
                var mainModifier = "";
                var secordModifier = item.Additional.FirstOrDefault(s => s.DisplayName == "职位（官方称呼）")?.DisplayValue;
                foreach (var infor in item.Additional)
                {
                    if (infor.DisplayName == "子项目")
                    {
                        mainModifier = infor.DisplayValue ?? "";
                    }
                }

                //遍历信息列表寻找 主关键词
                foreach (var infor in staffInforModel)
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
                    });
                    staffInforModel.Add(temp);
                }
            }

            //如果所有staff都有分组 则删除默认空分组
            if (staffInforModel[0].StaffList.Count == 0)
            {
                staffInforModel.RemoveAt(0);
            }


            //为Staff匹配Id
            var staffRealNames = new List<StaffNameModel>();
            staffRealNames.AddRange(model.Publishers);
            staffRealNames.AddRange(model.ProductionGroups);
            foreach (var item in staffInforModel)
            {
                foreach (var temp in item.StaffList)
                {
                    staffRealNames.AddRange(temp.Names);
                }
            }

            var staffRealIds = await _entryRepository.GetAll().AsNoTracking().Where(s => staffRealNames.Select(s => s.DisplayName).Contains(s.Name)).Select(s => new
            {
                s.DisplayName,
                s.Name,
                s.Id
            }).ToListAsync();

            foreach (var item in staffRealIds)
            {
                var temp = staffRealNames.Where(s => s.DisplayName == item.Name).ToList();
                foreach (var infor in temp)
                {
                    infor.DisplayName = item.DisplayName;
                    infor.Id = item.Id;
                }
            }

            //序列化图片列表

            //读取词条信息
            var pictures = new List<EntryPicture>();
            foreach (var item in entry.Pictures)
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



            //序列化标签列表

            //读取词条信息
            var tags = new List<TagsViewModel>();
            foreach (var item in entry.Tags)
            {
                tags.Add(new TagsViewModel { Name = item.Name, Id = item.Id });
            }

            //序列化相关性列表
            //加载附加信息 关联词条获取
            var roleInforModel = new List<EntryInforTipViewModel>();
            var newsModel = new List<NewsModel>();
            var staffGames = new List<EntryInforTipViewModel>();
            var relevancesEntry = new List<EntryInforTipViewModel>();
            var relevanceArticle = new List<ArticleInforTipViewModel>();
            var relevanceOther = new List<RelevancesKeyValueModel>();

            foreach (var item in entry.Articles.Where(s=>s.IsHidden==false))
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
            foreach (var nav in entry.EntryRelationFromEntryNavigation.Where(s => s.ToEntryNavigation.IsHidden == false))
            {
                var item = nav.ToEntryNavigation;
                if (item.Type == EntryType.Role)
                {
                    if (entry.Type != EntryType.Game)
                    {
                        var role = await _appHelper.GetEntryInforTipViewModel(item);
                        if (entry.Type == EntryType.Staff)
                        {
                            role.AddInfors.RemoveAll(s => s.Modifier == "配音");
                        }
                        relevancesEntry.Add(role);
                    }
                    else
                    {

                        //获取角色词条
                        var role = await _appHelper.GetEntryInforTipViewModel(item);
                        role.AddInfors.RemoveAll(s => s.Modifier == "登场游戏");

                        roleInforModel.Add(role);
                    }

                }
                else if (item.Type == EntryType.Game)
                {
                    if (entry.Type == EntryType.Staff)
                    {

                        //获取角色词条
                        var staffGame = await _appHelper.GetEntryInforTipViewModel(item);
                        staffGame.AddInfors.Clear();
                        //查找担任过的职位
                        var tempStaffs = item.Information.Where(s => s.Modifier == "STAFF" && s.DisplayValue == entry.Name);
                        if (tempStaffs.Any())
                        {
                            var inforPositions = new List<string>();
                            foreach (var roleInfor in tempStaffs)
                            {
                                inforPositions.Add(roleInfor.DisplayName);
                            }

                            staffGame.AddInfors.Add(new EntryInforTipAddInforModel
                            {
                                Modifier = "职位",
                                Contents = tempStaffs.Select(s => new StaffNameModel
                                {
                                    DisplayName = (s.Additional == null || s.Additional.Count == 0) ? s.DisplayName : ((s.Additional.Any(s => s.DisplayName == "职位（官方称呼）" && string.IsNullOrWhiteSpace(s.DisplayValue) == false) && s.Additional.Any(s => s.DisplayName == "子项目" && string.IsNullOrWhiteSpace(s.DisplayValue) == false)) ? s.Additional.FirstOrDefault(s => s.DisplayName == "子项目")?.DisplayValue + " - " + s.Additional.FirstOrDefault(s => s.DisplayName == "职位（官方称呼）")?.DisplayValue : s.DisplayName),
                                    Id = -1
                                }).ToList()
                            });
                        }

                        staffGames.Add(staffGame);

                    }
                    else
                    {
                        relevancesEntry.Add(await _appHelper.GetEntryInforTipViewModel(item));
                    }
                }
                else if (item.Type == EntryType.Staff)
                {
                    if (entry.Type != EntryType.Game)
                    {
                        relevancesEntry.Add(await _appHelper.GetEntryInforTipViewModel(item));
                    }
                }
                else if (item.Type == EntryType.ProductionGroup)
                {
                    relevancesEntry.Add(await _appHelper.GetEntryInforTipViewModel(item));
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

            //赋值
            model.Information = information;
            model.Pictures = picturesViewModels;
            model.ArticleRelevances = relevanceArticle;
            model.EntryRelevances = relevancesEntry;
            model.OtherRelevances = relevanceOther;
            model.Tags = tags;
            model.Roles = roleInforModel;
            model.StaffGames = staffGames;
            model.NewsOfEntry = newsModel;

            model.Staffs = staffInforModel;

            //获取是否评分
            if(model.Type== EntryType.Game)
{
                model.IsScored = await _playedGameRepository.GetAll().AnyAsync(s => s.EntryId == model.Id && s.ShowPublicly && s.MusicSocre != 0 && s.ShowSocre != 0 && s.TotalSocre != 0 && s.PaintSocre != 0 && s.ScriptSocre != 0);
            }

            return model;
        }

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

            if (entryAddInfor.Information.Count != 0)
            {
                examines.Add(new KeyValuePair<object, Operation>(entryAddInfor, Operation.EstablishAddInfor));

            }

            //第三部分 图片
            var entryImages = new EntryImages();
            //先把 当前词条中的图片 都 打上删除标签
            foreach (var item in currentEntry.Pictures)
            {
                entryImages.Images.Add(new EntryImage
                {
                    Url = item.Url,
                    Note = item.Note,
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
                        if (item.Note != infor.Note || item.Modifier != infor.Modifier)
                        {
                            item.Modifier = infor.Modifier;
                            item.IsDelete = false;
                            item.Note = infor.Note;
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
                    entryImages.Images.Add(new EntryImage
                    {
                        Url = infor.Url,
                        Modifier = infor.Modifier,
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
                entryRelevances.Relevances.Add(new EntryRelevancesAloneModel
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
                    entryRelevances.Relevances.Add(new EntryRelevancesAloneModel
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
                entryRelevances.Relevances.Add(new EntryRelevancesAloneModel
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
                    entryRelevances.Relevances.Add(new EntryRelevancesAloneModel
                    {
                        DisplayName = item.Id.ToString(),
                        DisplayValue = item.Name,
                        Type = RelevancesType.Article,
                        IsDelete = false
                    });
                }
            }

            //处理外部链接

            //遍历当前词条外部链接 打上删除标签
            foreach (var item in currentEntry.Outlinks)
            {
                entryRelevances.Relevances.Add(new EntryRelevancesAloneModel
                {
                    DisplayName = item.Name,
                    DisplayValue = item.BriefIntroduction,
                    IsDelete = true,
                    Type = RelevancesType.Outlink,
                    Link = item.Link
                });
            }


            //循环查找外部链接是否相同
            foreach (var infor in newEntry.Outlinks)
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
                    entryRelevances.Relevances.Add(new EntryRelevancesAloneModel
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
                MainPicturePath = _appHelper.GetImagePath(entry.MainPicture, "app.png"),
                Thumbnail = entry.Thumbnail,
                MainPicture = entry.MainPicture,
                BackgroundPicture = entry.BackgroundPicture,

                Name = entry.Name,
                BriefIntroduction = entry.BriefIntroduction,
                Type = entry.Type,
                DisplayName = entry.DisplayName,
                AnotherName = entry.AnotherName,
                SmallBackgroundPicture = entry.SmallBackgroundPicture,

                Id = entry.Id
            };

            return model;
        }

        public EditAddInforViewModel GetEditAddInforViewModel(Entry entry)
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
                    model.GamePlatforms = new List<GamePlatformModel>
                    {
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Android, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Windows, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.DOS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.IOS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Linux, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.Mac, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.NS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.PS, IsSelected = false },
                        new GamePlatformModel { GamePlatformType = GamePlatformType.HarmonyOS, IsSelected = false }
};
                    //遍历基本信息
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "发行时间":
                                    try
                                    {
                                        model.IssueTime = DateTime.ParseExact(item.DisplayValue, "yyyy年M月d日", null);
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            model.IssueTime = DateTime.ParseExact(item.DisplayValue, "yyyy/M/d", null);
                                        }
                                        catch
                                        {
                                            model.IssueTime = null;
                                        }
                                    }

                                    break;
                                case "发行时间备注":
                                    model.IssueTimeString = item.DisplayValue;
                                    break;
                                case "原作":
                                    model.Original = item.DisplayValue;
                                    break;
                                case "制作组":
                                    model.ProductionGroup = item.DisplayValue;
                                    break;
                                case "游戏平台":

                                    var sArray = Regex.Split(item.DisplayValue, "、", RegexOptions.IgnoreCase);
                                    foreach (var str in sArray)
                                    {
                                        switch (str)
                                        {
                                            case "Windows":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Windows)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "Linux":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Linux)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "Mac":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Mac)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "IOS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.IOS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "Android":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.Android)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "PS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.PS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "NS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.NS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "DOS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.DOS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                            case "HarmonyOS":
                                                foreach (var temp in model.GamePlatforms)
                                                {
                                                    if (temp.GamePlatformType == GamePlatformType.HarmonyOS)
                                                    {
                                                        temp.IsSelected = true;
                                                        break;
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                case "引擎":
                                    model.Engine = item.DisplayValue;
                                    break;
                                case "发行商":
                                    model.Publisher = item.DisplayValue;
                                    break;
                                case "发行方式":
                                    model.IssueMethod = item.DisplayValue;
                                    break;
                                case "官网":
                                    model.OfficialWebsite = item.DisplayValue;
                                    break;
                                case "Steam平台Id":
                                    model.SteamId = item.DisplayValue;
                                    break;
                                case "QQ群":
                                    model.QQgroupGame = item.DisplayValue;
                                    break;
                            }
                        }
                        else if (item.Modifier == "STAFF")
                        {
                            var staffModel = new StaffModel
                            {
                                Id = item.Id
                            };
                            foreach (var infor in item.Additional)
                            {
                                switch (infor.DisplayName)
                                {
                                    case "职位（官方称呼）":
                                        staffModel.PositionOfficial = infor.DisplayValue;
                                        break;
                                    case "昵称（官方称呼）":
                                        staffModel.NicknameOfficial = infor.DisplayValue;
                                        break;
                                    case "职位（通用）":
                                        staffModel.PositionGeneral = (PositionGeneralType)Enum.Parse(typeof(PositionGeneralType), infor.DisplayValue);
                                        break;
                                    case "角色":
                                        staffModel.Role = infor.DisplayValue;
                                        break;
                                    case "隶属组织":
                                        staffModel.SubordinateOrganization = infor.DisplayValue;
                                        break;
                                    case "子项目":
                                        staffModel.Subcategory = infor.DisplayValue;
                                        break;
                                }
                            }
                            model.Staffs.Add(staffModel);

                        }
                        else if (item.Modifier == "相关网站")
                        {
                            var socialPlatform = new SocialPlatform
                            {
                                Name = item.DisplayName,
                                Link = item.DisplayValue
                            };

                            model.SocialPlatforms.Add(socialPlatform);

                        }
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
                        else if (item.Modifier == "相关网站")
                        {
                            if (item.DisplayValue.Contains("weibo.com"))
                            {
                                model.WeiboId = item.DisplayValue.Replace("https://weibo.com/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("bilibili.com"))
                            {
                                model.BilibiliId = item.DisplayValue.Replace("https://space.bilibili.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("acfun.cn"))
                            {
                                model.AcFunId = item.DisplayValue.Replace("https://www.acfun.cn/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("zhihu.com"))
                            {
                                model.ZhihuId = item.DisplayValue.Replace("https://www.zhihu.com/people/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("afdian.net"))
                            {
                                model.AfdianId = item.DisplayValue.Replace("https://afdian.net/@", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("pixiv.net"))
                            {
                                model.PixivId = item.DisplayValue.Replace("https://www.pixiv.net/users/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("twitter.com"))
                            {
                                model.TwitterId = item.DisplayValue.Replace("https://twitter.com/", "").Replace("https://www.twitter.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("youtube.com"))
                            {
                                model.YouTubeId = item.DisplayValue.Replace("https://www.youtube.com/channel/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("facebook.com"))
                            {
                                model.FacebookId = item.DisplayValue.Replace("https://www.facebook.com/", "").Split('/').FirstOrDefault();

                            }
                            else
                            {
                                var socialPlatform = new SocialPlatform
                                {
                                    Name = item.DisplayName,
                                    Link = item.DisplayValue
                                };

                                model.SocialPlatforms.Add(socialPlatform);
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
                                case "声优":
                                    model.CV = item.DisplayValue;
                                    break;
                                case "性别":
                                    if(Enum.TryParse(typeof(GenderType), item.DisplayValue,true,  out object gender))
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
                        else if (item.Modifier == "相关网站")
                        {
                            var socialPlatform = new SocialPlatform
                            {
                                Name = item.DisplayName,
                                Link = item.DisplayValue
                            };

                            model.SocialPlatforms.Add(socialPlatform);

                        }

                    }
                    break;
                case EntryType.Staff:
                    //遍历基本信息
                    foreach (var item in entry.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {

                        }
                        else if (item.Modifier == "相关网站")
                        {
                            if (item.DisplayValue.Contains("weibo.com"))
                            {
                                model.WeiboId = item.DisplayValue.Replace("https://weibo.com/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("bilibili.com"))
                            {
                                model.BilibiliId = item.DisplayValue.Replace("https://space.bilibili.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("acfun.cn"))
                            {
                                model.AcFunId = item.DisplayValue.Replace("https://www.acfun.cn/u/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("zhihu.com"))
                            {
                                model.ZhihuId = item.DisplayValue.Replace("https://www.zhihu.com/people/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("afdian.net"))
                            {
                                model.AfdianId = item.DisplayValue.Replace("https://afdian.net/@", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("pixiv.net"))
                            {
                                model.PixivId = item.DisplayValue.Replace("https://www.pixiv.net/users/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("twitter.com"))
                            {
                                model.TwitterId = item.DisplayValue.Replace("https://twitter.com/", "").Replace("https://www.twitter.com/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("youtube.com"))
                            {
                                model.YouTubeId = item.DisplayValue.Replace("https://www.youtube.com/channel/", "").Split('/').FirstOrDefault();

                            }
                            else if (item.DisplayValue.Contains("facebook.com"))
                            {
                                model.FacebookId = item.DisplayValue.Replace("https://www.facebook.com/", "").Split('/').FirstOrDefault();

                            }
                            else
                            {
                                var socialPlatform = new SocialPlatform
                                {
                                    Name = item.DisplayName,
                                    Link = item.DisplayValue
                                };

                                model.SocialPlatforms.Add(socialPlatform);
                            }

                        }
                    }


                    break;
            }

            return model;
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
                    Url = _appHelper.GetImagePath(item.Url, ""),
                    Modifier = item.Modifier,
                    Note = item.Note
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
            foreach (var item in entry.Outlinks)
            {
                if (item.Link.Contains("zh.moegirl.org.cn"))
                {
                    model.MoegirlName = HttpUtility.UrlDecode(item.Link.Replace("https://zh.moegirl.org.cn/", "").Split('/').FirstOrDefault());
                }
                else if (item.Link.Contains("vndb.org"))
                {
                    model.VNDBId = item.Link.Replace("https://vndb.org/", "").Split('/').FirstOrDefault();
                }
                else if (item.Link.Contains("www.ymgal.com"))
                {
                    model.YMGalId = item.Link.Split('/').LastOrDefault();
                }
                else if (item.Link.Contains("bangumi.tv")|| item.Link.Contains("bgm.tv"))
                {
                    model.BangumiId = item.Link.Split('/').LastOrDefault();
                }
                else if (item.Link.Contains("wikidata.org"))
                {
                    model.WikiDataId = item.Link.Replace("https://www.wikidata.org/wiki/", "").Split('/').FirstOrDefault();
                }
                else if (item.Link.Contains("galge.fun") || item.Link.Contains("www.2dfan.com"))
                {
                    model._2DFanId = item.Link.Replace("https://galge.fun/subjects/", "").Replace("https://www.2dfan.com/subjects/", "").Split('/').FirstOrDefault();
                }
                else if (item.Link.Contains("baike.baidu.com"))
                {
                    model.BaiDuName = HttpUtility.UrlDecode(item.Link.Replace("https://baike.baidu.com/item/", "").Split('/').FirstOrDefault());
                }
                else if (item.Link.Contains("zh.wikipedia.org"))
                {
                    model.ZhWikiPediaName = HttpUtility.UrlDecode(item.Link.Replace("https://zh.wikipedia.org/wiki/", "").Split('/').FirstOrDefault());
                }
                else
                {
                    others.Add(new RelevancesModel
                    {
                        DisplayName = item.Name,
                        DisPlayValue = item.BriefIntroduction,
                        Link = item.Link
                    });
                }

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
        }

        public void SetDataFromEditAddInforViewModel(Entry newEntry, EditAddInforViewModel model)
        {
            newEntry.Information.Clear();
            //根据类别进行序列化操作
            switch (model.Type)
            {
                case EntryType.Game:
                    //遍历一遍当前视图中staffs 
                    foreach (var item in model.Staffs)
                    {

                        var staffs = new List<BasicEntryInformationAdditional>
                        {
                            new BasicEntryInformationAdditional { DisplayName = "职位（官方称呼）", DisplayValue = item.PositionOfficial },
                            new BasicEntryInformationAdditional { DisplayName = "昵称（官方称呼）", DisplayValue = item.NicknameOfficial },
                            new BasicEntryInformationAdditional { DisplayName = "职位（通用）", DisplayValue = item.PositionGeneral.ToString() },
                            new BasicEntryInformationAdditional { DisplayName = "角色", DisplayValue = item.Role },
                            new BasicEntryInformationAdditional { DisplayName = "隶属组织", DisplayValue = item.SubordinateOrganization },
                            new BasicEntryInformationAdditional { DisplayName = "子项目", DisplayValue = item.Subcategory }
                        };
                        newEntry.Information.Add(new BasicEntryInformation
                        {
                            Modifier = "STAFF",
                            DisplayName = item.Subcategory + item.PositionOfficial,
                            DisplayValue = item.NicknameOfficial,
                            Additional = staffs
                        });
                    }
                    //序列化游戏平台
                    string gamePlatforms = null;
                    var isFirst = true;
                    foreach (var item in model.GamePlatforms)
                    {
                        if (item.IsSelected == true)
                        {
                            if (isFirst == true)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                gamePlatforms += "、";
                            }
                            gamePlatforms += item.GamePlatformType.ToString();
                        }
                    }
                    //添加基本信息
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间", DisplayValue = model.IssueTime?.ToString("yyyy年M月d日") });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行时间备注", DisplayValue = model.IssueTimeString });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "原作", DisplayValue = model.Original });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "制作组", DisplayValue = model.ProductionGroup });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "游戏平台", DisplayValue = gamePlatforms });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "引擎", DisplayValue = model.Engine });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行商", DisplayValue = model.Publisher });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "发行方式", DisplayValue = model.IssueMethod });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "官网", DisplayValue = model.OfficialWebsite });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "Steam平台Id", DisplayValue = model.SteamId });
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGame });
                    break;
                case EntryType.ProductionGroup:
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "QQ群", DisplayValue = model.QQgroupGroup });
                    break;
                case EntryType.Role:
                    newEntry.Information.Add(new BasicEntryInformation { Modifier = "基本信息", DisplayName = "声优", DisplayValue = model.CV });
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
                    break;
            }
            //序列化相关网站

            //加载在基本信息中的网站
            if (string.IsNullOrWhiteSpace(model.WeiboId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "微博",
                    Link = "https://weibo.com/u/" + model.WeiboId
                });
            }
            if (string.IsNullOrWhiteSpace(model.BilibiliId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Bilibili",
                    Link = "https://space.bilibili.com/" + model.BilibiliId
                });
            }
            if (string.IsNullOrWhiteSpace(model.AcFunId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "AcFun",
                    Link = "https://www.acfun.cn/u/" + model.AcFunId
                });
            }
            if (string.IsNullOrWhiteSpace(model.ZhihuId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "知乎",
                    Link = "https://www.zhihu.com/people/" + model.ZhihuId
                });
            }
            if (string.IsNullOrWhiteSpace(model.AfdianId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "爱发电",
                    Link = "https://afdian.net/@" + model.AfdianId
                });
            }
            if (string.IsNullOrWhiteSpace(model.PixivId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Pixiv",
                    Link = "https://www.pixiv.net/users/" + model.PixivId
                });
            }
            if (string.IsNullOrWhiteSpace(model.TwitterId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Twitter",
                    Link = "https://twitter.com/" + model.TwitterId
                });
            }
            if (string.IsNullOrWhiteSpace(model.YouTubeId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "YouTube",
                    Link = "https://www.youtube.com/channel/" + model.YouTubeId
                });
            }
            if (string.IsNullOrWhiteSpace(model.FacebookId) == false)
            {
                model.SocialPlatforms.Add(new SocialPlatform
                {
                    Name = "Facebook",
                    Link = "https://www.facebook.com/" + model.FacebookId
                });
            }


            //遍历一遍当前视图中 相关网站
            foreach (var item in model.SocialPlatforms)
            {
                newEntry.Information.Add(new BasicEntryInformation
                {
                    Modifier = "相关网站",
                    DisplayName = item.Name,
                    DisplayValue = item.Link
                });
            }

            var tempList = newEntry.Information.ToList();
            tempList.RemoveAll(s => string.IsNullOrWhiteSpace(s.DisplayValue));
            newEntry.Information = tempList;
        }

        public void SetDataFromEditImagesViewModel(Entry newEntry, EditImagesViewModel model)
        {
            //再遍历视图模型中的图片 对应修改
            newEntry.Pictures.Clear();

            foreach (var item in model.Images)
            {

                newEntry.Pictures.Add(new EntryPicture
                {
                    Url = item.Url,
                    Modifier = item.Modifier,
                    Note = item.Note
                });

            }
        }

        public void SetDataFromEditRelevancesViewModel(Entry newEntry, EditRelevancesViewModel model, List<Entry> entries, List<Article> articles)
        {
            //加载在关联信息中的网站
            if (string.IsNullOrWhiteSpace(model.MoegirlName) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "萌娘百科",
                    Link = "https://zh.moegirl.org.cn/" + HttpUtility.UrlDecode(model.MoegirlName)
                });
            }

            if (string.IsNullOrWhiteSpace(model.VNDBId) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "VNDB",
                    Link = "https://vndb.org/" + model.VNDBId
                });
            }
            if (string.IsNullOrWhiteSpace(model.YMGalId) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "月幕Galgame",
                    Link = "https://www.ymgal.com/" + model.YMGalId
                });
            }

            if (string.IsNullOrWhiteSpace(model.BangumiId) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "Bangumi",
                    Link = "https://bangumi.tv/" +model.Type switch
                    {
                        EntryType.Game => "subject/",
                        EntryType.Role => "character/",
                        EntryType.Staff => "person/",
                        EntryType.ProductionGroup => "person/",
                        _ => "subject/",
                    } + model.BangumiId
                });
            }

            if (string.IsNullOrWhiteSpace(model.WikiDataId) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "WikiData",
                    Link = "https://www.wikidata.org/wiki/" + model.WikiDataId
                });
            }
            if (string.IsNullOrWhiteSpace(model.BaiDuName) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "百度百科",
                    Link = "https://baike.baidu.com/item/" + HttpUtility.UrlDecode(model.BaiDuName)
                });
            }
            if (string.IsNullOrWhiteSpace(model.ZhWikiPediaName) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "中文维基百科",
                    Link = "https://zh.wikipedia.org/wiki/" + HttpUtility.UrlDecode(model.ZhWikiPediaName)
                });
            }
            if (string.IsNullOrWhiteSpace(model._2DFanId) == false)
            {
                model.others.Add(new RelevancesModel
                {
                    DisplayName = "2DFan",
                    Link = "https://galge.fun/subjects/" + model._2DFanId
                });
            }

            newEntry.Outlinks.Clear();
            newEntry.Articles = articles;
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
    }
}
