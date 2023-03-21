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
using CnGalWebSite.IdentityServer.Services.Shared;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/users/[action]")]
    public class UsersAPIController : ControllerBase
    {
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IQueryService _queryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<ApplicationRole, string> _roleRepository;

        public UsersAPIController(IRepository<ApplicationUser, string> userRepository, IQueryService queryService, UserManager<ApplicationUser> userManager, IRepository<ApplicationRole, string> roleRepository)
        {
            _userRepository = userRepository;
            _queryService = queryService;
            _userManager = userManager;
            _roleRepository = roleRepository;
        }

        [HttpPost]
        public async Task<QueryResultModel<UserOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ApplicationUser, string>(_userRepository.GetAll().Include(s => s.UserRoles).ThenInclude(s => s.Role), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Id.Contains(model.SearchText) || s.UserName.Contains(model.SearchText) || s.Email.Contains(model.SearchText)));

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

        [HttpPost]
        public async Task<Result> Edit(UserEditModel model)
        {
            //检查数据
            //基本信息
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                return new Result { Success = false, Message = "用户名不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return new Result { Success = false, Message = "电子邮箱不能为空" };
            }

            //修改密码
            if (model.ChangePassword)
            {
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    return new Result { Success = false, Message = "密码不能为空" };
                }
                if (model.Password != model.ConfirmPassword)
                {
                    return new Result { Success = false, Message = "两次输入的密码不一致" };
                }
            }
            //添加新用户
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                if (await _userRepository.AnyAsync(s => s.UserName == model.UserName))
                {
                    return new Result { Success = false, Message = "用户名重复" };
                }
                if (await _userRepository.AnyAsync(s => s.Email == model.Email))
                {
                    return new Result { Success = false, Message = "电子邮箱重复" };
                }
            }

            //操作
            ApplicationUser user = null;
            //添加新用户
            if (string.IsNullOrWhiteSpace(model.Id))
            {


                //创建用户
                var result = await _userManager.CreateAsync(new ApplicationUser
                {
                    EmailConfirmed = true,
                });
                if (!result.Succeeded)
                {
                    return new Result { Success = false, Message = result.Errors.FirstOrDefault()?.Description };
                }
                //添加角色
                user = await _userRepository.FirstOrDefaultAsync(s => s.UserName == model.UserName);
                await _userManager.AddToRoleAsync(user, "User");
                //添加密码
                if (model.ChangePassword)
                {
                    result = await _userManager.AddPasswordAsync(user, model.Password);
                    if (!result.Succeeded)
                    {
                        result = await _userManager.AddPasswordAsync(user, "abc123..");
                        if (!result.Succeeded)
                        {
                            return new Result { Success = false, Message = result.Errors.FirstOrDefault()?.Description };
                        }
                        return new Result { Success = true, Message = "设置的密码无效，自动替换为默认密码“abc123..”" };
                    }
                }
            }
            //修改现有用户
            else
            {
                user = await _userRepository.FirstOrDefaultAsync(s => s.Id == model.Id);

                if (user == null)
                {
                    return new Result { Success = false, Message = "用户不存在" };
                }

                //修改密码
                if (model.ChangePassword)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                    if (!result.Succeeded)
                    {
                        return new Result { Success = false, Message = result.Errors.FirstOrDefault()?.Description };
                    }
                }
            }

            //基本信息
            user.UserName=model.UserName;
            user.Email=model.Email;

            //保存
            await _userRepository.UpdateAsync(user);

           //同步角色
            bool check = false;
            foreach (var item in _roleRepository.GetAll().AsNoTracking().Select(s => s.Name))
            {
                var isInRole = await _userManager.IsInRoleAsync(user, item);
                if (isInRole != model.Roles.Contains(item))
                {
                    //检查权限
                    if (!check)
                    {
                        var admin = await FindLoginUserAsync();
                        if (!await _userManager.IsInRoleAsync(admin, "SuperAdmin"))
                        {
                            return new Result { Success = false, Message = "权限不足" };
                        }
                        check = true;
                    }


                    if (isInRole)
                    {
                        await _userManager.RemoveFromRoleAsync(user, item);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, item);
                    }
                }
            }

            return new Result { Success = true };
        }

        private async Task<ApplicationUser> FindLoginUserAsync()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject || s.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
