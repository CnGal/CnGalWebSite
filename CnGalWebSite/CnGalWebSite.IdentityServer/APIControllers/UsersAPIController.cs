using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.ViewModels.Shared;
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

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/users/[action]")]
    public class UsersAPIController
    {
        private readonly IRepository<ApplicationUser, string> _userRepository;

        public UsersAPIController(IRepository<ApplicationUser, string> userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<QueryResultModel<UserOverviewModel>>> List(QueryParameterModel model)
        {
            var sortBy = model.SortBy.ToArray();
            var sortDesc = model.SortDesc.ToArray();
            var page = model.Page;
            var itemsPerPage = model.ItemsPerPage;

            
            var items = _userRepository.GetAll().AsNoTracking();
            //搜索
            if(!string.IsNullOrWhiteSpace(model.SearchText))
            {
                items = items.Where(s => s.Id.Contains(model.SearchText) || s.UserName.Contains(model.SearchText) || s.Email.Contains(model.SearchText));
            }

            //计算总数
            var total = await items.CountAsync();

            //排序
            var sb = new StringBuilder();
            for (int i = 0; i < sortBy.Length; i++)
            {
                sb.Append($"{(i == 0 ? "" : ", ")}{sortBy[i]}{(sortDesc[i] ? " desc" : "")}");
            }
            if (sb.Length != 0)
            {
                items = items.OrderBy(sb.ToString());
            }

            //分页
            if (itemsPerPage > 0)
            {
                items = items.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);
            }

            return new QueryResultModel<UserOverviewModel>
            {
                Items = items.Select(s => new UserOverviewModel
                {
                    Email = s.Email,
                    Id = s.Id,
                    UserName = s.UserName
                }),
                Total = total,
                Parameter = model
            };
        }

    }
}
