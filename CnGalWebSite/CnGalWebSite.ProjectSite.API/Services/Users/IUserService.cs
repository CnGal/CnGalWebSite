using CnGalWebSite.ProjectSite.Models.DataModels;

namespace CnGalWebSite.ProjectSite.API.Services.Users
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUserAsync();

        bool CheckCurrentUserRole(string role);
    }
}
