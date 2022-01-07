using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Articles.Dtos;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Articles
{
    public class ArticleService : IArticleService
    {
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IAppHelper _appHelper;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Article>, string, BootstrapBlazor.Components.SortOrder, IEnumerable<Article>>> SortLambdaCacheArticle = new();


        public ArticleService(IAppHelper appHelper, IRepository<Article, long> articleRepository)
        {
            _articleRepository = articleRepository;
            _appHelper = appHelper;
        }

        public async Task<PagedResultDto<Article>> GetPaginatedResult(GetArticleInput input)
        {
            var query = _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "感想":
                        query = query.Where(s => s.Type == ArticleType.Tought);
                        break;
                    case "访谈":
                        query = query.Where(s => s.Type == ArticleType.Interview);
                        break;
                    case "攻略":
                        query = query.Where(s => s.Type == ArticleType.Strategy);
                        break;
                    case "动态":
                        query = query.Where(s => s.Type == ArticleType.News);
                        break;
                    case "评测":
                        query = query.Where(s => s.Type == ArticleType.Evaluation);
                        break;
                    case "周边":
                        query = query.Where(s => s.Type == ArticleType.Peripheral);
                        break;
                    case "杂谈":
                        query = query.Where(s => s.Type == ArticleType.None);
                        break;
                    case "二创":
                        query = query.Where(s => s.Type == ArticleType.Fan);
                        break;
                    case "公告":
                        query = query.Where(s => s.Type == ArticleType.Notice);
                        break;
                }
            }
            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {

                query = query.Where(s => s.Name.Contains(input.FilterText)
                  || s.MainPage.Contains(input.FilterText)
                  || s.BriefIntroduction.Contains(input.FilterText));
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<Article> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.Examines).ToListAsync();
            }
            else
            {
                models = new List<Article>();
            }


            var dtos = new PagedResultDto<Article>
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

        public Task<QueryData<ListArticleAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListArticleAloneModel searchModel)
        {
            IEnumerable<Article> items = _articleRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                items = items.Where(item => item.Name?.Contains(searchModel.Name, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.BriefIntroduction))
            {
                items = items.Where(item => item.BriefIntroduction?.Contains(searchModel.BriefIntroduction, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OriginalAuthor))
            {
                items = items.Where(item => item.OriginalAuthor?.Contains(searchModel.OriginalAuthor, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.OriginalLink))
            {
                items = items.Where(item => item.OriginalLink?.Contains(searchModel.OriginalLink, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }

            // 处理 Searchable=true 列与 SeachText 模糊搜索
          
                // 处理 SearchText 模糊搜索
                if (!string.IsNullOrWhiteSpace(options.SearchText))
                {
                    items = items.Where(item => (item.Name?.Contains(options.SearchText) ?? false)
                                 || (item.BriefIntroduction?.Contains(options.SearchText) ?? false)
                                 || (item.OriginalAuthor?.Contains(options.SearchText) ?? false)
                                 || (item.OriginalLink?.Contains(options.SearchText) ?? false));
                }
          

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheArticle.GetOrAdd(typeof(Article), key => LambdaExtensions.GetSortLambda<Article>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder) options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListArticleAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListArticleAloneModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    IsHidden = item.IsHidden,
                    BriefIntroduction = _appHelper.GetStringAbbreviation(item.BriefIntroduction, 20),
                    Priority = item.Priority,
                    Type = item.Type,
                    CreateTime = item.CreateTime,
                    LastEditTime = item.LastEditTime,
                    ReaderCount = item.ReaderCount,
                    OriginalLink = item.OriginalLink,
                    OriginalAuthor = item.OriginalAuthor,
                    PubishTime = item.PubishTime,
                    CanComment = item.CanComment??true
                    //ThumbsUpCount=item.ThumbsUps.Count()
                });
            }

            return Task.FromResult(new QueryData<ListArticleAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        public async Task<PagedResultDto<ArticleInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input)
        {
            var query = _articleRepository.GetAll().AsNoTracking().Where(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false);
            //判断是否是条件筛选
            if (!string.IsNullOrWhiteSpace(input.ScreeningConditions))
            {
                switch (input.ScreeningConditions)
                {
                    case "感想":
                        query = query.Where(s => s.Type == ArticleType.Tought);
                        break;
                    case "访谈":
                        query = query.Where(s => s.Type == ArticleType.Interview);
                        break;
                    case "攻略":
                        query = query.Where(s => s.Type == ArticleType.Strategy);
                        break;
                    case "动态":
                        query = query.Where(s => s.Type == ArticleType.News);
                        break;
                    case "评测":
                        query = query.Where(s => s.Type == ArticleType.Evaluation);
                        break;
                    case "周边":
                        query = query.Where(s => s.Type == ArticleType.Peripheral);
                        break;
                    case "杂谈":
                        query = query.Where(s => s.Type == ArticleType.None);
                        break;
                    case "二创":
                        query = query.Where(s => s.Type == ArticleType.Fan);
                        break;
                    case "公告":
                        query = query.Where(s => s.Type == ArticleType.Notice);
                        break;
                }
            }
            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.CreateUserId == input.FilterText);
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            //这个特殊方法中当前页数解释为起始位
            query = query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<Article> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.CreateUser).ToListAsync();
            }
            else
            {
                models = new List<Article>();
            }

            var dtos = new List<ArticleInforTipViewModel>();
            foreach (var item in models)
            {
                dtos.Add(_appHelper.GetArticleInforTipViewModel(item));
            }

            var dtos_ = new PagedResultDto<ArticleInforTipViewModel>
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

        public async Task<List<long>> GetArticleIdsFromNames(List<string> names)
        {
            //判断关联是否存在
            var entryId = new List<long>();

            foreach (var item in names)
            {
                var infor = await _articleRepository.GetAll().AsNoTracking().Where(s => s.Name == item).Select(s => s.Id).FirstOrDefaultAsync();
                if (infor <= 0)
                {
                    throw new Exception("文章 " + item + " 不存在");
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

        public void UpdateArticleDataMain(Article article, ArticleMain examine)
        {
            article.Name = examine.Name;
            article.DisplayName = examine.DisplayName;
            article.BriefIntroduction = examine.BriefIntroduction;
            article.MainPicture = examine.MainPicture;
            article.BackgroundPicture = examine.BackgroundPicture;
            article.Type = examine.Type;
            article.OriginalLink = examine.OriginalLink;
            article.OriginalAuthor = examine.OriginalAuthor;
            article.PubishTime = examine.PubishTime;
            article.RealNewsTime = examine.RealNewsTime;
            article.SmallBackgroundPicture = examine.SmallBackgroundPicture;
            article.NewsType = examine.NewsType;

            //更新最后编辑时间
            article.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateArticleDataRelevances(Article article, ArticleRelecancesModel examine)
        {
            //序列化相关性列表
            //先读取词条信息
            var relevances = article.Relevances;

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
                    var temp = new ArticleRelevance
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
            article.LastEditTime = DateTime.Now.ToCstTime();


        }

        public void UpdateArticleDataMainPage(Article article, string examine)
        {
            article.MainPage = examine;
            //如果主图为空 则提取主图

            //更新最后编辑时间
            article.LastEditTime = DateTime.Now.ToCstTime();

        }

        public void UpdateArticleData(Article article, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.EditArticleMain:
                    ArticleMain articleMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        articleMain = (ArticleMain)serializer.Deserialize(str, typeof(ArticleMain));
                    }

                    UpdateArticleDataMain(article, articleMain);
                    break;
                case Operation.EditArticleRelevanes:
                    ArticleRelecancesModel articleRelecancesModel = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        var serializer = new JsonSerializer();
                        articleRelecancesModel = (ArticleRelecancesModel)serializer.Deserialize(str, typeof(ArticleRelecancesModel));
                    }

                    UpdateArticleDataRelevances(article, articleRelecancesModel);
                    break;
                case Operation.EditArticleMainPage:
                    UpdateArticleDataMainPage(article, examine.Context);
                    break;
            }
        }

    }
}
