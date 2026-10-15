using CnGalWebSite.IdentityServer.Models.Sorting;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.ViewModels.Users;
using CnGalWebSite.IdentityServer.Services.Account;
using CnGalWebSite.IdentityServer.Services.Geetest;
using CnGalWebSite.IdentityServer.Services.Messages;
using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.IdentityServer.Models.ViewModels.Roles;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Records;
using CnGalWebSite.IdentityServer.Models.ViewModels.Records;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/records/[action]")]
    public class RecordAPIController
    {
        private static readonly SortMap<OperationRecord, IdentityOperationRecordSortField> ListSorting =
            new SortMap<OperationRecord, IdentityOperationRecordSortField>(IdentityOperationRecordSortField.Id, IdentityOperationRecordSortField.Id)
            .Add(IdentityOperationRecordSortField.Id, s => s.Id)
            .Add(IdentityOperationRecordSortField.Ip, s => s.Ip)
            .Add(IdentityOperationRecordSortField.Time, s => s.Time)
            .Add(IdentityOperationRecordSortField.Type, s => s.Type)
            .Add(IdentityOperationRecordSortField.UserName, s => s.ApplicationUser.UserName);


        private readonly IRepository<ApplicationDbContext, OperationRecord, long> _operationRecordRepository;
        private readonly IQueryService _queryService;
        private readonly IRepository<ApplicationDbContext, ApplicationUser, string> _userRepository;

        public RecordAPIController(IRepository<ApplicationDbContext, OperationRecord, long> operationRecordRepository, IQueryService queryService, IRepository<ApplicationDbContext, ApplicationUser, string> userRepository)
        {
            _operationRecordRepository = operationRecordRepository;
            _queryService = queryService;
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<QueryResultModel<OperationRecordOverviewModel, IdentityOperationRecordSortField>> List(QueryParameterModel<IdentityOperationRecordSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<OperationRecord, IdentityOperationRecordSortField>(_operationRecordRepository.GetAll().AsSingleQuery().Include(s => s.ApplicationUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.ApplicationUser.UserName.Contains(model.SearchText)), ListSorting);

            return new QueryResultModel<OperationRecordOverviewModel, IdentityOperationRecordSortField>
            {
                Items = await items.Select(s => new OperationRecordOverviewModel
                {
                    Id = s.Id,
                    Ip = s.Ip,
                    Time = s.Time,
                    Type = s.Type,
                    UserName = s.ApplicationUser.UserName,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
