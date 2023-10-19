

using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using System.Net;
using System.Text.RegularExpressions;

namespace CnGalWebSite.ProjectSite.API.Services
{
    public partial class OperationRecordService : IOperationRecordService
    {
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;
        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, string> _ips=new Dictionary<string, string>();
        private readonly IHttpContextAccessor _httpContextAccessor;

        private DateTime _lastRefreshTime;

        public OperationRecordService(IRepository<OperationRecord, long> operationRecordRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _operationRecordRepository = operationRecordRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

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

        public async Task AddOperationRecord(OperationRecordType type, long objectId, PageType pageType, long pageId, ApplicationUser user, DeviceIdentificationModel model)
        {
            if (string.IsNullOrEmpty(model.Cookie)||string.IsNullOrWhiteSpace(model.Ip))
            {
                throw new Exception("身份验证参数不能为空");
            }

            model.Ip = GetIp(model.Ip);

            //可以重复添加相同类型的操作记录
            await _operationRecordRepository.InsertAsync(new OperationRecord
            {
                Type = type,
                Cookie = model.Cookie,
                Time = DateTime.Now.ToCstTime(),
                IP = model.Ip,
                PageId = pageId,
                PageType = pageType,
                UA=model.UA,
                UserId=user.Id,
                ObjectId= objectId
            });
        }

       

        public string GetIp(string userIp)
        {
            var ip = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = _httpContextAccessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
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
