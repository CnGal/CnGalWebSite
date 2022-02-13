using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.Shared.Component.Errors;
using CnGalWebSite.Shared.Component.Others;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class MainLayout : IAsyncDisposable
    {

        public UserUnReadedMessagesModel UnreadedMessages { get; set; } = new UserUnReadedMessagesModel();

        private List<MenuItem> Menus { get; set; }

        private Dictionary<string, string> TabItemTextDictionary { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
        [CascadingParameter]
        public ErrorHandler ErrorHandler { get; set; }
        [NotNull]
        private StyleTip styleTip { get; set; }

        private string image = null;
        private string userId = null;
        private string userName = null;

        #region 获取屏幕大小

        /// <summary>
        /// 获得/设置 IJSRuntime 实例
        /// </summary>
        [Inject]
        [System.Diagnostics.CodeAnalysis.NotNull]
        public IJSRuntime JSRuntime { get; set; }



        public bool IsSmallScreen { get; set; }
        public bool IsNormalScreen { get; set; }
        public bool IsLargeScreen { get; set; }


        private JSInterop<MainLayout> Interop { get; set; }

        /// <summary>
        /// OnAfterRenderAsync 方法
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    Interop = new JSInterop<MainLayout>(JSRuntime);
                    await Interop.InvokeVoidAsync(this, null, "bb_layout", nameof(SetCollapsed));

                }
                catch
                {

                }


                //启动定时器
                mytimer = new System.Threading.Timer(new System.Threading.TimerCallback(Send), null, 0, 1000 * 60 * 10);

                //加载调试工具
                if (_dataCacheService.ThemeSetting.IsDebug)
                {
                    try
                    {
                        await JS.InvokeAsync<string>("initDebugTool");
                    }
                    catch
                    {


                    }
                }




                //需要调用一次令牌刷新接口 确保登入没有过期
                var result = await _authService.Refresh();
                if (result != null && result.Code != LoginResultCode.OK)
                {
                    ErrorHandler.ProcessError(new Exception(result.ErrorDescribe), "尝试自动登入失败");
                    StateHasChanged();
                }

                //var authState = await authenticationStateTask;
                //var user = authState.User;
                //获取用户信息
                //GetUserInfor(user);
                StateHasChanged();
                //读取用户未读信息
                await GetUserUnreadedMessages();

                //设置用户身份
                try
                {
                    await JS.InvokeAsync<string>("setCustomVar", "登录状态", string.IsNullOrWhiteSpace(userId) ? "未登入" : "已登入");
                }
                catch (Exception)
                {

                }
                //记录数据
                if (string.IsNullOrWhiteSpace(userName) == false && string.IsNullOrWhiteSpace(userId) == false)
                {
                    try
                    {
                        await JS.InvokeAsync<string>("trackEvent", "登入", userId, userName, "1", "login");
                    }
                    catch (Exception)
                    {

                    }
                }
            }


            //删除多余的滚动条
            try
            {
                await JS.InvokeAsync<string>("deleteDiv", "slimScrollBar");
            }
            catch
            {

            }
        }



        /// <summary>
        /// 设置侧边栏收缩方法 客户端监控 window.onresize 事件回调此方法
        /// </summary>
        /// <returns></returns>
        [JSInvokable]
        public void SetCollapsed(int width)
        {
            if (IsSmallScreen != (width < 682))
            {
                IsSmallScreen = width < 682;
                StateHasChanged();
            }
            if (IsNormalScreen != (width >= 682 && width < 768))
            {
                IsNormalScreen = width >= 682 && width < 768;
                StateHasChanged();
            }
            if (IsLargeScreen != (width >= 768))
            {
                IsLargeScreen = width >= 768;
                StateHasChanged();
            }
        }
        #endregion


        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            //清楚登入令牌
            _dataCacheService.LoginKey = "";
            _dataCacheService.IsOnThirdPartyLogin = true;
            _dataCacheService.ThirdPartyLoginTempModel = null;

            var authState = await authenticationStateTask;
            var user = authState.User;

            // 菜单获取可以通过数据库获取，此处为示例直接拼装的菜单集合
            TabItemTextDictionary = new()
            {
                [""] = "主页",
                ["/entries/index"] = "词条",
            };
            //获取菜单
            Menus = GetIconSideMenuItemsAsync(user);

            
        }

        private void GetUserInfor(ClaimsPrincipal user)
        {
            foreach (var item in user.Claims)
            {
                if (item.Type == "image")
                {
                    image = item.Value;
                }
                else if (item.Type == "userid")
                {
                    userId = item.Value;
                }
            }
        }

        private static List<MenuItem> GetIconSideMenuItemsAsync(ClaimsPrincipal user)
        {
            var menus = new List<MenuItem>
            {
                new MenuItem() { Text = "主页", Icon = "fa fa-fw fa-home", Url = "/" , Match = NavLinkMatch.All},
                new MenuItem() { Text = "随机", Icon = "fa fa-fw fa-random", Url = "/home/random" },
                new MenuItem() { Text = "打折游戏", Icon = "fa fa-fw fa-dollar", Url = "/discount" },
                new MenuItem() { Text = "免费游戏", Icon = "fa fa-fw fa-gift", Url = "/free" },

                new MenuItem() { Text = "文章", Icon = "fa fa-fw fa-newspaper-o", Url = "/articles/home" },
                new MenuItem() { Text = "词条", Icon = "fa fa-fw fa-codepen", Url="/null", IsCollapsed=false, Items=new List<MenuItem>{
                    new MenuItem() { Text = "游戏", Icon = "fa fa-fw fa-gamepad", Url = "/entries/home/游戏" },
                    new MenuItem() { Text = "角色", Icon = "fa fa-fw fa-group", Url = "/entries/home/角色" },
                    new MenuItem() { Text = "STAFF", Icon = "fa fa-fw fa-star", Url = "/entries/home/STAFF" },
                    new MenuItem() { Text = "制作组", Icon = "fa fa-fw fa-object-group", Url = "/entries/home/制作组" }
                } },

                new MenuItem() { Text = "分类", Icon = "fa fa-fw fa-tags", Url="/null",IsCollapsed=true, Items=new List<MenuItem>{
                    new MenuItem() { Text = "游戏", Icon = "fa fa-fw fa-gamepad", Url = "/tags/index/1" },
                    new MenuItem() { Text = "角色", Icon = "fa fa-fw fa-group", Url = "/tags/index/2" },
                    new MenuItem() { Text = "STAFF", Icon = "fa fa-fw fa-star", Url = "/tags/index/3"},
                    new MenuItem() { Text = "制作组", Icon = "fa fa-fw fa-object-group", Url = "/tags/index/4"}
                } },
                new MenuItem() { Text = "投票", Icon = "fa fa-fw fa-dropbox", Url = "/votes/home" },
                new MenuItem() { Text = "抽奖", Icon = "fa fa-fw fa-gift", Url = "/lotteries/home" },
                new MenuItem() { Text = "数据汇总", Icon = "fa fa-fw fa-line-chart", Url = "/tables/index" },
                new MenuItem() { Text = "编辑指引", Icon = "fa fa-fw fa-map-signs", Url = "/perfections/home" },
                new MenuItem() { Text = "Steam鉴赏家", Icon = "fa fa-fw fa-steam", Url = "/navoutlink?link=https://store.steampowered.com/curator/11627314/",Target="_blank" },
                new MenuItem() { Text = "网店", Icon = "fa fa-fw fa-shopping-cart", Url = "/navoutlink?link=https://shop523081230.taobao.com/",Target="_blank"},
                new MenuItem() { Text = "关于", Icon = "fa fa-fw fa-bolt", Url = "/home/about" },

               //new MenuItem() { Text = "测试", Icon = "fa fa-fw fa-bolt", Url = "/_content/CnGalWebSite.Shared/pages/NotSupported.html" },
            };

            if (user.IsInRole("Admin"))
            {
                menus.Insert(0,
                new MenuItem()
                {
                    IsCollapsed = true,
                    Text = "管理员后台",
                    Icon = "fa fa-fw fa-mortar-board",
                    Url = "/null",
                    Items = new List<MenuItem>{
                              new MenuItem() { Text = "概览", Icon = "fa fa-fw fa-bar-chart-o", Url = "/admin/index" },
                              new MenuItem() { Text = "数据", Icon = "fa fa-fw fa-database", Url = "/admin/data" },
                              new MenuItem() { Text = "审核", Icon = "fa fa-fw fa-pencil", Url = "/admin/ListExamines" },
                              new MenuItem() { Text = "工具", Icon = "fa fa-fw fa-wrench", Url = "/admin/tools" },
                              new MenuItem() { Text = "备份", Icon = "fa fa-fw fa-cloud-upload", Url = "/admin/listbackuparchives" },
                              new MenuItem() { Text = "批量导入", Icon = "fa fa-fw fa-upload", Url = "/admin/importdata" },
                              new MenuItem() { Text = "定时任务", Icon = "fa fa-fw fa-tasks", Url = "/admin/listtimedtasks" },
                              new MenuItem() { Text = "动态采集", Icon = "fa fa-fw fa-newspaper-o", Url = "/admin/listnews" },
                              new MenuItem() { Text = "管理评论", Icon = "fa fa-fw fa-comments-o", Url = "/admin/listcomments" },
                              new MenuItem() { Text = "管理消息", Icon = "fa fa-fw fa-envelope-o", Url = "/admin/listmessages" },
                              new MenuItem() { Text = "管理用户", Icon = "fa fa-fw  fa-user-circle", Url = "/admin/ListUsers" },
                              new MenuItem() { Text = "管理用户角色", Icon = "fa fa-fw fa-street-view", Url = "/admin/ListRoles" },
                              new MenuItem() { Text = "管理词条", Icon = "fa fa-fw  fa-align-center", Url = "/admin/ListEntries" },
                              new MenuItem() { Text = "管理文章", Icon = "fa fa-fw fa-file-text-o", Url = "/admin/ListArticles" },
                              new MenuItem() { Text = "管理标签", Icon = "fa fa-fw fa-tag", Url = "/admin/ListTags" },
                              new MenuItem() { Text = "管理收藏", Icon = "fa fa-fw fa-folder-open", Url = "/admin/ListFavoriteFolders" },
                              new MenuItem() { Text = "管理周边", Icon = "fa fa-fw fa-shopping-basket", Url = "/admin/ListPeripheries" },
                              new MenuItem() { Text = "管理投票", Icon = "fa fa-fw fa-dropbox", Url = "/admin/ListVotes" },
                              new MenuItem() { Text = "管理抽奖", Icon = "fa fa-fw fa-gift", Url = "/admin/ListLotteries" },
                              new MenuItem() { Text = "管理头衔", Icon = "fa fa-fw fa-tree", Url = "/admin/ListRanks" },
                              new MenuItem() { Text = "管理图片", Icon = "fa fa-fw fa-image", Url = "/admin/ListImages" },
                              new MenuItem() { Text = "其他设置", Icon = "fa fa-fw fa-cog", Url = "/admin/ManageHome" }
                    }
                });

            }
            else if (user.IsInRole("Editor"))
            {
                menus.Insert(0,
                new MenuItem()
                {
                    IsCollapsed = false,
                    Text = "编辑者后台",
                    Icon = "fa fa-fw fa-mortar-board",
                    Url = "/null",
                    Items = new List<MenuItem>{
                                new MenuItem() { Text = "动态采集", Icon = "fa fa-fw fa-newspaper-o", Url = "/admin/listnews" }
                    }
                });
            }


            return menus;
        }

        private System.Threading.Timer mytimer;

        public async void Send(object o)
        {
            await InvokeAsync(async () =>
            {
                try
                {
                    var authState = await authenticationStateTask;
                    var user = authState.User;
                    if (user.Identity.IsAuthenticated)
                    {
                        await Http.GetFromJsonAsync<Result>(ToolHelper.WebApiPath + "api/account/MakeUserOnline");
                    }
                }

                catch
                {

                }
            });

        }

        public void OnSearch()
        {
            NavigationManager.NavigateTo("/home/search/全部");
        }

        public void OnReturnHome()
        {
            NavigationManager.NavigateTo("/home/");
        }

        public Task Logout()
        {
            NavigationManager.NavigateTo("/account/logout/");
            return Task.CompletedTask;
        }

        public async Task NavigateToWASM()
        {
            await JS.InvokeAsync<string>("openNewPage", "https://app.cngal.org/");
        }

        public async Task NavigateToSSR()
        {
            await JS.InvokeAsync<string>("openNewPage", "https://www.cngal.org/");
        }

        public async Task GetUserUnreadedMessages()
        {
            if (string.IsNullOrWhiteSpace(userId) == false)
            {
                try
                {
                    UnreadedMessages = await Http.GetFromJsonAsync<UserUnReadedMessagesModel>(ToolHelper.WebApiPath + "api/space/GetUserUnReadedMessages/" + userId);
                    StateHasChanged();
                }
                catch
                {
                    UnreadedMessages = new UserUnReadedMessagesModel();
                }
            }

        }

        public async Task OnClickMessage()
        {
            var authState = await authenticationStateTask;
            var user = authState.User;

            NavigationManager.NavigateTo(Provider, "/space/index/" + userId + "/5", user.Identity.Name, "fa-star-o");
            await OnReadedAllMessage();

            UnreadedMessages = new UserUnReadedMessagesModel();
        }

        public async Task OnReadedAllMessage()
        {
            try
            {
                var obj = await Http.GetFromJsonAsync<Result>(ToolHelper.WebApiPath + "api/space/ReadedAllMessages/");
                //判断结果
                if (obj.Successful == false)
                {
                    await ToastService.Error("使消息已读失败", obj.Error);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ProcessError(ex, "设置消息已读失败");
            }
        }

        public async Task ChangeToAppMode()
        {
            _dataCacheService.IsApp = true;
            await _dataCacheService.RefreshApp.InvokeAsync();
            StateHasChanged();
        }

        #region 释放实例
        private async ValueTask DisposeAsyncCore()
        {
            if (Interop != null)
            {
                await Interop.InvokeVoidAsync(this, null, "bb_layout", "dispose");
                Interop.Dispose();
                Interop = null;
            }
            if (mytimer != null)
            {
                mytimer.Dispose();
                mytimer = null;
            }
        }
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
