using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Examines;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Examines
{
    public interface IExamineService
    {
        Task AddExamine(ApplicationUser user, Examine examine);
    }
}
