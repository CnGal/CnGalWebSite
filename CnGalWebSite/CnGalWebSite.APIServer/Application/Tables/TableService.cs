using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Tables
{
    public class TableService : ITableService
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<SteamInfor, long> _steamInforRepository;
        private readonly IRepository<BasicInforTableModel, long> _basicInforTableModelRepository;
        private readonly IRepository<GroupInforTableModel, long> _groupInforTableModelRepository;
        private readonly IRepository<MakerInforTableModel, long> _makerInforTableModelRepository;
        private readonly IRepository<StaffInforTableModel, long> _staffInforTableModelRepository;
        private readonly IRepository<RoleInforTableModel, long> _roleInforTableModelRepository;
        private readonly IRepository<SteamInforTableModel, long> _steamInforTableModelRepository;
        private readonly IPlayedGameService _playedGameService;
        private readonly ILogger<TableService> _logger;

        public TableService(IRepository<BasicInforTableModel, long> basicInforTableModelRepository, IRepository<Entry, int> entryRepository, IPlayedGameService playedGameService,
        IRepository<GroupInforTableModel, long> groupInforTableModelRepository, IRepository<MakerInforTableModel, long> makerInforTableModelRepository,
        IRepository<StaffInforTableModel, long> staffInforTableModelRepository, IRepository<RoleInforTableModel, long> roleInforTableModelRepository, ILogger<TableService> logger,
        IRepository<SteamInforTableModel, long> steamInforTableModelRepository, IRepository<SteamInfor, long> steamInforRepository)
        {
            _entryRepository = entryRepository;
            _basicInforTableModelRepository = basicInforTableModelRepository;
            _groupInforTableModelRepository = groupInforTableModelRepository;
            _makerInforTableModelRepository = makerInforTableModelRepository;
            _staffInforTableModelRepository = staffInforTableModelRepository;
            _roleInforTableModelRepository = roleInforTableModelRepository;
            _steamInforTableModelRepository = steamInforTableModelRepository;
            _steamInforRepository = steamInforRepository;
            _playedGameService = playedGameService;
            _logger = logger;
        }

        public async Task UpdateBasicInforListAsync()
        {
            try
            {
                //通过Id获取词条

                var entries = await _entryRepository.GetAll().AsNoTracking()
                     .Include(s => s.Information).Include(s => s.Tags)
                     .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                     .Select(s => new
                     {
                         s.Id,
                         s.AnotherName,
                         s.DisplayName,
                         s.Name,
                         s.PubulishTime,
                         s.Information,
                         Tags = s.Tags.Select(s => s.Name).ToList()
                     })
                     .AsSingleQuery()
                     .ToListAsync();

                var Infors = new List<BasicInforTableModel>();
                //循环进行对应赋值
                foreach (var infor in entries)
                {
                    var tableModel = new BasicInforTableModel
                    {
                        RealId = infor.Id,
                        Name = infor.DisplayName ?? infor.Name,
                        IssueTime = infor.PubulishTime,
                        GameNickname = infor.AnotherName,
                        Tags = ""
                    };
                    //序列化标签
                    if (infor.Tags != null && infor.Tags.Count > 0)
                    {
                        for (var i = 0; i < infor.Tags.Count; i++)
                        {
                            tableModel.Tags += infor.Tags[i];

                            if (i != infor.Tags.Count - 1)
                            {
                                tableModel.Tags += "、";
                            }
                        }
                    }

                    //查找基本信息
                    foreach (var item in infor.Information)
                    {
                        if (item.Modifier == "基本信息")
                        {
                            switch (item.DisplayName)
                            {
                                case "原作":
                                    tableModel.Original = item.DisplayValue;
                                    break;
                                case "制作组":
                                    if (string.IsNullOrWhiteSpace(tableModel.ProductionGroup))
                                    {
                                        tableModel.ProductionGroup = item.DisplayValue;
                                    }
                                    break;
                                case "制作组名称":
                                    if (string.IsNullOrWhiteSpace(tableModel.ProductionGroup))
                                    {
                                        tableModel.ProductionGroup = item.DisplayValue;
                                    }
                                    break;
                                case "游戏平台":

                                    tableModel.GamePlatforms = item.DisplayValue;

                                    break;
                                case "引擎":
                                    tableModel.Engine = item.DisplayValue;
                                    break;
                                case "发行商":
                                    tableModel.Publisher = item.DisplayValue;
                                    break;
                                case "发行方式":
                                    tableModel.IssueMethod = item.DisplayValue;
                                    break;
                                case "官网":
                                    tableModel.OfficialWebsite = item.DisplayValue;
                                    break;
                                case "Steam平台Id":
                                    tableModel.SteamId = item.DisplayValue;
                                    break;
                                case "QQ群":
                                    tableModel.QQgroupGame = item.DisplayValue;
                                    break;
                            }
                        }
                    }
                    Infors.Add(tableModel);
                }

                //与数据中现有的项目对比
                //删除不存在的项目
                var currentIds = Infors.Select(s => s.RealId);

                await _basicInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
                //添加不存在的项目
                var oldIds = await _basicInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

                var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
                foreach (var item in newItems)
                {
                    await _basicInforTableModelRepository.InsertAsync(item);
                }
                //对已存在的项目进行更新
                var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
                var oldItems = await _basicInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
                foreach (var item in oldItems)
                {
                    var temp = currentItems.Find(s => s.RealId == item.RealId);
                    temp.Id = item.Id;
                    if (item.RealId != temp.RealId || item.Name != temp.Name || item.IssueTime != temp.IssueTime || item.Original != temp.Original
                        || item.ProductionGroup != temp.ProductionGroup || item.GamePlatforms != temp.GamePlatforms || item.Engine != temp.Engine
                        || item.Publisher != temp.Publisher || item.GameNickname != temp.GameNickname || item.Tags != temp.Tags || item.IssueMethod != temp.IssueMethod
                        || item.OfficialWebsite != temp.OfficialWebsite || item.SteamId != temp.SteamId || item.QQgroupGame != temp.QQgroupGame)
                    {
                        item.Name = temp.Name;
                        item.IssueTime = temp.IssueTime;
                        item.Original = temp.Original;
                        item.ProductionGroup = temp.ProductionGroup;
                        item.GamePlatforms = temp.GamePlatforms;
                        item.Engine = temp.Engine;
                        item.Publisher = temp.Publisher;
                        item.GameNickname = temp.GameNickname;
                        item.Tags = temp.Tags;
                        item.IssueMethod = temp.IssueMethod;
                        item.OfficialWebsite = temp.OfficialWebsite;
                        item.SteamId = temp.SteamId;
                        item.QQgroupGame = temp.QQgroupGame;
                        await _basicInforTableModelRepository.UpdateAsync(item);
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        public async Task UpdateGroupInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Information)
                .Where(s => s.Type == EntryType.ProductionGroup && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.IsHidden,
                    s.Name,
                    s.DisplayName,
                    s.AnotherName,
                    s.Id,
                    s.Information
                })
                .ToListAsync();
            var Infors = new List<GroupInforTableModel>();
            //循环进行对应赋值
            foreach (var infor in entries)
            {
                var tableModel = new GroupInforTableModel
                {
                    RealId = infor.Id,
                    Name = infor.DisplayName ?? infor.Name,
                    AnotherNameGroup = infor.AnotherName
                };
                foreach (var item in infor.Information)
                {
                    if (item.Modifier == "基本信息")
                    {
                        switch (item.DisplayName)
                        {
                            case "QQ群":
                                tableModel.QQgroupGroup = item.DisplayValue;
                                break;
                        }
                    }
                }
                Infors.Add(tableModel);
            }

            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _groupInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _groupInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _groupInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _groupInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;
                if (item.RealId != temp.RealId || item.Name != temp.Name || item.QQgroupGroup != temp.QQgroupGroup || item.AnotherNameGroup != temp.AnotherNameGroup)
                {
                    item.Name = temp.Name;
                    item.AnotherNameGroup = temp.AnotherNameGroup;
                    item.QQgroupGroup = temp.QQgroupGroup;

                    await _groupInforTableModelRepository.UpdateAsync(item);
                }
            }


        }

        public async Task UpdateStaffInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.EntryStaffFromEntryNavigation,
                    s.Name,
                    s.DisplayName
                })
                .ToListAsync();
            var Infors = new List<StaffInforTableModel>();
            //循环进行对应赋值
            foreach (var inEntry in entries)
            {

                foreach (var item in inEntry.EntryStaffFromEntryNavigation)
                {
                        var tableModel = new StaffInforTableModel
                        {
                            RealId = item.EntryStaffId,
                            GameName = inEntry.DisplayName ?? inEntry.Name,
                            Subcategory = item.Modifier,
                            SubordinateOrganization = item.SubordinateOrganization,
                            NicknameOfficial = string.IsNullOrWhiteSpace(item.CustomName) ? (item.ToEntryNavigation?.Name ?? item.Name) : item.CustomName,
                            PositionGeneral = item.PositionGeneral,
                            PositionOfficial = item.PositionOfficial,
                        };

                        Infors.Add(tableModel);
                    
                }

            }
            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _staffInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _staffInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _staffInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _staffInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;
                if (item.RealId != temp.RealId || item.GameName != temp.GameName || item.Subcategory != temp.Subcategory || item.PositionOfficial != temp.PositionOfficial
                    || item.NicknameOfficial != temp.NicknameOfficial || item.PositionGeneral != temp.PositionGeneral || item.SubordinateOrganization != temp.SubordinateOrganization)
                {
                    item.GameName = temp.GameName;
                    item.Subcategory = temp.Subcategory;
                    item.PositionOfficial = temp.PositionOfficial;
                    item.NicknameOfficial = temp.NicknameOfficial;
                    item.PositionGeneral = temp.PositionGeneral;
                    item.SubordinateOrganization = temp.SubordinateOrganization;

                    await _staffInforTableModelRepository.UpdateAsync(item);
                }
            }



        }

        public async Task UpdateMakerInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Information)
                .Where(s => s.Type == EntryType.Staff && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.DisplayName,
                    s.AnotherName,
                    s.Information
                })
                .ToListAsync();
            var Infors = new List<MakerInforTableModel>();
            //循环进行对应赋值
            foreach (var infor in entries)
            {
                var tableModel = new MakerInforTableModel
                {
                    RealId = infor.Id,
                    Name = infor.DisplayName ?? infor.Name,
                    AnotherName = infor.AnotherName

                };
                foreach (var item in infor.Information)
                {
                    if (item.Modifier == "基本信息")
                    {
                        switch (item.DisplayName)
                        {
                            case "昵称（官方称呼）":
                                tableModel.Nickname = item.DisplayValue;
                                break;
                        }
                    }
                    else if (item.Modifier == "相关网站")
                    {
                        switch (item.DisplayName)
                        {
                            case "Bilibili":
                                tableModel.Bilibili = item.DisplayValue;
                                break;
                            case "B站":
                                tableModel.Bilibili = item.DisplayValue;
                                break;
                            case "微博":
                                tableModel.MicroBlog = item.DisplayValue;
                                break;
                            case "博客":
                                tableModel.Blog = item.DisplayValue;
                                break;

                            case "Lofter":
                                tableModel.Lofter = item.DisplayValue;
                                break;
                            case "其他活动网站":
                                tableModel.Other = item.DisplayValue;
                                break;
                        }
                    }
                }
                Infors.Add(tableModel);
            }

            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _makerInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _makerInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _makerInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _makerInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;
                if (item.RealId != temp.RealId || item.Name != temp.Name || item.AnotherName != temp.AnotherName || item.Nickname != temp.Nickname
                    || item.Bilibili != temp.Bilibili || item.MicroBlog != temp.MicroBlog || item.Blog != temp.Blog || item.Lofter != temp.Lofter
                     || item.Other != temp.Other)
                {
                    item.Name = temp.Name;
                    item.AnotherName = temp.AnotherName;
                    item.Nickname = temp.Nickname;
                    item.MicroBlog = temp.MicroBlog;
                    item.Bilibili = temp.Bilibili;
                    item.Blog = temp.Blog;
                    item.Lofter = temp.Lofter;
                    item.Other = temp.Other;

                    await _makerInforTableModelRepository.UpdateAsync(item);
                }
            }
        }

        public async Task UpdateRoleInforListAsync()
        {
            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().AsNoTracking()
                 .Include(s => s.Information)
                 .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                 .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                 .Where(s => s.Type == EntryType.Role && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
                 .Select(s => new
                 {
                     s.Id,
                     s.Name,
                     s.DisplayName,
                     s.AnotherName,
                     s.Information,
                     s.EntryRelationFromEntryNavigation
                 })
                 .ToListAsync();
            var Infors = new List<RoleInforTableModel>();
            //循环进行对应赋值
            foreach (var infor in entries)
            {
                var tableModel = new RoleInforTableModel
                {
                    RealId = infor.Id,
                    Name = infor.DisplayName ?? infor.Name,
                    AnotherNameRole = infor.AnotherName
                };
                foreach (var item in infor.Information)
                {
                    if (item.Modifier == "基本信息")
                    {
                        switch (item.DisplayName)
                        {
                            case "声优":
                                tableModel.CV = item.DisplayValue;
                                break;
                            case "性别":
                                if (Enum.TryParse(typeof(GenderType), item.DisplayValue, true, out object gender))
                                {
                                    tableModel.Gender = (GenderType)gender;
                                }
                                else
                                {
                                    _logger.LogWarning("角色 - {name}({id}) 性别（{gender}）无法识别", infor.Name, infor.Id, item.DisplayValue);
                                }
                                break;
                            case "身材数据":
                                tableModel.FigureData = item.DisplayValue;
                                break;
                            case "身材(主观)":
                                tableModel.FigureSubjective = item.DisplayValue;
                                break;
                            case "生日":
                                try
                                {
                                    tableModel.Birthday = DateTime.ParseExact(item.DisplayValue, "M月d日", null);
                                }
                                catch
                                {

                                    tableModel.Birthday = null;
                                }
                                break;
                            case "发色":
                                tableModel.Haircolor = item.DisplayValue;
                                break;
                            case "瞳色":
                                tableModel.Pupilcolor = item.DisplayValue;
                                break;
                            case "服饰":
                                tableModel.ClothesAccessories = item.DisplayValue;
                                break;
                            case "性格":
                                tableModel.Character = item.DisplayValue;
                                break;
                            case "角色身份":
                                tableModel.RoleIdentity = item.DisplayValue;
                                break;
                            case "身高":
                                tableModel.RoleHeight = item.DisplayValue;
                                break;
                            case "血型":
                                tableModel.BloodType = item.DisplayValue;
                                break;
                            case "年龄":
                                tableModel.RoleAge = item.DisplayValue;
                                break;
                            case "兴趣":
                                tableModel.RoleTaste = item.DisplayValue;
                                break;
                        }
                    }


                }

                foreach (var nav in infor.EntryRelationFromEntryNavigation)
                {
                    var item = nav.ToEntryNavigation;
                    if (item.Type == EntryType.Game && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                    {
                        if (string.IsNullOrWhiteSpace(tableModel.GameName))
                        {
                            tableModel.GameName = item.DisplayName;
                        }
                        else
                        {
                            tableModel.GameName += ("、" + item.DisplayName);
                        }
                    }
                }
                Infors.Add(tableModel);
            }

            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = Infors.Select(s => s.RealId);

            await _roleInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.RealId) == false);
            //添加不存在的项目
            var oldIds = await _roleInforTableModelRepository.GetAll().Select(s => s.RealId).ToListAsync();

            var newItems = Infors.Where(s => oldIds.Contains(s.RealId) == false);
            foreach (var item in newItems)
            {
                await _roleInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = Infors.Where(s => oldIds.Contains(s.RealId)).ToList();
            var oldItems = await _roleInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.RealId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.RealId == item.RealId);
                temp.Id = item.Id;

                if (item.RealId != temp.RealId || item.Name != temp.Name || item.CV != temp.CV || item.AnotherNameRole != temp.AnotherNameRole
                    || item.Gender != temp.Gender || item.FigureData != temp.FigureData || item.FigureSubjective != temp.FigureSubjective || item.Birthday != temp.Birthday
                    || item.Haircolor != temp.Haircolor || item.Pupilcolor != temp.Pupilcolor || item.ClothesAccessories != temp.ClothesAccessories || item.Character != temp.Character
                              || item.RoleIdentity != temp.RoleIdentity || item.BloodType != temp.BloodType || item.RoleHeight != temp.RoleHeight || item.RoleTaste != temp.RoleTaste
                     || item.RoleAge != temp.RoleAge || item.GameName != temp.GameName)
                {
                    item.Name = temp.Name;
                    item.CV = temp.CV;
                    item.AnotherNameRole = temp.AnotherNameRole;
                    item.Gender = temp.Gender;
                    item.FigureData = temp.FigureData;
                    item.FigureSubjective = temp.FigureSubjective;
                    item.Birthday = temp.Birthday;
                    item.Haircolor = temp.Haircolor;
                    item.ClothesAccessories = temp.ClothesAccessories;
                    item.Pupilcolor = temp.Pupilcolor;
                    item.Character = temp.Character;
                    item.RoleIdentity = temp.RoleIdentity;
                    item.BloodType = temp.BloodType;
                    item.RoleHeight = temp.RoleHeight;
                    item.RoleTaste = temp.RoleTaste;
                    item.RoleAge = temp.RoleAge;
                    item.GameName = temp.GameName;

                    await _roleInforTableModelRepository.UpdateAsync(item);
                }
            }

        }

        public async Task UpdateSteamInforListAsync()
        {
            //循环添加
            var steamList = await _steamInforRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s=>s.Entry.IsHidden==false&&string.IsNullOrWhiteSpace(s.Entry.Name)==false)
                .Select(s => new SteamInforTableModel
                {
                    SteamId = s.SteamId,
                    CutLowest = s.CutLowest,
                    CutNow = s.CutNow,
                    EntryId = s.Entry.Id,
                    EvaluationCount = s.EvaluationCount,
                    LowestTime = s.LowestTime,
                    Name = s.Entry.Name,
                    OriginalPrice = s.OriginalPrice,
                    PriceLowest = s.PriceLowest,
                    PriceNow = s.PriceNow,
                    RecommendationRate = s.RecommendationRate
                })
                .ToListAsync();

            //与数据中现有的项目对比
            //删除不存在的项目
            var currentIds = steamList.Select(s => s.SteamId);

            await _steamInforTableModelRepository.DeleteRangeAsync(s => currentIds.Contains(s.SteamId) == false);
            //添加不存在的项目
            var oldIds = await _steamInforTableModelRepository.GetAll().Select(s => s.SteamId).ToListAsync();

            var newItems = steamList.Where(s => oldIds.Contains(s.SteamId) == false);
            foreach (var item in newItems)
            {
                await _steamInforTableModelRepository.InsertAsync(item);
            }
            //对已存在的项目进行更新
            var currentItems = steamList.Where(s => oldIds.Contains(s.SteamId)).ToList();
            var oldItems = await _steamInforTableModelRepository.GetAll().Where(s => oldIds.Contains(s.SteamId)).ToListAsync();
            foreach (var item in oldItems)
            {
                var temp = currentItems.Find(s => s.SteamId == item.SteamId);
                temp.Id = item.Id;

                if (item.Name != temp.Name || item.EntryId != temp.EntryId || item.OriginalPrice != temp.OriginalPrice
                    || item.PriceNow != temp.PriceNow || item.CutNow != temp.CutNow || item.PriceLowest != temp.PriceLowest || item.CutLowest != temp.CutLowest
                    || item.LowestTime != temp.LowestTime || item.EvaluationCount != temp.EvaluationCount || item.RecommendationRate != temp.RecommendationRate)
                {
                    item.Name = temp.Name;
                    item.EntryId = temp.EntryId;
                    item.OriginalPrice = temp.OriginalPrice;
                    item.PriceNow = temp.PriceNow;
                    item.CutNow = temp.CutNow;
                    item.PriceLowest = temp.PriceLowest;
                    item.CutLowest = temp.CutLowest;
                    item.LowestTime = temp.LowestTime;
                    item.EvaluationCount = temp.EvaluationCount;
                    item.RecommendationRate = temp.RecommendationRate;

                    await _steamInforTableModelRepository.UpdateAsync(item);
                }
            }
        }

        public async Task UpdateAllInforListAsync()
        {
            await UpdateBasicInforListAsync();
            await UpdateGroupInforListAsync();
            await UpdateMakerInforListAsync();
            await UpdateRoleInforListAsync();
            await UpdateStaffInforListAsync();
            await UpdateSteamInforListAsync();
            await _playedGameService.UpdateAllGameScore();
        }

        public async Task<EChartsTreeMapOptionModel> GetGroupGameRoleTreeMap()
        {
           

            //获取所有制作组
            var groups = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s => s.Type == EntryType.ProductionGroup && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden == false && s.EntryRelationFromEntryNavigation.Any())
                .Select(s => new
                {
                    s.Id,
                    s.DisplayName,
                    Entries = s.EntryRelationFromEntryNavigation.Where(s => s.ToEntryNavigation.Type == EntryType.Game).Select(s => s.ToEntryNavigation)
                })
                .ToListAsync();

            //获取所有游戏
            var gameIds = new List<int>();
            groups.ForEach(s => gameIds.AddRange(s.Entries.Select(x => x.Id)));

            var games = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s =>gameIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.DisplayName,
                    Entries = s.EntryRelationFromEntryNavigation.Where(s=>s.ToEntryNavigation.Type== EntryType.Role).Select(s => s.ToEntryNavigation)
                })
                .ToListAsync();

            //获取所有角色
            var roleIds = new List<int>();
            games.ForEach(s => roleIds.AddRange(s.Entries.Select(x => x.Id)));

            var roles = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => roleIds.Contains(s.Id))
                .Select(s => new
                {
                    s.Id,
                    s.DisplayName,
                })
                .ToListAsync();

            //依次遍历制作组游戏角色 生成图表数据

            var roleDatas = new List<EChartsTreeMapOptionSeryDataChildren>();
            roles.ForEach(s => roleDatas.Add(new EChartsTreeMapOptionSeryDataChildren
            {
                Name = s.DisplayName,
                Value = 1,
                Id = s.Id
            }));

            var gameDatas = new List<EChartsTreeMapOptionSeryDataChildren>();
            games.ForEach(s => gameDatas.Add(new EChartsTreeMapOptionSeryDataChildren
            {
                Name = s.DisplayName,
                Value = s.Entries.Count(),
                Id=s.Id,
                Children = roleDatas.Where(x => s.Entries.Select(s => s.Id).Contains(x.Id)).ToList()
            }));

            var groupDatas = new List<EChartsTreeMapOptionSeryData>();
            groups.ForEach(s => groupDatas.Add(new EChartsTreeMapOptionSeryData
            {
                Name = s.DisplayName,
                Children = gameDatas.Where(x => s.Entries.Select(s => s.Id).Contains(x.Id)).ToList()
            }));
            groupDatas.ForEach(s => s.Value = s.Children.Sum(s => s.Value));

            return new EChartsTreeMapOptionModel
            {
                Series = new List<EChartsTreeMapOptionSery>
              {
                  new EChartsTreeMapOptionSery
                  {
                      Data=groupDatas,
                      Name="制作组"
                  }
              }
            };

        }
    }
}
