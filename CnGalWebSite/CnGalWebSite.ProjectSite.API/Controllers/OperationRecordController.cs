using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.OperationRecords;
using CnGalWebSite.ProjectSite.Models.ViewModels.Stalls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OperationRecordController : ControllerBase
    {
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;
        private readonly IQueryService _queryService;

        public OperationRecordController(IRepository<OperationRecord, long> operationRecordRepository, IQueryService queryService)
        {
            _operationRecordRepository = operationRecordRepository;
            _queryService = queryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<OperationRecordOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<OperationRecord, long>(_operationRecordRepository.GetAll().AsSingleQuery().Include(s => s.User), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.IP.Contains(model.SearchText) || s.Cookie.Contains(model.SearchText) || s.UA.Contains(model.SearchText) || s.User.UserName.Contains(model.SearchText)));

            return new QueryResultModel<OperationRecordOverviewModel>
            {
                Items = await items.Select(s => new OperationRecordOverviewModel
                {
                    Id = s.Id,
                    Cookie = s.Cookie,
                    IP = s.IP,
                    PageId = s.PageId,
                    PageType = s.PageType,
                    Time = s.Time,
                    Type = s.Type,
                    UserId = s.UserId,
                    UserName = s.User.UserName
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

    }
}
