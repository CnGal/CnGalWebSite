using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Peripheries
{
    public class PeripheryService : IPeripheryService
    {
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<PeripheryRelation, long> _peripheryRelationRepository;
        private readonly IRepository<PeripheryRelevanceEntry, long> _peripheryRelevanceEntryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IAppHelper _appHelper;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Periphery>, string, SortOrder, IEnumerable<Periphery>>> SortLambdaCachePeriphery = new();

        public PeripheryService(IAppHelper appHelper, IRepository<Periphery, long> peripheryRepository, IRepository<Examine, long> examineRepository,
            IRepository<PeripheryRelevanceEntry, long> peripheryRelevanceEntryRepository, IRepository<PeripheryRelation, long> peripheryRelationRepository)
        {
            _peripheryRepository = peripheryRepository;
            _appHelper = appHelper;
            _examineRepository = examineRepository;
            _peripheryRelevanceEntryRepository = peripheryRelevanceEntryRepository;
            _peripheryRelationRepository = peripheryRelationRepository;
        }

        public Task<QueryData<ListPeripheryAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListPeripheryAloneModel searchModel)
        {
            IEnumerable<Periphery> items = _peripheryRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Author))
            {
                items = items.Where(item => item.Author?.Contains(searchModel.Author, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Material))
            {
                items = items.Where(item => item.Material?.Contains(searchModel.Material, StringComparison.OrdinalIgnoreCase) ?? false);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                             || (item.BriefIntroduction?.Contains(options.SearchText) ?? false)
                             || (item.Author?.Contains(options.SearchText) ?? false)
                             || (item.Material?.Contains(options.SearchText) ?? false));
            }


            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCachePeriphery.GetOrAdd(typeof(Periphery), key => LambdaExtensions.GetSortLambda<Periphery>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListPeripheryAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListPeripheryAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    IsHidden = item.IsHidden,
                    BriefIntroduction = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20),
                    Priority = item.Priority,
                    LastEditTime = item.LastEditTime,
                    ReaderCount = item.ReaderCount,
                    CanComment = item.CanComment ?? true,
                    Author = item.Author,
                    CollectedCount = item.CollectedCount,
                    CommentCount = item.CommentCount,
                    Material = item.Material,
                });
            }

            return Task.FromResult(new QueryData<ListPeripheryAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task<List<long>> GetPeripheryIdsFromNames(List<string> names)
        {
            //判断关联是否存在
            var entryId = new List<long>();

            foreach (var item in names)
            {
                var infor = await _peripheryRepository.GetAll().AsNoTracking().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    throw new Exception("周边 " + item + " 不存在");
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

        public void UpdatePeripheryDataMain(Periphery periphery, PeripheryMain examine)
        {
            periphery.Name = examine.Name;
            periphery.DisplayName = examine.DisplayName;
            periphery.BriefIntroduction = examine.BriefIntroduction;
            periphery.MainPicture = examine.MainPicture;
            periphery.Thumbnail = examine.Thumbnail;
            periphery.BackgroundPicture = examine.BackgroundPicture;
            periphery.SmallBackgroundPicture = examine.SmallBackgroundPicture;
            periphery.Author = examine.Author;
            periphery.Material = examine.Material;
            periphery.Type = examine.Type;
            periphery.Size = examine.Size;
            periphery.Brand = examine.Brand;
            periphery.IndividualParts = examine.IndividualParts;
            periphery.Price = examine.Price;
            periphery.IsAvailableItem = examine.IsAvailableItem;
            periphery.IsReprint = examine.IsReprint;
            periphery.PageCount = examine.PageCount;
            periphery.SongCount = examine.SongCount;
            periphery.Category = examine.Category;
            periphery.SaleLink = examine.SaleLink;

            //更新最后编辑时间
            periphery.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdatePeripheryDataImages(Periphery periphery, PeripheryImages examine)
        {
            //序列化图片列表
            //先读取词条信息
            var pictures = periphery.Pictures;

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
            periphery.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdatePeripheryDataRelatedEntries(Periphery periphery, PeripheryRelatedEntries examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = periphery.Entries;

            foreach (var item in examine.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.EntryId == item.EntryId)
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
                    //没有找到关键词 则新建关键词
                    var temp = new PeripheryRelevanceEntry
                    {
                        EntryId = item.EntryId,
                    };
                    relevances.Add(temp);
                }
            }

            if (relevances.Any() == false)
            {
                throw new Exception("周边至少需要关联一个词条");
            }

            //更新最后编辑时间
            periphery.LastEditTime = DateTime.Now.ToCstTime();
        }

        public async Task RealUpdateRelevances(Periphery periphery)
        {
            //查找新添加的关联词条
            var now = periphery.Entries.Select(s => s.EntryId).ToList();
            var old = await _peripheryRelevanceEntryRepository.GetAll().Where(s => s.PeripheryId == periphery.Id).Select(s => s.EntryId).ToListAsync();

            var removeIds = old.Where(s => now.Contains(s) == false).ToList();
            await _peripheryRelevanceEntryRepository.DeleteAsync(s => removeIds.Contains(s.EntryId) && s.PeripheryId == periphery.Id);

            var newAddIds = now.Where(s => old.Contains(s) == false).ToList();
            foreach (var item in newAddIds)
            {
                await _peripheryRelevanceEntryRepository.InsertAsync(new PeripheryRelevanceEntry
                {
                    EntryId = item,
                    PeripheryId = periphery.Id,
                });
            }
        }

        public async Task UpdatePeripheryDataRelatedPeripheriesAsync(Periphery periphery, PeripheryRelatedPeripheries examine)
        {

            //序列化相关性列表 From
            //先读取周边信息
            var relevances = periphery.PeripheryRelationFromPeripheryNavigation;

            foreach (var item in examine.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.ToPeriphery == item.PeripheryId)
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
                    //查找周边
                    var peripheryNew = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == item.PeripheryId);
                    if (peripheryNew != null)
                    {
                        relevances.Add(new PeripheryRelation
                        {
                            FromPeriphery = periphery.Id,
                            FromPeripheryNavigation = periphery,
                            ToPeriphery = peripheryNew.Id,
                            ToPeripheryNavigation = peripheryNew
                        });
                    }
                }
            }
            periphery.PeripheryRelationFromPeripheryNavigation = relevances;


            //序列化相关性列表 To
            //先读取周边信息
            relevances = periphery.PeripheryRelationToPeripheryNavigation;

            foreach (var item in examine.Relevances)
            {
                var isAdd = false;

                //遍历信息列表寻找关键词
                foreach (var infor in relevances)
                {

                    if (infor.FromPeriphery == item.PeripheryId)
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
                    //查找周边
                    var peripheryNew = periphery.PeripheryRelationFromPeripheryNavigation.FirstOrDefault(s => s.ToPeriphery == item.PeripheryId).ToPeripheryNavigation;
                    if (peripheryNew != null)
                    {
                        relevances.Add(new PeripheryRelation
                        {
                            ToPeriphery = periphery.Id,
                            ToPeripheryNavigation = periphery,
                            FromPeriphery = peripheryNew.Id,
                            FromPeripheryNavigation = peripheryNew
                        });
                    }
                }
            }
            periphery.PeripheryRelationToPeripheryNavigation = relevances;

            //更新最后编辑时间
            periphery.LastEditTime = DateTime.Now.ToCstTime();
        }


        public async void UpdatePeripheryData(Periphery periphery, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.EditPeripheryMain:
                    PeripheryMain peripheryMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        peripheryMain = (PeripheryMain)serializer.Deserialize(str, typeof(PeripheryMain));
                    }

                    UpdatePeripheryDataMain(periphery, peripheryMain);
                    break;

                case Operation.EditPeripheryImages:
                    PeripheryImages peripheryImages = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        peripheryImages = (PeripheryImages)serializer.Deserialize(str, typeof(PeripheryImages));
                    }

                    UpdatePeripheryDataImages(periphery, peripheryImages);
                    break;
                case Operation.EditPeripheryRelatedEntries:
                    PeripheryRelatedEntries peripheryRelevances = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        peripheryRelevances = (PeripheryRelatedEntries)serializer.Deserialize(str, typeof(PeripheryRelatedEntries));
                    }

                    UpdatePeripheryDataRelatedEntries(periphery, peripheryRelevances);
                    break;
                case Operation.EditPeripheryRelatedPeripheries:
                    PeripheryRelatedPeripheries peripheryRelatedPeripheries = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        peripheryRelatedPeripheries = (PeripheryRelatedPeripheries)serializer.Deserialize(str, typeof(PeripheryRelatedPeripheries));
                    }

                    await UpdatePeripheryDataRelatedPeripheriesAsync(periphery, peripheryRelatedPeripheries);
                    break;
            }
        }

        public GameOverviewPeripheriesModel GetGameOverViewPeripheriesModel(ApplicationUser user, Entry entry, List<long> ownedPeripheries, bool showUserPhoto)
        {
            var model = new GameOverviewPeripheriesModel();
            //获取周边列表
            foreach (var item in entry.Peripheries.Where(s => s.Periphery.IsHidden == false))
            {
                model.Peripheries.Add(new PeripheryOverviewModel
                {
                    Id = item.PeripheryId ?? 0,
                    Image = _appHelper.GetImagePath(item.Periphery.MainPicture, "app.png"),
                    Name = item.Periphery.DisplayName ?? item.Periphery.Name,
                    CollectedCount = item.Periphery.CollectedCount,
                });
            }

            //判断是否收集
            if (user != null)
            {
                foreach (var item in ownedPeripheries)
                {
                    var temp = model.Peripheries.FirstOrDefault(s => s.Id == item);
                    if (temp != null)
                    {
                        temp.IsCollected = true;
                    }
                }
            }

            //获取用户信息
            if (showUserPhoto)
            {
                model.Image = _appHelper.GetImagePath(user?.PhotoPath, "user.png");
                model.Name = user?.UserName ?? "登入后记录周边收集进度";
                model.UserId = user?.Id;
                model.Type = PeripheryOverviewHeadType.User;
            }

            //获取词条信息
            if (showUserPhoto == false)
            {
                model.Image = _appHelper.GetImagePath((entry.Type == EntryType.Game || entry.Type == EntryType.ProductionGroup) ? entry.MainPicture : entry.Thumbnail, (entry.Type == EntryType.Game || entry.Type == EntryType.ProductionGroup) ? "app.png" : "user.png");
                model.Name = entry.DisplayName ?? entry.Name;
                model.Type = (entry.Type == EntryType.Game || entry.Type == EntryType.ProductionGroup) ? PeripheryOverviewHeadType.GameOrGroup : PeripheryOverviewHeadType.RoleOrStaff;
            }
            model.EntryId = entry.Id;

            return model;
        }

        public GameOverviewPeripheriesModel GetGameOverViewPeripheriesModel(ApplicationUser user, Periphery periphery, List<long> ownedPeripheries, bool showUserPhoto)
        {
            var model = new GameOverviewPeripheriesModel();
            //获取周边列表
            foreach (var item in periphery.PeripheryRelationFromPeripheryNavigation.Select(s => s.ToPeripheryNavigation).Where(s => s.IsHidden == false))
            {
                model.Peripheries.Add(new PeripheryOverviewModel
                {
                    Id = item.Id,
                    Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    Name = item.DisplayName ?? item.Name,
                    CollectedCount = item.CollectedCount,
                });
            }

            //判断是否收集
            if (user != null)
            {
                foreach (var item in ownedPeripheries)
                {
                    var temp = model.Peripheries.FirstOrDefault(s => s.Id == item);
                    if (temp != null)
                    {
                        temp.IsCollected = true;
                    }
                }
            }

            //获取用户信息
            if (showUserPhoto)
            {
                model.Image = _appHelper.GetImagePath(user?.PhotoPath, "user.png");
                model.Name = user?.UserName ?? "登入后记录周边收集进度";
                model.UserId = user?.Id;
                model.Type = PeripheryOverviewHeadType.User;
            }

            //获取主周边信息信息
            if (showUserPhoto == false)
            {
                model.Image = _appHelper.GetImagePath(periphery.MainPicture, "app.png");
                model.Name = periphery.DisplayName ?? periphery.Name;
                model.Type = PeripheryOverviewHeadType.Periphery;
            }
            model.PeripheryId = periphery.Id;

            return model;
        }


        public async Task<PeripheryEditState> GetPeripheryEditState(ApplicationUser user, long peripheryId)
        {
            var model = new PeripheryEditState();
            //获取该词条的各部分编辑状态
            //读取审核信息
            List<Examine> examineQuery = null;
            if (user != null)
            {
                examineQuery = await _examineRepository.GetAll().AsNoTracking()
                               .Where(s => s.PeripheryId == peripheryId && s.ApplicationUserId == user.Id && s.IsPassed == null
                               && (s.Operation == Operation.EditPeripheryMain || s.Operation == Operation.EditPeripheryImages || s.Operation == Operation.EditPeripheryRelatedEntries || s.Operation == Operation.EditPeripheryRelatedPeripheries))
                               .Select(s => new Examine
                               {
                                   Operation = s.Operation
                               })
                               .ToListAsync();
            }

            if (user != null)
            {
                if (examineQuery.Any(s => s.Operation == Operation.EditPeripheryMain))
                {
                    model.MainState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditPeripheryImages))
                {
                    model.ImagesState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditPeripheryRelatedEntries))
                {
                    model.RelatedEntriesState = EditState.Preview;
                }
                if (examineQuery.Any(s => s.Operation == Operation.EditPeripheryRelatedPeripheries))
                {
                    model.RelatedPeripheriesState = EditState.Preview;
                }

            }
            //获取各部分状态
            var examiningList = new List<Operation>();
            if (user != null)
            {
                examiningList = await _examineRepository.GetAll().Where(s => s.PeripheryId == peripheryId && s.ApplicationUserId != user.Id && s.IsPassed == null).Select(s => s.Operation).ToListAsync();

            }
            if (user != null)
            {
                if (model.MainState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryMain))
                    {
                        model.MainState = EditState.locked;
                    }
                    else
                    {
                        model.MainState = EditState.Normal;
                    }
                }
                if (model.ImagesState != EditState.Preview)
                {

                    if (examiningList.Any(s => s == Operation.EditPeripheryImages))
                    {
                        model.ImagesState = EditState.locked;
                    }
                    else
                    {
                        model.ImagesState = EditState.Normal;
                    }
                }
                if (model.RelatedEntriesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryRelatedEntries))
                    {
                        model.RelatedEntriesState = EditState.locked;
                    }
                    else
                    {
                        model.RelatedEntriesState = EditState.Normal;
                    }
                }
                if (model.RelatedPeripheriesState != EditState.Preview)
                {
                    if (examiningList.Any(s => s == Operation.EditPeripheryRelatedPeripheries))
                    {
                        model.RelatedPeripheriesState = EditState.locked;
                    }
                    else
                    {
                        model.RelatedPeripheriesState = EditState.Normal;
                    }
                }
            }

            return model;
        }
    }
}
