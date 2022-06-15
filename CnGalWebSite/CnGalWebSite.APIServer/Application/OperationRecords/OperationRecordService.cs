using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.OperationRecords
{
    public class OperationRecordService : IOperationRecordService
    {
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;
        private readonly IConfiguration _configuration;


        public OperationRecordService(IRepository<OperationRecord, long> operationRecordRepository, IConfiguration configuration)
        {
            _operationRecordRepository = operationRecordRepository;
            _configuration = configuration;
        }

        public async Task<QueryData<ListOperationRecordAloneModel>> GetPaginatedResult(DataModel.ViewModel.Search.QueryPageOptions options, ListOperationRecordAloneModel searchModel)
        {
            var items = _operationRecordRepository.GetAll()
                .Include(s => s.ApplicationUser).AsNoTracking();

            // 处理高级搜索

            if (!string.IsNullOrWhiteSpace(searchModel.UserId))
            {
                items = items.Where(item => item.ApplicationUserId.Contains(searchModel.UserId, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.UserName))
            {
                items = items.Where(item => item.ApplicationUser.UserName.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(searchModel.ObjectId))
            {
                items = items.Where(item => item.ObjectId.Contains(searchModel.ObjectId, StringComparison.OrdinalIgnoreCase));
            }
            if (searchModel.Type != null)
            {
                items = items.Where(item => item.Type == searchModel.Type);
            }



            // 处理 SearchText 模糊搜索
            if (!string.IsNullOrWhiteSpace(options.SearchText))
            {
                items = items.Where(item => item.ObjectId.ToString().Contains(options.SearchText)
                             || item.ApplicationUserId.Contains(options.SearchText));
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {

                items = items.OrderBy(s => s.Id).Sort(options.SortName, (BootstrapBlazor.Components.SortOrder)options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            var itemsReal = await items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToListAsync();

            //复制数据
            var resultItems = new List<ListOperationRecordAloneModel>();
            foreach (var item in itemsReal)
            {
                resultItems.Add(new ListOperationRecordAloneModel
                {
                    Id = item.Id,
                    UserName = item.ApplicationUser?.UserName,
                    UserId = item.ApplicationUserId,
                    Cookie = item.Cookie,
                    Ip = item.Ip,
                    ObjectId = item.ObjectId,
                    Type = item.Type,
                    OperationTime = item.OperationTime,
                });
            }

            return new QueryData<ListOperationRecordAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            };
        }


        public async Task AddOperationRecord(OperationRecordType type, string objectId, ApplicationUser user, DeviceIdentificationModel model, HttpContext context)
        {
            if (string.IsNullOrEmpty(model.Cookie)||string.IsNullOrWhiteSpace(model.Ip))
            {
                throw new Exception("身份验证参数不能为空");
            }

            model.Ip = GetIp(context, model.Ip);

            var item = await _operationRecordRepository.FirstOrDefaultAsync(s => s.Type == type && (s.ObjectId == objectId || s.Type == OperationRecordType.Login) && s.ApplicationUserId == user.Id);
            if (item == null)
            {
                item = await _operationRecordRepository.InsertAsync(new OperationRecord
                {
                    Type = type,
                    ObjectId = objectId,
                    ApplicationUserId = user.Id
                });
            }

            item.Ip = model.Ip;
            item.Cookie = model.Cookie;
            item.OperationTime = DateTime.Now.ToCstTime();

            await _operationRecordRepository.UpdateAsync(item);
        }

        public string GetIp(HttpContext context, string userIp)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = context.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }

            if (ip == _configuration["InternalIp"])
            {
                ip = userIp ?? "";
            }

            return ip.Split(',').FirstOrDefault();
        }
    }
}
