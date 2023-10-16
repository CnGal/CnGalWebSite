using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.ViewModel.Space;
using Microsoft.Extensions.Configuration;

namespace CnGalWebSite.GameSite.Shared.Services.Users
{
    public class Userservice : IUserService
    {
        private readonly NavigationManager _navigationManager;
        private readonly IHttpService _httpService;
        private readonly IConfiguration _configuration;

        public UserInforViewModel UserInfo { get; private set; } = new UserInforViewModel();

        public event Action UserInfoChanged;


        public Userservice(NavigationManager navigationManager, IHttpService httpService, IConfiguration configuration)
        {
            _navigationManager = navigationManager;
            _httpService = httpService;
            _configuration = configuration;
        }

        public async Task Refresh(string id)
        {
            UserInfo = await _httpService.GetAsync<UserInforViewModel>($"{_configuration["CnGalAPI"]}api/space/GetUserData/{id}");

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
