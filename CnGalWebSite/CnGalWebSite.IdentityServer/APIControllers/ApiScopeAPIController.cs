using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Clients;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.ApiScopes;
using CnGalWebSite.IdentityServer.Models.ViewModels.Clients;
using CnGalWebSite.IdentityServer.Models.ViewModels.Roles;
using CnGalWebSite.IdentityServer.Models.ViewModels.Shared;
using CnGalWebSite.IdentityServer.Services.Examines;
using CnGalWebSite.IdentityServer.Services.Shared;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;
using ApiScope = IdentityServer4.EntityFramework.Entities.ApiScope;
using Client = IdentityServer4.EntityFramework.Entities.Client;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/apiscopes/[action]")]
    public class ApiScopeAPIController : ControllerBase
    {
        private readonly IRepository<ConfigurationDbContext, ApiScope, string> _apiScopeRepository;
        private readonly IQueryService _queryService;
        private readonly IRepository<ApplicationDbContext, ApplicationUser, string> _userRepository;

        public ApiScopeAPIController(IRepository<ConfigurationDbContext, ApiScope, string> apiScopeRepository,IQueryService queryService, IRepository<ApplicationDbContext, ApplicationUser, string> userRepository)
        {
            _apiScopeRepository = apiScopeRepository;
            _queryService = queryService;
            _userRepository = userRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<KeyValuePair<string, string>>> All()
        {
            return await _apiScopeRepository.GetAll().Select(s => new KeyValuePair<string, string>(s.Name, s.DisplayName)).ToListAsync();
        }

        [HttpPost]
        public async Task<QueryResultModel<ApiScopeOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ApiScope, string>(_apiScopeRepository.GetAll(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.DisplayName.Contains(model.SearchText) || s.Description.Contains(model.SearchText)));

            return new QueryResultModel<ApiScopeOverviewModel>
            {
                Items = await items.Select(s => new ApiScopeOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    DisplayName = s.DisplayName,
                    Enabled = s.Enabled,
                    Required = s.Required,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpGet]
        public async Task<ActionResult<ApiScopeEditModel>> Edit(int id)
        {
            var scope = await _apiScopeRepository.GetAll()
              .Include(s => s.UserClaims)
              .FirstOrDefaultAsync(s => s.Id == id);

            if (scope == null)
            {
                return NotFound("找不到ApiScope");
            }

            var model = new ApiScopeEditModel
            {
                Description = scope.Description,
                DisplayName = scope.DisplayName,
                Enabled = scope.Enabled,
                Id = scope.Id,
                Name = scope.Name,
                Required = scope.Required,
                UserClaims = scope.UserClaims.Select(s => s.Type).ToList(),
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> Edit(ApiScopeEditModel model)
        {

            ApiScope scope = null;
            if (model.Id == 0)
            {
                scope = await _apiScopeRepository.InsertAsync(new ApiScope {
                    Name = model.Name,
                    DisplayName= model.DisplayName,
                    Description = model.Description,
                    Enabled = model.Enabled,
                    Required = model.Required,
                });
                model.Id = scope.Id;
                _apiScopeRepository.Clear();
            }

            scope = await _apiScopeRepository.GetAll()
                .Include(s => s.UserClaims)
                .FirstOrDefaultAsync(s => s.Id == model.Id);


            if (scope == null)
            {
                return new Result { Success = false, Message = "ApiScope不存在" };
            }



            //基本信息

            scope.Name = model.Name;
            scope.DisplayName = model.DisplayName;
            scope.Description = model.Description;
            scope.Enabled = model.Enabled;
            scope.Required = model.Required;

            //更新用户声明
            scope.UserClaims.RemoveAll(s => !model.UserClaims.Contains(s.Type));
            foreach (var item in model.UserClaims.Where(s => !scope.UserClaims.Select(s => s.Type).Contains(s)))
            {
                scope.UserClaims.Add(new ApiScopeClaim
                {
                    ScopeId=scope.Id,
                    Type = item
                });
            }

            //保存
            await _apiScopeRepository.UpdateAsync(scope);


            return new Result { Success = true };
        }

        private async Task<ApplicationUser> FindLoginUserAsync()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject || s.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }

    }
}
