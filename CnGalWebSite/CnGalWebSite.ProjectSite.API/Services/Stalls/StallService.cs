using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Stalls;

namespace CnGalWebSite.ProjectSite.API.Services.Stalls
{
    public class StallService:IStallService
    {
        public StallInfoViewModel GetStallInfoViewModel(Stall model,ApplicationUser user=null)
        {
            return new StallInfoViewModel
            {
                Description = model.Description,
                Id = model.Id,
                Name = model.Name,
                PositionType = model.PositionType,
                Image=model.Images.FirstOrDefault()?.Image,
                PositionTypeName = model.PositionTypeName,
                Price = model.Price,
                Type = model.Type,
                CreateTime = model.CreateTime,
                Priority = model.Priority,
                UpdateTime = model.UpdateTime,
                UserInfo=new Models.ViewModels.Users.UserInfoViewModel
                {
                    Avatar=user?.Avatar??model.CreateUser?.Avatar,
                    Name= user?.PersonName ?? user?.UserName ?? model.CreateUser?.PersonName ?? model.CreateUser?.UserName,
                    Id=user?.Id ?? model.CreateUser?.Id,
                }
            };
        }
    }
}
