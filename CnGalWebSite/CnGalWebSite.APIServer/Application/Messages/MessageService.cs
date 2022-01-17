using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Users.Dtos;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Markdig;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Messages
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<DataModel.Model.Message, long> _messageRepository;
        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<DataModel.Model.Message>, string, SortOrder, IEnumerable<DataModel.Model.Message>>> SortLambdaCache = new();

        public MessageService(IRepository<DataModel.Model.Message, long> messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<PagedResultDto<DataModel.Model.Message>> GetPaginatedResult(GetMessageInput input, string userId)
        {
            var query = _messageRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == userId);

            //判断输入的查询名称是否为空
            /* if (!string.IsNullOrWhiteSpace(input.FilterText))
             {
                 query = query.Where(s => s.UserName.Contains(input.FilterText)
                   || s.MainPageContext.Contains(input.FilterText)
                   || s.PersonalSignature.Contains(input.FilterText));
             }*/
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            if (input.IsVisual)
            {
                query = query.OrderBy(input.Sorting).Skip(input.CurrentPage).Take(input.MaxResultCount);

            }
            else
            {
                query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            }

            //将结果转换为List集合 加载到内存中
            List<DataModel.Model.Message> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().ToListAsync();
            }
            else
            {
                models = new List<DataModel.Model.Message>();
            }

            foreach (var item in models)
            {
                //提前将MarkDown语法转为Html

                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
                item.Text = Markdig.Markdown.ToHtml(item.Text ?? "", pipeline);
            }

            var dtos = new PagedResultDto<DataModel.Model.Message>
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

        public Task<QueryData<ListMessageAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListMessageAloneModel searchModel)
        {
            var items = (IEnumerable<DataModel.Model.Message>)_messageRepository.GetAll().AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.Text))
            {
                items = items.Where(item => item.Text?.Contains(searchModel.Text, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.ApplicationUserId))
            {
                items = items.Where(item => item.ApplicationUserId?.Contains(searchModel.ApplicationUserId, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Title))
            {
                items = items.Where(item => item.Title?.Contains(searchModel.Title, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Rank))
            {
                items = items.Where(item => item.Rank?.Contains(searchModel.Rank, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.AdditionalInfor))
            {
                items = items.Where(item => item.AdditionalInfor?.Contains(searchModel.AdditionalInfor, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.LinkTitle))
            {
                items = items.Where(item => item.LinkTitle?.Contains(searchModel.LinkTitle, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Image))
            {
                items = items.Where(item => item.Image?.Contains(searchModel.Image, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Link))
            {
                items = items.Where(item => item.Link?.Contains(searchModel.Link, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }


            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => (item.Text?.Contains(options.SearchText) ?? false)
                             || (item.ApplicationUserId?.Contains(options.SearchText) ?? false)
                             || (item.Title?.Contains(options.SearchText) ?? false)
                             || (item.Rank?.Contains(options.SearchText) ?? false)
                             || (item.AdditionalInfor?.Contains(options.SearchText) ?? false)
                             || (item.LinkTitle?.Contains(options.SearchText) ?? false)
                             || (item.Image?.Contains(options.SearchText) ?? false)
                             || (item.Link?.Contains(options.SearchText) ?? false));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCache.GetOrAdd(typeof(DataModel.Model.Message), key => LambdaExtensions.GetSortLambda<DataModel.Model.Message>().Compile());
                items = invoker(items, options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            var resultItems = new List<ListMessageAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListMessageAloneModel
                {
                    Id = item.Id,
                    Type = item.Type,
                    PostTime = item.PostTime,
                    Text = item.Text,
                    Title = item.Title,
                    ApplicationUserId = item.ApplicationUserId,
                    Rank = item.Rank,
                    Link = item.Link,
                    AdditionalInfor = item.AdditionalInfor,
                    LinkTitle = item.LinkTitle,
                    Image = item.Image,
                    IsReaded = item.IsReaded
                });
            }

            return Task.FromResult(new QueryData<ListMessageAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }
    }
}
