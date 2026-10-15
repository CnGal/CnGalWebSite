using CnGalWebSite.ProjectSite.Models.Sorting;
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
        private static readonly SortMap<OperationRecord, ProjectOperationRecordSortField> ListSorting =
            new SortMap<OperationRecord, ProjectOperationRecordSortField>(ProjectOperationRecordSortField.Id, ProjectOperationRecordSortField.Id)
            .Add(ProjectOperationRecordSortField.Id, s => s.Id)
            .Add(ProjectOperationRecordSortField.Cookie, s => s.Cookie)
            .Add(ProjectOperationRecordSortField.IP, s => s.IP)
            .Add(ProjectOperationRecordSortField.PageId, s => s.PageId)
            .Add(ProjectOperationRecordSortField.PageType, s => s.PageType)
            .Add(ProjectOperationRecordSortField.Time, s => s.Time)
            .Add(ProjectOperationRecordSortField.Type, s => s.Type)
            .Add(ProjectOperationRecordSortField.UserId, s => s.UserId)
            .Add(ProjectOperationRecordSortField.UserName, s => s.User.UserName);


        private readonly IRepository<OperationRecord, long> _operationRecordRepository;
        private readonly IQueryService _queryService;

        public OperationRecordController(IRepository<OperationRecord, long> operationRecordRepository, IQueryService queryService)
        {
            _operationRecordRepository = operationRecordRepository;
            _queryService = queryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<OperationRecordOverviewModel, ProjectOperationRecordSortField>> List(QueryParameterModel<ProjectOperationRecordSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<OperationRecord, ProjectOperationRecordSortField>(_operationRecordRepository.GetAll().AsSingleQuery().Include(s => s.User), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.IP.Contains(model.SearchText) || s.Cookie.Contains(model.SearchText) || s.UA.Contains(model.SearchText) || s.User.UserName.Contains(model.SearchText)), ListSorting);

            return new QueryResultModel<OperationRecordOverviewModel, ProjectOperationRecordSortField>
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
