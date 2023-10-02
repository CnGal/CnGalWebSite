using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Stalls;

namespace CnGalWebSite.ProjectSite.API.Services.Stalls
{
    public interface IStallService
    {
        StallInfoViewModel GetStallInfoViewModel(Stall model, ApplicationUser user=null);
    }
}
