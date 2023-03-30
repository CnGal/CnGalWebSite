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
using System.Linq.Dynamic.Core;
using System.Text;
using Microsoft.EntityFrameworkCore;
using BlazorComponent;
using CnGalWebSite.IdentityServer.Models.ViewModels.Roles;
using CnGalWebSite.IdentityServer.Services.Shared;
using OneOf.Types;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.Core.Models;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/roles/[action]")]
    public class RolesAPIController : ControllerBase
    {
        private readonly IRepository<ApplicationDbContext, ApplicationRole, string> _roleRepository;
        private readonly IQueryService _queryService;
        private readonly IRepository<ApplicationDbContext, ApplicationUser, string> _userRepository;

        public RolesAPIController(IRepository<ApplicationDbContext, ApplicationRole, string> roleRepository, IQueryService queryService, IRepository<ApplicationDbContext, ApplicationUser, string> userRepository)
        {
            _roleRepository = roleRepository;
            _queryService = queryService;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IEnumerable< KeyValuePair<string, string>>> All()
        {
            return await _roleRepository.GetAll().Select(s => new KeyValuePair<string, string>(s.Name, null)).ToListAsync();
        }

        [HttpPost]
        public async Task<QueryResultModel<RoleOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ApplicationRole, string>(_roleRepository.GetAll(), model, s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Id.Contains(model.SearchText) || s.Name.Contains(model.SearchText)));

            return new QueryResultModel<RoleOverviewModel>
            {
                Items = await items.Select(s => new RoleOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<Result> Edit(RoleEditModel model)
        {
            //检查数据
            //基本信息
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new Result { Success = false, Message = "名称不能为空" };
            }

            ApplicationRole role = null;

            if(string.IsNullOrWhiteSpace( model.Id))
            {
                role = await _roleRepository.InsertAsync(new ApplicationRole());
            }
            else
            {
                role = await _roleRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            }

            if (role == null)
            {
                return new Result { Success = false, Message = "角色不存在" };
            }


            //基本信息
            role.Name = model.Name;
            role.NormalizedName = model.Name.ToUpper();

            //保存
            await _roleRepository.UpdateAsync(role);


            return new Result { Success = true };
        }

        [HttpGet]
        public async Task<ActionResult<RoleDetailModel>> View(string id)
        {
            var role = await _roleRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new RoleDetailModel
            {
                Id = role.Id,
                Name = role.Name,
            };

            return model;
        }

        [HttpPost]
        public async Task<QueryResultModel<UserOverviewModel>> ListUser(QueryParameterModel model, [FromQuery]string id)
        {
            var (items, total) = await _queryService.QueryAsync<ApplicationUser, string>(_userRepository.GetAll().Include(s => s.UserRoles).ThenInclude(s => s.Role), model,
                s => (string.IsNullOrWhiteSpace(model.SearchText)||(s.Id.Contains(model.SearchText) || s.UserName.Contains(model.SearchText) || s.Email.Contains(model.SearchText))) && s.UserRoles.Select(s => s.Role.Id).Contains(id));

            return new QueryResultModel<UserOverviewModel>
            {
                Items = await items.Select(s => new UserOverviewModel
                {
                    Email = s.Email,
                    Id = s.Id,
                    UserName = s.UserName,
                    Roles = s.UserRoles.Select(s => s.Role.Name).ToList()
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
