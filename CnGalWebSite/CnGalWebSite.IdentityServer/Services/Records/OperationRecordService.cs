using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Messages;
using CnGalWebSite.IdentityServer.Models.DataModels.Records;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Records
{
    public class OperationRecordService:IOperationRecordService
    {
        private readonly IRepository<ApplicationDbContext, OperationRecord, long> _operationRecordRepository;
        private readonly IHttpContextAccessor _accessor;

        public OperationRecordService(IRepository<ApplicationDbContext, OperationRecord, long> operationRecordRepository, IHttpContextAccessor accessor)
        {
            _operationRecordRepository = operationRecordRepository;
            _accessor = accessor;
        }

        public async Task AddOperationRecordAsync(string userId,OperationRecordType type )
        {
            var ip = _accessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = _accessor.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }

            await _operationRecordRepository.InsertAsync(new OperationRecord
            {
                ApplicationUserId = userId,
                Ip = ip,
                Time = DateTime.UtcNow,
                Type = type
            });
        }
    }
}
