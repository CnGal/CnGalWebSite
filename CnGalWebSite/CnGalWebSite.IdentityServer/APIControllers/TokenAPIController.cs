using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.IdentityServer.Admin.SSR.Models;
using CnGalWebSite.IdentityServer.Data;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;

using CnGalWebSite.IdentityServer.Models.ViewModels.Tokens;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/tokens/[action]")]
    public partial class TokenAPIController : ControllerBase
    {
        private readonly IRepository<ApplicationDbContext, AppUserAccessToken, long> _tokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, string> _ips = new Dictionary<string, string>();
        private DateTime _lastRefreshTime;

        public TokenAPIController(IRepository<ApplicationDbContext, AppUserAccessToken, long> tokenRepository, IConfiguration configuration)
        {
            _tokenRepository = tokenRepository;
            _configuration = configuration;

            if (string.IsNullOrWhiteSpace(_configuration["IpWhitelist"]) == false)
            {
                foreach (var item in _configuration["IpWhitelist"].Split(',').Select(s => new KeyValuePair<string, string>(s.Trim(), null)))
                {
                    _ips.Add(item);
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<AppUserAccessToken>> Get(GetTokenModel model)
        {
            if (!Check(model.Secret))
            {
                return BadRequest();
            }

            var token = await _tokenRepository.FirstOrDefaultAsync(s => s.UserId == model.UserId && s.Type == model.Type);
            if (token == null)
            {
                return NotFound();
            }

            return token;

        }

        [HttpPost]
        public async Task<Result> Set(SetTokenModel model)
        {
            if (!Check(model.Secret))
            {
                return new Result { Success = false };
            }

            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                return new Result { Success = false, Message = "用户Id不能为空" };
            }

            await _tokenRepository.GetAll().Where(s => s.UserId == model.UserId && s.Type == model.Type).ExecuteDeleteAsync();

            await _tokenRepository.InsertAsync(new AppUserAccessToken
            {
                AccessToken = model.AccessToken,
                Expiration = model.Expiration,
                RefreshToken = model.RefreshToken,
                Type = model.Type,
                UserId = model.UserId,
            });

            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<Result> Delete(DeleteTokenModel model)
        {
            if (!Check(model.Secret))
            {
                return new Result { Success = false };
            }

            await _tokenRepository.GetAll().Where(s => s.UserId == model.UserId && s.Type == model.Type).ExecuteDeleteAsync();

            return new Result { Success = true };
        }


        #region 判断权限
        private static string GetHostAddresses(string howtogeek)
        {
            IPAddress[] addresslist = Dns.GetHostAddresses(howtogeek);

            return addresslist.FirstOrDefault(s => s.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
        }

        private void RefreshIPs()
        {
            if ((DateTime.UtcNow - _lastRefreshTime).TotalMinutes < 10)
            {
                return;
            }
            _lastRefreshTime = DateTime.UtcNow;

            foreach (var item in _ips)
            {
                if (IpRegex().IsMatch(item.Key))
                {
                    _ips[item.Key] = item.Key;
                }
                else
                {
                    _ips[item.Key] = GetHostAddresses(item.Key);
                }
            }
        }

        private bool Check(string secret)
        {
            var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            //刷新ip
            RefreshIPs();
            //判断是否本地调用
            if (IntranetIpRegex().IsMatch(ip) || _ips.Any(s => ip.Contains(s.Value)))
            {
                if (secret == _configuration["TokenAPISecret"])
                {
                    return true;
                }
            }

            return false;
        }

        [GeneratedRegex("^(127\\.0\\.0\\.1)|(localhost)|(10\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3})|(172\\.((1[6-9])|(2\\d)|(3[01]))\\.\\d{1,3}\\.\\d{1,3})|(192\\.168\\.\\d{1,3}\\.\\d{1,3})$")]
        public static partial Regex IntranetIpRegex();
        [GeneratedRegex("^((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})(\\.((2(5[0-5]|[0-4]\\d))|[0-1]?\\d{1,2})){3}$")]
        public static partial Regex IpRegex();
        #endregion
    }
}
