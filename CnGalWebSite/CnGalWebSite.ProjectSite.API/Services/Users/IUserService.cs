﻿using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;

namespace CnGalWebSite.ProjectSite.API.Services.Users
{
    public interface IUserService
    {
        Task<ApplicationUser> GetCurrentUserAsync();

        bool CheckCurrentUserRole(string role);

        Task<UserInfoViewModel> GetUserInfo(string id, bool extraInfo = false);

        UserInfoViewModel GetUserInfo(ApplicationUser user, bool extraInfo = false);
    }
}
