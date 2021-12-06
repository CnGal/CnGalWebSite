using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
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
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Entries
{
    public class EntryService : IEntryService
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<DataModel.Model.Tag, int> _tagRepository;
        private readonly IAppHelper _appHelper;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Entry>, string, SortOrder, IEnumerable<Entry>>> SortLambdaCacheEntry = new();

        public EntryService(IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<DataModel.Model.Tag, int> tagRepository, 
           IRepository<Examine, long> examineRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _tagRepository = tagRepository;
            _examineRepository = examineRepository;
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

        public Task<QueryData<ListEntryAloneModel>> GetPaginatedResult(QueryPageOptions options, ListEntryAloneModel searchModel)
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


            // 处理 Searchable=true 列与 SeachText 模糊搜索
            if (options.Searchs.Any())
            {

                // items = items.Where(options.Searchs.GetFilterFunc<Entry>(FilterLogic.Or));
            }
            else
            {
                // 处理 SearchText 模糊搜索
                if (!string.IsNullOrWhiteSpace(options.SearchText))
                {
                    items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                                 || (item.BriefIntroduction?.Contains(options.SearchText) ?? false));
                }
            }
            // 过滤
            /* var isFiltered = false;
             if (options.Filters.Any())
             {
                 items = items.Where(options.Filters.GetFilterFunc<Entry>());
                 isFiltered = true;
             }*/

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheEntry.GetOrAdd(typeof(Entry), key => LambdaExtensions.GetSortLambda<Entry>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
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
                    Type = item.Type
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
                models = await query.AsNoTracking().Include(s => s.Information).Include(s => s.Relevances).ToListAsync();
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



        public void UpdateEntryDataMain(Entry entry, EntryMain examine)
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
            try
            {
                //序列化附加信息列表
                //先读取词条信息
                var information = entry.Information;

                foreach (var item in examine.Information)
                {
                    var isAdd = false;

                    //遍历信息列表寻找关键词
                    foreach (var infor in information)
                    {
                        //主键值判断条件要修改
                        if (infor.DisplayName == item.DisplayName && infor.DisplayValue == item.DisplayValue)
                        {
                            //查看是否为删除操作
                            if (item.IsDelete == true)
                            {
                                information.Remove(infor);
                                isAdd = true;
                                break;
                            }
                            else
                            {
                                infor.DisplayName = item.DisplayName;
                                infor.DisplayValue = item.DisplayValue;
                                if (item.Additional != null)
                                {
                                    for (var i = 0; i < item.Additional.Count; i++)
                                    {
                                        if (item.Additional[i].IsDelete == false)
                                        {
                                            infor.Additional.Add(new BasicEntryInformationAdditional
                                            {
                                                DisplayName = item.Additional[i].DisplayName,
                                                DisplayValue = item.Additional[i].DisplayValue
                                            });
                                        }
                                        else
                                        {
                                            //如果是删除则循环查找
                                            foreach (var nal in infor.Additional)
                                            {
                                                if (nal.DisplayName == item.Additional[i].DisplayName && nal.DisplayValue == item.Additional[i].DisplayValue)
                                                {
                                                    infor.Additional.Remove(nal);
                                                    break;
                                                }
                                            }

                                        }

                                    }
                                }

                                isAdd = true;
                                break;
                            }
                        }


                    }
                    if (isAdd == false && item.IsDelete == false)
                    {
                        //没有找到关键词 则新建关键词
                        var temp = new BasicEntryInformation
                        {
                            Modifier = item.Modifier,
                            DisplayName = item.DisplayName,
                            DisplayValue = item.DisplayValue,
                            Additional = new List<BasicEntryInformationAdditional>()
                        };
                        if (item.Additional != null)
                        {
                            for (var i = 0; i < item.Additional.Count; i++)
                            {
                                if (item.Additional[i].IsDelete == false)
                                {
                                    temp.Additional.Add(new BasicEntryInformationAdditional
                                    {
                                        DisplayName = item.Additional[i].DisplayName,
                                        DisplayValue = item.Additional[i].DisplayValue
                                    });
                                }
                            }
                        }

                        information.Add(temp);
                    }


                    if (item.IsDelete == false)
                    {
                        if (entry.Type == EntryType.Game)
                        {
                            if (item.Modifier == "基本信息")
                            {
                                //查找是否修改发行时间 对下文不影响 只是更新字段缓存
                                if (item.DisplayName == "发行时间")
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
                                else if (item.DisplayName == "Steam平台Id")
                                {
                                    if (string.IsNullOrWhiteSpace(item.DisplayValue) == false && string.IsNullOrWhiteSpace(entry.MainPicture))
                                    {
                                        entry.MainPicture = "https://media.st.dl.pinyuncloud.com/steam/apps/" + item.DisplayValue + "/header.jpg";
                                    }
                                }

                            }
                        }
                    }

                }
            }
            catch
            {

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
                    if (pic.Modifier == item.Modifier && pic.Note == item.Note && pic.Url == item.Url)
                    {
                        if (item.IsDelete == true)
                        {
                            pictures.Remove(pic);

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

        public void UpdateEntryDataRelevances(Entry entry, EntryRelevancesModel examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = entry.Relevances;

            foreach (var item in examine.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.DisplayName + infor.DisplayValue == item.DisplayName + item.DisplayValue)
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
                            infor.DisplayValue = item.DisplayValue;
                            infor.DisplayName = item.DisplayName;
                            infor.Link = item.Link;
                            infor.Modifier = item.Modifier;
                            isAdd = true;
                            break;
                        }
                    }
                }
                if (isAdd == false && item.IsDelete == false)
                {
                    //没有找到关键词 则新建关键词
                    var temp = new EntryRelevance
                    {
                        Modifier = item.Modifier,
                        DisplayName = item.DisplayName,
                        DisplayValue = item.DisplayValue,
                        Link = item.Link
                    };
                    relevances.Add(temp);
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
                    EntryMain entryMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryMain = (EntryMain)serializer.Deserialize(str, typeof(EntryMain));
                    }

                    UpdateEntryDataMain(entry, entryMain);
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
                    EntryRelevancesModel entryRelevancesModel = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        entryRelevancesModel = (EntryRelevancesModel)serializer.Deserialize(str, typeof(EntryRelevancesModel));
                    }

                    UpdateEntryDataRelevances(entry, entryRelevancesModel);
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
                    UpdateEntryDataMainPage(entry, examine.Context);
                    break;
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

        /*public async Task ChangeRelevancesAfterChangeEntryName(int entryId,string oldName)
        {
            Entry entry=await _entryRepository.GetAll().Include(s=>s.Information).Include(s=>s.Relevances).FirstOrDefaultAsync(s=>s.Id== entryId);
            //目前只支持游戏
            if(entry==null||entry.Type!=EntryType.Game)
            {
                return;
            }

            List<string> relevances = new List<string>();
            string typeStr = "";
                switch (entry.Type)
            {
                case EntryType.Role:
                    typeStr = "角色";
                    break;
                case EntryType.Game:
                    typeStr = "角色";
                    break;
                case EntryType.ProductionGroup:
                    typeStr = "角色";
                    break;
                case EntryType.Staff:
                    typeStr = "角色";
                    break;
            };
            //获取该词条的所有关联项目

            //制作组 发行商
            List<string> temp=entry.Information.Where(s=>s.Modifier=="基本信息"&&(s.DisplayName=="制作组"||s.DisplayName=="发行商")).Select(s=>s.DisplayValue).ToList();
            if(temp.Any())
            {
                relevances.AddRange(temp);
            }

            //Staff
            temp = entry.Information.Where(s => s.Modifier == "STAFF").Select(s => s.DisplayValue).ToList();
            if (temp.Any())
            {
                relevances.AddRange(temp);
            }

            //关联游戏
            temp = entry.Relevances.Where(s => s.Modifier == "游戏").Select(s => s.DisplayValue).ToList();
            if (temp.Any())
            {
                relevances.AddRange(temp);
            }

            //关联制作组
            temp = entry.Relevances.Where(s => s.Modifier == "制作组").Select(s => s.DisplayValue).ToList();
            if (temp.Any())
            {
                relevances.AddRange(temp);
            }

            //关联Staff
            temp = entry.Relevances.Where(s => s.Modifier == "Staff").Select(s => s.DisplayValue).ToList();
            if (temp.Any())
            {
                relevances.AddRange(temp);
            }

            //关联角色
            temp = entry.Relevances.Where(s => s.Modifier == "角色").Select(s => s.DisplayValue).ToList();
            if (temp.Any())
            {
                relevances.AddRange(temp);
            }

            //根据temp修改

        }*/

    }
}
