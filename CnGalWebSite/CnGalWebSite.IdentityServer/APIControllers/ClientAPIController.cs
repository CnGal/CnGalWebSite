using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Clients;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using CnGalWebSite.IdentityServer.Models.ViewModels.Clients;

using CnGalWebSite.IdentityServer.Services.Examines;
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
using Client = IdentityServer4.EntityFramework.Entities.Client;

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
        private readonly IRepository<ApplicationDbContext, Examine, string> _examineRepository;
        private readonly IRepository<ApplicationDbContext, UserClient, string> _userClientRepository;

        public ClientAPIController(IRepository<ConfigurationDbContext, Client, string> clientRepository, IQueryService queryService, IExamineService examineService, IRepository<ApplicationDbContext, ApplicationUser, string> userRepository,
            IRepository<ApplicationDbContext, Examine, string> examineRepository, IRepository<ApplicationDbContext, UserClient, string> userClientRepository)
        {
            _clientRepository = clientRepository;
            _queryService = queryService;
            _examineService = examineService;
            _userRepository = userRepository;
            _examineRepository = examineRepository;
            _userClientRepository = userClientRepository;
        }

        /// <summary>
        /// 列出所有客户端
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<ClientOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Client, string>(_clientRepository.GetAll().AsSingleQuery(), model,
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

        /// <summary>
        /// 列出用户创建的客户端
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<UserClientOverviewModel>> List()
        {
            var user = await FindLoginUserAsync();

            var userclients = await _userClientRepository.GetAll().AsNoTracking()
                .Where(s => s.ApplicationUserId == user.Id)
                .ToListAsync();

            var clients = await _clientRepository.GetAll().AsNoTracking()
                .Where(s => userclients.Select(s => s.ClientId).Contains(s.Id))
                .ToListAsync();

            var model = new List<UserClientOverviewModel>();
            foreach (var item in clients)
            {
                var userClient = userclients.FirstOrDefault(s => s.ClientId == item.Id&&s.IsPassed==null);
                if (userClient != null && userClient.IsPassed == null)
                {
                    item.LogoUri = userClient.LogoUri;
                    item.ClientUri = userClient.ClientUri;
                    item.ClientName = userClient.ClientName;
                    item.Description = userClient.Description;
                }

                model.Add(new UserClientOverviewModel
                {
                    ClientId = item.ClientId,
                    ClientName = item.ClientName,
                    ClientUri = item.ClientUri,
                    Description = item.Description,
                    Enabled = item.Enabled,
                    IsPassed = userClient == null ? true : userClient.IsPassed,
                    Id = item.Id,
                    LogoUri = item.LogoUri,
                });
            }


            return model;
        }

        [HttpGet]
        public async Task<ActionResult<ClientEditModel>> Edit(int id)
        {
            var client = await _clientRepository.GetAll()
              .Include(s => s.AllowedGrantTypes)
              .Include(s => s.AllowedCorsOrigins)
              .Include(s => s.RedirectUris)
              .Include(s => s.PostLogoutRedirectUris)
              .Include(s => s.AllowedScopes)
              .FirstOrDefaultAsync(s => s.Id == id);

            if (client == null)
            {
                return NotFound("找不到客户端");
            }

            //查找待审核记录
            var user = await FindLoginUserAsync();
            var userClient = await _userClientRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null && s.ClientId == client.Id);
            if(userClient != null)
            {
                client.LogoUri = userClient.LogoUri;
                client.ClientUri = userClient.ClientUri;
                client.ClientName = userClient.ClientName;
                client.Description = userClient.Description;
            }

            var model = new ClientEditModel
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                Description = client.Description,
                Enabled = client.Enabled,
                RequireConsent = client.RequireConsent,
                RequireClientSecret = client.RequireClientSecret,
                Id = client.Id,
                LogoUri = client.LogoUri,
                AllowedScopes = client.AllowedScopes.Select(s => s.Scope).ToList(),
                AllowedCorsOrigins = client.AllowedCorsOrigins.Select(s => s.Origin).ToList(),
                AllowedGrantTypes = client.AllowedGrantTypes.Select(s => s.GrantType).ToList(),
                PostLogoutRedirectUris = client.PostLogoutRedirectUris.Select(s => s.PostLogoutRedirectUri).ToList(),
                RedirectUris = client.RedirectUris.Select(s => s.RedirectUri).ToList(),
                RequirePkce=client.RequirePkce,
                AllowAccessTokensViaBrowser=client.AllowAccessTokensViaBrowser,
            };

            return model;
        }

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
            var user = await FindLoginUserAsync();

            Client client = null;
            string secret = null;

            //检测所属
            if(!User.IsInRole("Admin")&& model.Id!=0)
            {
                if (!await _userClientRepository.AnyAsync(s=>s.ApplicationUserId== user.Id&&s.ClientId== model.Id))
                {
                    return new Result { Success = false, Message = "当前用户没有客户端的所有权" };
                }
            }

            if (model.Id == 0)
            {
                //检查用户客户端上限
                if(!User.IsInRole("Admin"))
                {
                    if (await _userClientRepository.CountAsync(s => s.ApplicationUserId == user.Id) > 10)
                    {
                        return new Result { Success = false, Message = "客户端达到上限" };
                    }
                }
                secret = Guid.NewGuid().ToString();
                client = await _clientRepository.InsertAsync(new Client
                {
                    RequireClientSecret = true,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequireConsent = true,
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


            //保存用户客户端
            var userClient = await _userClientRepository.FirstOrDefaultAsync(s => s.ClientId == client.Id);
            if (userClient == null)
            {
                userClient = await _userClientRepository.InsertAsync(new UserClient
                {
                    ApplicationUserId = user.Id,
                    ClientId = client.Id,
                    IsPassed = true
                }) ;
            }
            else
            {
                userClient.IsPassed = true;
            }

            //基本信息
            if (User.IsInRole("Admin"))
            {
                client.ClientName = model.ClientName;
                client.LogoUri = model.LogoUri;
                client.ClientUri = model.ClientUri;
                client.Description = model.Description;
                client.RequireConsent = model.RequireConsent;
                client.RequireClientSecret = model.RequireClientSecret;
                client.RequirePkce = model.RequirePkce;
                client.AllowAccessTokensViaBrowser = model.AllowAccessTokensViaBrowser;

                await _userClientRepository.UpdateAsync(userClient);
            }
            else
            {
                //检查是否修改
                if (client.ClientName != model.ClientName || client.LogoUri != model.LogoUri || client.ClientUri != model.ClientUri || client.Description != model.Description)
                {
                    userClient.ClientName = model.ClientName;
                    userClient.ClientUri = model.ClientUri;
                    userClient.Description = model.Description;
                    userClient.LogoUri = model.LogoUri;
                    userClient.IsPassed = null;
                    await _userClientRepository.UpdateAsync(userClient);
                    _userClientRepository.Clear();

                    var examine = new Examine
                    {
                        Type = ExamineType.Client,
                        UserClientId = userClient.Id,
                    };

                    await _examineService.AddExamine(user, examine);
                }
            }
           

            //不需要检查权限
            client.Enabled = model.Enabled;

            //更新授权方式
            client.AllowedGrantTypes.RemoveAll(s => !model.AllowedGrantTypes.Contains(s.GrantType));
            foreach (var item in model.AllowedGrantTypes.Where(s => !client.AllowedGrantTypes.Select(s => s.GrantType).Contains(s)))
            {
                client.AllowedGrantTypes.Add(new ClientGrantType
                {
                    ClientId = client.Id,
                    GrantType = item
                });
            }


            //主域
            client.AllowedCorsOrigins.RemoveAll(s => !model.AllowedCorsOrigins.Contains(s.Origin));
            foreach (var item in model.AllowedCorsOrigins.Where(s => !client.AllowedCorsOrigins.Select(s => s.Origin).Contains(s)))
            {
                client.AllowedCorsOrigins.Add(new ClientCorsOrigin
                {
                    ClientId = client.Id,
                    Origin = item
                });
            }

            //登入成功后回调
            client.RedirectUris.RemoveAll(s => !model.RedirectUris.Contains(s.RedirectUri));
            foreach (var item in model.RedirectUris.Where(s => !client.RedirectUris.Select(s => s.RedirectUri).Contains(s)))
            {
                client.RedirectUris.Add(new ClientRedirectUri
                {
                    ClientId = client.Id,
                    RedirectUri = item
                });
            }

            //退出登入后回调
            client.PostLogoutRedirectUris.RemoveAll(s => !model.PostLogoutRedirectUris.Contains(s.PostLogoutRedirectUri));
            foreach (var item in model.PostLogoutRedirectUris.Where(s => !client.PostLogoutRedirectUris.Select(s => s.PostLogoutRedirectUri).Contains(s)))
            {
                client.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUri
                {
                    ClientId = client.Id,
                    PostLogoutRedirectUri = item
                });
            }

            //更新授权方式
            client.AllowedScopes.RemoveAll(s => !model.AllowedScopes.Contains(s.Scope));
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

        /// <summary>
        /// 重置客户端密钥
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> Secret(ClientSecretResetModel model)
        {
            var user = await FindLoginUserAsync();
            var admin = User.IsInRole("Admin");

            if (admin)
            {
                if (!await _clientRepository.AnyAsync(s => s.Id == model.Id))
                {
                    return BadRequest("客户端不存在");
                }
            }
            else
            {
                if (!await _userClientRepository.AnyAsync(s => (s.ApplicationUserId == user.Id || admin) && s.ClientId == model.Id))
                {
                    return BadRequest("客户端不存在");
                }
            }


            var client = await _clientRepository.GetAll()
                .Include(s => s.ClientSecrets).FirstOrDefaultAsync(s => s.Id == model.Id);

            var secret = client.ClientSecrets.FirstOrDefault()?.Value;


            secret = Guid.NewGuid().ToString();
            client.ClientSecrets.Clear();
            client.ClientSecrets.Add(new ClientSecret { Value = secret.Sha256() });
            await _clientRepository.UpdateAsync(client);


            return new Result { Success = true,Message= secret };
        }

        private async Task<ApplicationUser> FindLoginUserAsync()
        {
            var id = User?.Claims?.FirstOrDefault(s => s.Type == JwtClaimTypes.Subject || s.Type == ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
