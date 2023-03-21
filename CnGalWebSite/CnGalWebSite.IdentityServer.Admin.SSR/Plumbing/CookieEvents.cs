using CnGalWebSite.Extensions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Plumbing;

public class CookieEvents : CookieAuthenticationEvents
{
    private readonly IUserAccessTokenStore _store;
    private readonly IUserAccessTokenManagementService _userAccessTokenManagementService;
    private readonly List<KeyValuePair<DateTime,string>> _failedCount = new List<KeyValuePair<DateTime, string>>();

    public CookieEvents(IUserAccessTokenStore store, IUserAccessTokenManagementService userAccessTokenManagementService)
    {
        _store = store;
        _userAccessTokenManagementService = userAccessTokenManagementService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        await base.ValidatePrincipal(context);

        var id = context?.Principal?.Claims?.GetUserId();
        if (string.IsNullOrWhiteSpace(id))
        {
            context.RejectPrincipal();
            return;
        }

        var token = await _userAccessTokenManagementService.GetUserAccessTokenAsync(context.Principal);
        if (string.IsNullOrWhiteSpace(token))
        {
            //添加失败记录
            _failedCount.Add(new KeyValuePair<DateTime, string>(DateTime.UtcNow, id));
            //清除过期记录
            _failedCount.RemoveAll(s => s.Key < DateTime.UtcNow.AddSeconds(-30));
            //判断错误次数
            if (_failedCount.Count(s => s.Value == id) > 3)
            {
                context.RejectPrincipal();
            }
        }
        else
        {
            //成功后清除记录
            _failedCount.RemoveAll(s => s.Value == id);
        }
    }

}
