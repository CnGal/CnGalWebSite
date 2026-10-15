using CnGalWebSite.IdentityServer.Models.Sorting;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.ViewModels.Roles;
using CnGalWebSite.IdentityServer.Models.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static IdentityServer4.IdentityServerConstants;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/roles/[action]")]
    public class RolesAPIController : ControllerBase
    {
        private static readonly SortMap<ApplicationRole, IdentityRoleSortField> ListSorting =
            new SortMap<ApplicationRole, IdentityRoleSortField>(IdentityRoleSortField.Id, IdentityRoleSortField.Id)
            .Add(IdentityRoleSortField.Id, s => s.Id)
            .Add(IdentityRoleSortField.Name, s => s.Name);

        private static readonly SortMap<ApplicationUser, IdentityUserSortField> ListUserSorting =
            new SortMap<ApplicationUser, IdentityUserSortField>(IdentityUserSortField.Id, IdentityUserSortField.Id)
            .Add(IdentityUserSortField.Email, s => s.Email)
            .Add(IdentityUserSortField.Id, s => s.Id)
            .Add(IdentityUserSortField.UserName, s => s.UserName);


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
        public async Task<IEnumerable<KeyValuePair<string, string>>> All()
        {
            return await _roleRepository.GetAll().Select(s => new KeyValuePair<string, string>(s.Name, null)).ToListAsync();
        }

        [HttpPost]
        public async Task<QueryResultModel<RoleOverviewModel, IdentityRoleSortField>> List(QueryParameterModel<IdentityRoleSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<ApplicationRole, IdentityRoleSortField>(_roleRepository.GetAll().AsSingleQuery(), model, s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Id.Contains(model.SearchText) || s.Name.Contains(model.SearchText)), ListSorting);

            return new QueryResultModel<RoleOverviewModel, IdentityRoleSortField>
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

            if (string.IsNullOrWhiteSpace(model.Id))
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
        public async Task<QueryResultModel<UserOverviewModel, IdentityUserSortField>> ListUser(QueryParameterModel<IdentityUserSortField> model, [FromQuery] string id)
        {
            var (items, total) = await _queryService.QueryAsync<ApplicationUser, IdentityUserSortField>(_userRepository.GetAll().Include(s => s.UserRoles).ThenInclude(s => s.Role), model,
                s => (string.IsNullOrWhiteSpace(model.SearchText) || (s.Id.Contains(model.SearchText) || s.UserName.Contains(model.SearchText) || s.Email.Contains(model.SearchText))) && s.UserRoles.Select(s => s.Role.Id).Contains(id), ListUserSorting);

            return new QueryResultModel<UserOverviewModel, IdentityUserSortField>
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
