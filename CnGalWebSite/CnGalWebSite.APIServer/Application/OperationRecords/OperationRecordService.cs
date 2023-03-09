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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.OperationRecords
{
    public partial class OperationRecordService : IOperationRecordService
    {
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;
        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, string> _ips=new Dictionary<string, string>();
        private DateTime _lastRefreshTime;

        public OperationRecordService(IRepository<OperationRecord, long> operationRecordRepository, IConfiguration configuration)
        {
            _operationRecordRepository = operationRecordRepository;
            _configuration = configuration;

            if(string.IsNullOrWhiteSpace( _configuration["IpWhitelist"])==false)
            {
               foreach(var item in  _configuration["IpWhitelist"].Split(',').Select(s => new KeyValuePair<string, string>(s.Trim(), null)))
                {
                    _ips.Add(item);
                }
            }

        }

        private static string GetHostAddresses(string howtogeek)
        {
            IPAddress[] addresslist = Dns.GetHostAddresses(howtogeek);

            return addresslist.FirstOrDefault(s=>s.AddressFamily== System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
        }

        private void RefreshIPs()
        {
            if((DateTime.Now.ToCstTime()- _lastRefreshTime).TotalMinutes<10)
            {
                return;
            }
            _lastRefreshTime = DateTime.Now.ToCstTime();

            foreach (var item in _ips)
            {
                if(IpRegex().IsMatch(item.Key))
                {
                    _ips[item.Key] = item.Key;
                }
                else
                {
                    _ips[item.Key] = GetHostAddresses(item.Key);
                }
            }
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

                items = items.OrderBy(s => s.Id).Sort(options.SortName, (SortOrder)options.SortOrder);
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

            //可以重复添加相同类型的操作记录
            await _operationRecordRepository.InsertAsync(new OperationRecord
            {
                Type = type,
                ObjectId = objectId,
                ApplicationUserId = user.Id,
                Ip = model.Ip,
                Cookie = model.Cookie,
                OperationTime = DateTime.Now.ToCstTime()
            });
        }

        public async Task<bool> CheckOperationRecord(OperationRecordType type, string objectId, ApplicationUser user, DeviceIdentificationModel model, HttpContext context)
        {
            if (string.IsNullOrEmpty(model.Cookie) || string.IsNullOrWhiteSpace(model.Ip))
            {
                throw new Exception("身份验证参数不能为空");
            }

            model.Ip = GetIp(context, model.Ip);

            return await _operationRecordRepository.GetAll().AnyAsync(s => s.Type == type && s.ObjectId == objectId && (s.Cookie == model.Cookie || s.Ip == model.Ip));
        }

        public async Task<bool> CheckOperationRecord(OperationRecordType type, string objectId, ApplicationUser user)
        {
            var item = await _operationRecordRepository.FirstOrDefaultAsync(s => s.Type == type && (s.ObjectId == objectId || s.Type == OperationRecordType.Login) && s.ApplicationUserId == user.Id);
            if(item==null)
            {
                return true;
            }

            if (string.IsNullOrEmpty(item.Cookie) || string.IsNullOrWhiteSpace(item.Ip))
            {
                throw new Exception("身份验证参数不能为空");
            }


            return await _operationRecordRepository.GetAll().CountAsync(s => s.Type == type && s.ObjectId == objectId && (s.Cookie == item.Cookie || s.Ip == item.Ip)) > 1;
        }

        public async Task CopyOperationRecord(OperationRecordType fromType, string fromObjectId, OperationRecordType toType, string toObjectId,ApplicationUser user)
        {
            var fromItem = await _operationRecordRepository.FirstOrDefaultAsync(s => s.Type == fromType && (s.ObjectId == fromObjectId || s.Type == OperationRecordType.Login) && s.ApplicationUserId == user.Id);
            var toItem = await _operationRecordRepository.FirstOrDefaultAsync(s => s.Type == toType && (s.ObjectId == toObjectId || s.Type == OperationRecordType.Login) && s.ApplicationUserId == user.Id);

            if (fromItem != null)
            {
                if (toItem == null)
                {
                    toItem = await _operationRecordRepository.InsertAsync(new OperationRecord
                    {
                        Type = toType,
                        ObjectId = toObjectId,
                        ApplicationUserId = user.Id
                    });
                }

                toItem.Ip = fromItem.Ip;
                toItem.Cookie = fromItem.Cookie;
                toItem.OperationTime = DateTime.Now.ToCstTime();

                await _operationRecordRepository.UpdateAsync(toItem);
            }
        }

        public string GetIp(HttpContext context, string userIp)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = context.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            //刷新ip
            RefreshIPs();
            //判断是否本地调用
            if (IntranetIpRegex().IsMatch(ip) || _ips.Any(s => ip.Contains(s.Value)))
            {
                ip = userIp ?? "";
            }

            return ip.Split(',').FirstOrDefault();
        }

        [GeneratedRegex("^(127\\.0\\.0\\.1)|(localhost)|(10\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3})|(172\\.((1[6-9])|(2\\d)|(3[01]))\\.\\d{1,3}\\.\\d{1,3})|(192\\.168\\.\\d{1,3}\\.\\d{1,3})$")]
        private static partial Regex IntranetIpRegex();
        [GeneratedRegex("^((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}$")]
        private static partial Regex IpRegex();
    }
}
