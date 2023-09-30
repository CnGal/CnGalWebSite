using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.Shared.Service
{
    public class Userservice : IUserService
    {
        private readonly NavigationManager _navigationManager;

        public Userservice(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
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
