using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using CnGalWebSite.IdentityServer.Models.InputModels.Account;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Account
{
    public class ChangePhoneNumberViewModel : ChangePhoneNumberInputModel
    {
        public List<SelectListItem> Countries { get; } = CountryPhoneNumberHelper.Countries;
    }
}
