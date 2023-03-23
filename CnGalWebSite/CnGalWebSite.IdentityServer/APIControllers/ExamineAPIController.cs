using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Clients;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.Clients;
using CnGalWebSite.IdentityServer.Models.ViewModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.Roles;
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
    [Authorize(LocalApi.PolicyName, Roles = "Admin")]
    [ApiController]
    [Route("api/examines/[action]")]
    public class ExamineAPIController : ControllerBase
    {
        private readonly IRepository<ApplicationDbContext, Examine, string> _examineRepository;
        private readonly IQueryService _queryService;
        private readonly IRepository<ApplicationDbContext, ApplicationUser, string> _userRepository;
        private readonly IRepository<ConfigurationDbContext, Client, string> _clientRepository;

        public ExamineAPIController(IRepository<ApplicationDbContext, Examine, string> examineRepository, IQueryService queryService, IRepository<ApplicationDbContext, ApplicationUser, string> userRepository, IRepository<ConfigurationDbContext, Client, string> clientRepository)
        {
            _examineRepository = examineRepository;
            _queryService = queryService;
            _userRepository = userRepository;
            _clientRepository = clientRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<ExamineOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Examine, string>(_examineRepository.GetAll().Include(s => s.ApplicationUser), model,
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
                    UserName = s.ApplicationUser.UserName,
                    Id = s.Id,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpGet]
        public async Task<ActionResult<ExamineDetailModel>> View(long id)
        {
            var examine = await _examineRepository.GetAll()
                .Include(s => s.UserClient)
                .Include(s => s.ApplicationUser)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (examine == null)
            {
                return NotFound();
            }

            //初始化视图
            var model = new ExamineDetailModel
            {
                ApplyTime = examine.ApplyTime,
                Id = id,
                IsPassed = examine.IsPassed,
                PassedAdminName = examine.PassedAdminName,
                PassedTime = examine.PassedTime,
                UserName = examine.ApplicationUser.UserName,
                Type = examine.Type,
            };

            //仅可视化待处理的审核
            if (model.IsPassed == null)
            {
                model.KeyValues.Add(new ExamineKeyVauleModel
                {
                    Key = "客户端名称",
                    Vaule = examine.UserClient.ClientName
                });
                model.KeyValues.Add(new ExamineKeyVauleModel
                {
                    Key = "客户端简介",
                    Vaule = examine.UserClient.Description
                });
                model.KeyValues.Add(new ExamineKeyVauleModel
                {
                    Key = "客户端主页",
                    Vaule = examine.UserClient.ClientUri
                });

                model.Images.Add(new ExamineImageModel
                {
                    Name = "Logo",
                    Url = examine.UserClient.LogoUri
                });
            }

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> Proc(ExamineProcModel model)
        {
            var examine = await _examineRepository.GetAll()
                .Include(s => s.UserClient)
                .FirstOrDefaultAsync(s => s.Id == model.Id);
            if (examine == null)
            {
                return NotFound();
            }

            var user = await FindLoginUserAsync();

            //通过审核
            if (model.IsPassed)
            {
                var userClient = examine.UserClient;

                var client = await _clientRepository.FirstOrDefaultAsync(s => s.Id == userClient.ClientId);
                if (client == null)
                {
                    return new Result { Success = false, Message = "客户端不存在" };
                }

                //复制数据
                client.LogoUri = userClient.LogoUri;
                client.ClientUri = userClient.ClientUri;
                client.ClientName = userClient.ClientName;
                client.Description = userClient.Description;
                
                await _clientRepository.UpdateAsync(client);

            }

            //更新数据
            examine.UserClient.IsPassed= examine.IsPassed = model.IsPassed;
            examine.PassedAdminName = user.UserName;
            examine.PassedTime = DateTime.UtcNow;
            await _examineRepository.UpdateAsync(examine);

            return new Result { Success = true };

        }

        private async Task<ApplicationUser> FindLoginUserAsync()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject || s.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }

    }
}
