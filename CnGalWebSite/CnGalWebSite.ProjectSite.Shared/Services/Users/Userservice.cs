using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using CnGalWebSite.Core.Services;

namespace CnGalWebSite.ProjectSite.Shared.Services.Users
{
    public class Userservice : IUserService
    {
        private readonly NavigationManager _navigationManager;
        private readonly IHttpService _httpService;

        public UserInfoViewModel UserInfo { get; private set; } = new UserInfoViewModel();

        public event Action UserInfoChanged;


        public Userservice(NavigationManager navigationManager, IHttpService httpService)
        {
            _navigationManager = navigationManager;
            _httpService = httpService;
        }

        public async Task Refresh()
        {
            UserInfo = await _httpService.GetAsync<UserInfoViewModel>("api/User/GetUserInfo");

            OnUserInfoChanged();
        }

        public void OnUserInfoChanged()
        {
            UserInfoChanged?.Invoke();
        }

        public void Login()
        {
            InteractiveRequestOptions requestOptions = new()
            {
                Interaction = InteractionType.SignIn,
                ReturnUrl = _navigationManager.Uri,
            };


            if (!OperatingSystem.IsBrowser())
            {
                _navigationManager.NavigateTo($"Account/Login?returnUrl={_navigationManager.Uri}", true);
            }
            else
            {
                _navigationManager.NavigateToLogin("authentication/login", requestOptions);
            }
        }

        public void Logout()
        {
            if (!OperatingSystem.IsBrowser())
            {
                _navigationManager.NavigateTo("Account/Logout", true);
            }
            else
            {
                _navigationManager.NavigateToLogout("authentication/logout", "/");
            }
        }
    }
}
