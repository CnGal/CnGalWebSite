using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Home;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Search
{
    public class SearchService : ISearchService
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IAppHelper _appHelper;


        public SearchService(IRepository<Entry, int> entryRepository, IRepository<Article, long> articleRepository, IRepository<ApplicationUser, string> userRepository, IAppHelper appHelper)
        {
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _userRepository = userRepository;
            _appHelper = appHelper;
        }

        public async Task<PagedResultDto<SearchAloneModel>> GetPaginatedResult(GetSearchInput input)
        {
            IQueryable<Entry> query1 = null;
            IQueryable<Article> query2 = null;
            IEnumerable<ApplicationUser> query3 = null;

            //是否启用对应数据
            if (input.ScreeningConditions == "词条" || input.ScreeningConditions == "游戏" || input.ScreeningConditions == "角色" || input.ScreeningConditions == "STAFF"
                || input.ScreeningConditions == "制作组")
            {
                query1 = _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
            }
            else if (input.ScreeningConditions == "文章" || input.ScreeningConditions == "感想" || input.ScreeningConditions == "访谈" || input.ScreeningConditions == "攻略"
                || input.ScreeningConditions == "动态" || input.ScreeningConditions == "评测" || input.ScreeningConditions == "周边" || input.ScreeningConditions == "公告"
                || input.ScreeningConditions == "杂谈")
            {
                query2 = _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
            }
            else if (input.ScreeningConditions == "用户")
            {
                query3 = new List<ApplicationUser>(); // _userRepository.GetAll().AsNoTracking();
            }
            else
            {
                query1 = _entryRepository.GetAll().AsNoTracking().Include(s => s.Information)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information).ThenInclude(s => s.Additional)
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                    .Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
                query2 = _articleRepository.GetAll().AsNoTracking().Include(s => s.CreateUser).Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
                query3 = _userRepository.GetAll().AsNoTracking();
            }
            //根据数据集分别进行筛选
            var count1 = 0;
            var count2 = 0;
            var count3 = 0;
            if (query1 != null)
            {
                //判断是否是条件筛选
                if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
                {
                    switch (input.ScreeningConditions)
                    {
                        case "游戏":
                            query1 = query1.Where(s => s.Type == EntryType.Game);
                            break;
                        case "角色":
                            query1 = query1.Where(s => s.Type == EntryType.Role);
                            break;
                        case "STAFF":
                            query1 = query1.Where(s => s.Type == EntryType.Staff);
                            break;
                        case "制作组":
                            query1 = query1.Where(s => s.Type == EntryType.ProductionGroup);
                            break;

                    }
                }
                //判断输入的查询名称是否为空
                if (!string.IsNullOrWhiteSpace(input.FilterText))
                {
                    query1 = query1.Where(s => s.Name.Contains(input.FilterText)
                      || s.DisplayName.Contains(input.FilterText)
                      || s.AnotherName.Contains(input.FilterText)
                      || s.BriefIntroduction.Contains(input.FilterText)
                      || s.MainPage.Contains(input.FilterText));
                }
                count1 = query1.Count();
            }
            if (query2 != null)
            {
                //判断是否是条件筛选
                if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
                {
                    switch (input.ScreeningConditions)
                    {
                        case "感想":
                            query2 = query2.Where(s => s.Type == ArticleType.Tought);
                            break;
                        case "访谈":
                            query2 = query2.Where(s => s.Type == ArticleType.Interview);
                            break;
                        case "攻略":
                            query2 = query2.Where(s => s.Type == ArticleType.Strategy);
                            break;
                        case "动态":
                            query2 = query2.Where(s => s.Type == ArticleType.News);
                            break;
                        case "评测":
                            query2 = query2.Where(s => s.Type == ArticleType.Evaluation);
                            break;
                        case "周边":
                            query2 = query2.Where(s => s.Type == ArticleType.Peripheral);
                            break;
                        case "公告":
                            query2 = query2.Where(s => s.Type == ArticleType.Notice);
                            break;
                        case "杂谈":
                            query2 = query2.Where(s => s.Type == ArticleType.None);
                            break;
                    }
                }
                //判断输入的查询名称是否为空
                if (!string.IsNullOrWhiteSpace(input.FilterText))
                {

                    query2 = query2.Where(s => s.Name.Contains(input.FilterText)
                      || s.DisplayName.Contains(input.FilterText)
                      || s.MainPage.Contains(input.FilterText)
                      || s.BriefIntroduction.Contains(input.FilterText));
                }
                count2 = query2.Count();
            }
            if (query3 != null)
            {
                //判断输入的查询名称是否为空
                if (!string.IsNullOrWhiteSpace(input.FilterText))
                {

                    query3 = query3.Where(s => s.UserName.Contains(input.FilterText)
                      || s.MainPageContext.Contains(input.FilterText)
                      || s.PersonalSignature.Contains(input.FilterText));
                }
                count3 = query3.Count();
            }
            //根据 每个数据 的条数最大值进行分页计算
            var count = 0;
            if (count1 > count)
            {
                count = count1;
            }
            if (count2 > count)
            {
                count = count2;
            }
            if (count3 > count)
            {
                count = count3;
            }

            //根据需求进行排序，然后进行分页逻辑的计算
            var realCount1 = 0;
            var realCount2 = 0;
            var realCount3 = 0;

            //PC端分页计算
            if (input.StartIndex == -1)
            {
                if ((input.CurrentPage - 1) * input.MaxResultCount < count1)
                {

                    if ((input.CurrentPage - 1) * input.MaxResultCount + input.MaxResultCount < count1)
                    {
                        //在数据范围内
                        realCount1 = input.MaxResultCount;
                        query1 = query1.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);
                    }
                    else
                    {
                        realCount1 = count1 - (input.CurrentPage - 1) * input.MaxResultCount;
                        query1 = query1.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(realCount1);
                    }
                }
                if ((input.CurrentPage - 1) * input.MaxResultCount < count2)
                {

                    if ((input.CurrentPage - 1) * input.MaxResultCount + input.MaxResultCount < count2)
                    {
                        //在数据范围内
                        realCount2 = input.MaxResultCount;
                        query2 = query2.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);
                    }
                    else
                    {
                        realCount2 = count2 - (input.CurrentPage - 1) * input.MaxResultCount;
                        query2 = query2.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(realCount2);
                    }
                }
                if ((input.CurrentPage - 1) * input.MaxResultCount < count3)
                {
                    //更改排序字段
                    if (input.Sorting == "Name")
                    {
                    }
                    else
                    {
                    }
                    if ((input.CurrentPage - 1) * input.MaxResultCount + input.MaxResultCount < count3)
                    {
                        //在数据范围内
                        realCount3 = input.MaxResultCount;
                        // query3 = query3.OrderBy(sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);
                    }
                    else
                    {
                        realCount3 = count3 - (input.CurrentPage - 1) * input.MaxResultCount;
                        // query3 = query3.OrderBy(sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(realCount3);
                    }
                }

            }
            //手机端分页计算
            else
            {
                if (input.StartIndex < count1)
                {

                    if (input.MaxResultCount + input.StartIndex <= count1)
                    {
                        //在数据范围内
                        realCount1 = input.MaxResultCount;
                        query1 = query1.OrderBy(input.Sorting).Skip(input.StartIndex).Take(input.MaxResultCount);
                    }
                    else
                    {
                        realCount1 = count1 - input.StartIndex;
                        query1 = query1.OrderBy(input.Sorting).Skip(input.StartIndex).Take(realCount1);
                    }
                }
                if (input.StartIndex < count2)
                {

                    if (input.MaxResultCount + input.StartIndex <= count2)
                    {
                        //在数据范围内
                        realCount2 = input.MaxResultCount;
                        query2 = query2.OrderBy(input.Sorting).Skip(input.StartIndex).Take(input.MaxResultCount);
                    }
                    else
                    {
                        realCount2 = count2 - input.StartIndex;
                        query2 = query2.OrderBy(input.Sorting).Skip(input.StartIndex).Take(realCount2);
                    }
                }
                if (input.StartIndex < count3)
                {
                    //更改排序字段
                    if (input.Sorting == "Name")
                    {
                    }
                    else
                    {
                    }
                    if (input.MaxResultCount + input.StartIndex < count3)
                    {
                        //在数据范围内
                        realCount3 = input.MaxResultCount;
                        // query3 = query3.OrderBy(sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);
                    }
                    else
                    {
                        realCount3 = count3 - (input.CurrentPage - 1) * input.MaxResultCount;
                        // query3 = query3.OrderBy(sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(realCount3);
                    }
                }

            }

            //将结果转换为List集合 加载到内存中
            var models = new List<SearchAloneModel>();
            List<Entry> models1 = null;
            List<Article> models2 = null;
            List<ApplicationUser> models3 = null;
            count1 = 0;
            count2 = 0;
            count3 = 0;

            //依次加载
            if (realCount1 != 0)
            {
                models1 = await query1.AsNoTracking().Include(s => s.Tags).ToListAsync();
                count1 = models1.Count;
            }

            if (realCount2 != 0)
            {
                models2 = await query2.AsNoTracking().ToListAsync();
                count2 = models2.Count;

            }

            if (realCount3 != 0)
            {
                models3 = new List<ApplicationUser>(); //await query3.AsNoTracking().ToListAsync();
                count3 = models3.Count;

            }

            //根据结果进行插值
            for (var i = 0; i < count; i++)
            {


                if (i < count1)
                {
                    var item = models1[i];


                    models.Add(new SearchAloneModel
                    {
                        entry =await _appHelper.GetEntryInforTipViewModel(item)
                    });

                }
                if (i < count2)
                {
                    var item = models2[i];
                    item.BriefIntroduction = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 50);

                    models.Add(new SearchAloneModel
                    {
                        article = _appHelper.GetArticleInforTipViewModel(item)
                    });
                }
                if (i < count3)
                {
                    var item = models3[i];
                    item.PhotoPath = _appHelper.GetImagePath(item.PhotoPath, "user.png");

                    models.Add(new SearchAloneModel
                    {
                        user = new DataModel.ViewModel.Search.UserInforTipViewModel
                        {
                            Id = item.Id,
                            PersonalSignature = item.PersonalSignature,
                            // Email = item.Email,
                            PhotoPath = item.PhotoPath,
                            UserName = item.UserName
                        }
                    });
                }
            }



            var dtos = new PagedResultDto<SearchAloneModel>
            {
                TotalCount = input.StartIndex == -1 ? count : (count1 + count2 + count3),
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };

            return dtos;
        }
    }
}
