using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Clients;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.ApiScopes;
using CnGalWebSite.IdentityServer.Models.ViewModels.Clients;
using CnGalWebSite.IdentityServer.Models.ViewModels.IdentityResources;
using CnGalWebSite.IdentityServer.Models.ViewModels.Roles;

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
using IdentityResource = IdentityServer4.EntityFramework.Entities.IdentityResource;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/identityresources/[action]")]
    public class IdentityResourceAPIController : ControllerBase
    {
        private readonly IRepository<ConfigurationDbContext, IdentityResource, string> _identityResourceRepository;
        private readonly IQueryService _queryService;
        private readonly IRepository<ApplicationDbContext, ApplicationUser, string> _userRepository;

        public IdentityResourceAPIController(IRepository<ConfigurationDbContext, IdentityResource, string> identityResourceRepository, IQueryService queryService, IRepository<ApplicationDbContext, ApplicationUser, string> userRepository)
        {
            _identityResourceRepository =  identityResourceRepository;
            _queryService = queryService;
            _userRepository = userRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IEnumerable<KeyValuePair<string, string>>> All()
        {
            return await _identityResourceRepository.GetAll().Select(s => new KeyValuePair<string, string>(s.Name, s.DisplayName)).ToListAsync();
        }



        [HttpPost]
        public async Task<QueryResultModel<IdentityResourceOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<IdentityResource, string>(_identityResourceRepository.GetAll(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.DisplayName.Contains(model.SearchText) || s.Description.Contains(model.SearchText)));

            return new QueryResultModel<IdentityResourceOverviewModel>
            {
                Items = await items.Select(s => new IdentityResourceOverviewModel
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
        public async Task<ActionResult<IdentityResourceEditModel>> Edit(int id)
        {
            var scope = await _identityResourceRepository.GetAll()
              .Include(s => s.UserClaims)
              .FirstOrDefaultAsync(s => s.Id == id);

            if (scope == null)
            {
                return NotFound("找不到IdentityResource");
            }

            var model = new IdentityResourceEditModel
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
        public async Task<Result> Edit(IdentityResourceEditModel model)
        {

            IdentityResource identity = null;
            if (model.Id == 0)
            {
                identity = await _identityResourceRepository.InsertAsync(new IdentityResource
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    Enabled = model.Enabled,
                    Required = model.Required,
                    Created=DateTime.UtcNow
                });
                model.Id = identity.Id;
                _identityResourceRepository.Clear();
            }

            identity = await _identityResourceRepository.GetAll()
                .Include(s => s.UserClaims)
                .FirstOrDefaultAsync(s => s.Id == model.Id);


            if (identity == null)
            {
                return new Result { Success = false, Message = "IdentityResource不存在" };
            }



            //基本信息

            identity.Name = model.Name;
            identity.DisplayName = model.DisplayName;
            identity.Description = model.Description;
            identity.Enabled = model.Enabled;
            identity.Required = model.Required;

            //更新用户声明
            identity.UserClaims.RemoveAll(s => !model.UserClaims.Contains(s.Type));
            foreach (var item in model.UserClaims.Where(s => !identity.UserClaims.Select(s => s.Type).Contains(s)))
            {
                identity.UserClaims.Add(new IdentityResourceClaim
                {
                    IdentityResourceId = identity.Id,
                    Type = item
                });
            }

            identity.Updated = DateTime.UtcNow;
            //保存
            await _identityResourceRepository.UpdateAsync(identity);


            return new Result { Success = true };
        }

        private async Task<ApplicationUser> FindLoginUserAsync()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject || s.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }

    }
}
