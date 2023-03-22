using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.Clients;
using CnGalWebSite.IdentityServer.Models.ViewModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.Shared;
using CnGalWebSite.IdentityServer.Models.ViewModels.Users;
using CnGalWebSite.IdentityServer.Services.Account;
using CnGalWebSite.IdentityServer.Services.Geetest;
using CnGalWebSite.IdentityServer.Services.Messages;
using CnGalWebSite.IdentityServer.Services.Shared;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin,User")]
    [ApiController]
    [Route("api/examines/[action]")]
    public class ExamineAPIController : ControllerBase
    {
        private readonly IRepository<ApplicationDbContext, Examine, string> _examineRepository;
        private readonly IQueryService _queryService;

        public ExamineAPIController(IRepository<ApplicationDbContext, Examine, string> examineRepository, IQueryService queryService)
        {
            _examineRepository = examineRepository;
            _queryService = queryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<ExamineOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Examine, string>(_examineRepository.GetAll().Include(s=>s.ApplicationUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.PassedAdminName.Contains(model.SearchText)));

            return new QueryResultModel<ExamineOverviewModel>
            {
                Items = await items.Select(s => new ExamineOverviewModel
                {
                    ApplyTime = s.ApplyTime,
                    IsPassed = s.IsPassed,
                    PassedAdminName = s.PassedAdminName,
                    PassedTime = s.PassedTime,
                    Type = s.Type,
                    UserName=s.ApplicationUser.UserName,
                    Id = s.Id,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
