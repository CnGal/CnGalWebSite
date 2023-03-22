using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.Clients;
using CnGalWebSite.IdentityServer.Models.ViewModels.Shared;
using CnGalWebSite.IdentityServer.Models.ViewModels.Users;
using CnGalWebSite.IdentityServer.Services.Account;
using CnGalWebSite.IdentityServer.Services.Examines;
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
    [Route("api/clients/[action]")]
    public class ClientAPIController : ControllerBase
    {
        private readonly IRepository<ConfigurationDbContext, Client, string> _clientRepository;
        private readonly IQueryService _queryService;
        private readonly IExamineService _examineService;
        private readonly IRepository<ApplicationDbContext, ApplicationUser, string> _userRepository;

        public ClientAPIController(IRepository<ConfigurationDbContext, Client, string> clientRepository, IQueryService queryService, IExamineService examineService, IRepository<ApplicationDbContext, ApplicationUser, string> userRepository)
        {
            _clientRepository = clientRepository;
            _queryService   = queryService;
            _examineService = examineService;
            _userRepository= userRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<ClientOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Client, string>(_clientRepository.GetAll(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.ClientId.Contains(model.SearchText) || s.ClientName.Contains(model.SearchText) || s.Description.Contains(model.SearchText)));

            return new QueryResultModel<ClientOverviewModel>
            {
                Items = await items.Select(s => new ClientOverviewModel
                {
                    ClientId = s.ClientId,
                    ClientName = s.ClientName,
                    ClientUri = s.ClientUri,
                    Description = s.Description,
                    Enabled = s.Enabled,
                    LogoUri = s.LogoUri,
                    Id = s.Id,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<ActionResult< ClientEditModel>> Edit(int id)
        {
           var client = await _clientRepository.GetAll()
             .Include(s => s.AllowedGrantTypes)
             .Include(s => s.AllowedCorsOrigins)
             .Include(s => s.RedirectUris)
             .Include(s => s.PostLogoutRedirectUris)
             .Include(s => s.AllowedScopes)
             .FirstOrDefaultAsync(s => s.Id == id);

            if(client== null)
            {
                return NotFound("找不到客户端");
            }

            var model = new ClientEditModel
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                Description = client.Description,
                Enabled = client.Enabled,
                Id = client.Id,
                LogoUri = client.LogoUri,
                AllowedScopes = client.AllowedScopes.Select(s => s.Scope).ToList(),
                AllowedCorsOrigins = client.AllowedCorsOrigins.Select(s => s.Origin).ToList(),
                AllowedGrantTypes = client.AllowedGrantTypes.Select(s => s.GrantType).ToList(),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(s => s.PostLogoutRedirectUri).ToList(),
                RedirectUris = client.RedirectUris.Select(s => s.RedirectUri).ToList(),
            };

            return model;
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<Result> Edit(ClientEditModel model)
        {
            //检查数据
            //基本信息
            if (string.IsNullOrWhiteSpace(model.ClientName))
            {
                return new Result { Success = false, Message = "客户端名称不能为空" };
            }
            if (string.IsNullOrWhiteSpace(model.ClientUri))
            {
                return new Result { Success = false, Message = "客户端链接不能为空" };
            }

            Client client = null;
            string secret = null;
            if (model.Id==0)
            {
                secret = Guid.NewGuid().ToString();
                client = await _clientRepository.InsertAsync(new Client
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientSecrets = new List<ClientSecret> { new ClientSecret { Value = secret } }
                });
                model.Id = client.Id;
                _clientRepository.Clear();
            }

            client = await _clientRepository.GetAll()
                .Include(s => s.AllowedGrantTypes)
                .Include(s => s.AllowedCorsOrigins)
                .Include(s => s.RedirectUris)
                .Include(s => s.PostLogoutRedirectUris)
                .Include(s => s.AllowedScopes)
                .FirstOrDefaultAsync(s => s.Id == model.Id);
            

            if (client == null)
            {
                return new Result { Success = false, Message = "客户端不存在" };
            }


            //基本信息
            if(User.IsInRole("Admin"))
            {
                client.ClientName = model.ClientName;
                client.LogoUri = model.LogoUri;
                client.ClientUri = model.ClientUri;
                client.Description = model.Description;
                client.Enabled = model.Enabled;
            }
            else
            {
                //检查是否修改
                if(client.ClientName != model.ClientName || client.LogoUri != model.LogoUri || client.ClientUri != model.ClientUri || client.Description != model.Description )
                {
                    var user =await FindLoginUserAsync();

                    var examine = new Examine
                    {
                        Type = ExamineType.Client,
                        ClientId = client.Id,
                        ClientExamine = new ClientExamine
                        {
                            ClientName = model.ClientName,
                            ClientUri = model.ClientUri,
                            Description = model.Description,
                            LogoUri = model.LogoUri,
                        }
                    };

                    await _examineService.AddExamines(user,examine);
                }
            }


            //更新授权方式
            client.AllowedGrantTypes.RemoveAll(s => model.AllowedGrantTypes.Contains(s.GrantType));
            foreach(var item in model.AllowedGrantTypes.Where(s=>!client.AllowedGrantTypes.Select(s=>s.GrantType).Contains(s)))
            {
                client.AllowedGrantTypes.Add(new ClientGrantType
                {
                    ClientId = client.Id,
                    GrantType = item
                });
            }

           
            //主域
            client.AllowedCorsOrigins.RemoveAll(s => model.AllowedCorsOrigins.Contains(s.Origin));
            foreach (var item in model.AllowedCorsOrigins.Where(s => !client.AllowedCorsOrigins.Select(s => s.Origin).Contains(s)))
            {
                client.AllowedCorsOrigins.Add(new ClientCorsOrigin
                {
                    ClientId = client.Id,
                    Origin = item
                });
            }

            //登入成功后回调
            client.RedirectUris.RemoveAll(s => model.RedirectUris.Contains(s.RedirectUri));
            foreach (var item in model.RedirectUris.Where(s => !client.RedirectUris.Select(s => s.RedirectUri).Contains(s)))
            {
                client.RedirectUris.Add(new ClientRedirectUri
                {
                    ClientId = client.Id,
                    RedirectUri = item
                });
            }

            //退出登入后回调
            client.PostLogoutRedirectUris.RemoveAll(s => model.PostLogoutRedirectUris.Contains(s.PostLogoutRedirectUri));
            foreach (var item in model.PostLogoutRedirectUris.Where(s => !client.PostLogoutRedirectUris.Select(s => s.PostLogoutRedirectUri).Contains(s)))
            {
                client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri
                {
                    ClientId = client.Id,
                    PostLogoutRedirectUri = item
                });
            }

            //更新授权方式
            client.AllowedScopes.RemoveAll(s => model.AllowedScopes.Contains(s.Scope));
            foreach (var item in model.AllowedScopes.Where(s => !client.AllowedScopes.Select(s => s.Scope).Contains(s)))
            {
                client.AllowedScopes.Add(new ClientScope
                {
                    ClientId = client.Id,
                    Scope = item
                });
            }

            client.Updated = DateTime.UtcNow;
            //保存
            await _clientRepository.UpdateAsync(client);


            return new Result { Success = true };
        }

        private async Task<ApplicationUser> FindLoginUserAsync()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject || s.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
