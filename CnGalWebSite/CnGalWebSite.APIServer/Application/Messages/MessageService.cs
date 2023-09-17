using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Users.Dtos;
using CnGalWebSite.APIServer.DataReositories;

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
    }
}
