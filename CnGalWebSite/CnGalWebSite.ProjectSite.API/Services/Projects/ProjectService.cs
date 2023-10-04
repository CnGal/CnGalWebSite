using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;

namespace CnGalWebSite.ProjectSite.API.Services.Projects
{
    public class ProjectService:IProjectService
    {
        private readonly IRepository<Project, long> _projectRepository;

        public ProjectService(IRepository<Project, long> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public ProjectInfoViewModel GetProjectInfoViewModel(Project model)
        {
            return new ProjectInfoViewModel
            {
                BudgetMax = model.Positions.Any()? model.Positions.Sum(x => x.BudgetMax):0,
                BudgetMin = model.Positions.Any() ? model.Positions.Sum(s => s.BudgetMin) : 0,
                Percentage = model.Positions.Any() ? model.Positions.Average(s => s.Percentage) : 0,
                Description = model.Description,
                PositionCount=model.Positions.Count,
                EndTime = model.EndTime,
                Id = model.Id,
                CreateTime = model.CreateTime,
                Priority = model.Priority,
                UpdateTime = model.UpdateTime,
                Name = model.Name,
                UserInfo = new Models.ViewModels.Users.UserInfoViewModel
                {
                    Avatar = model.CreateUser?.Avatar,
                    Name =model.CreateUser?.OrganizationName ?? model.CreateUser?.UserName,
                    Id = model.CreateUser?.Id,
                }
            };
        }

        public ProjectPositionInfoViewModel GetProjectPositionInfoViewModel(ProjectPosition model)
        {
            return new ProjectPositionInfoViewModel
            {
                BudgetMax = model.BudgetMax,
                BudgetMin= model.BudgetMin,
                BudgetType = model.BudgetType,
                Percentage= model.Percentage,
                PositionType = model.PositionType,
                PositionTypeName = model.PositionTypeName,
                UrgencyType = model.UrgencyType,
                Type=model.Type,
                Description = model.Description,
                Id = model.Project.Id,
                Tags=model.Tags,
                CreateTime = model.CreateTime,
                Priority = model.Priority,
                UpdateTime = model.UpdateTime,
                UserInfo = new Models.ViewModels.Users.UserInfoViewModel
                {
                    Avatar = model.Project.CreateUser?.Avatar,
                    Name = model.Project.CreateUser?.OrganizationName ?? model.Project.CreateUser?.UserName,
                    Id = model.Project.CreateUser?.Id,
                }
            };
        }
    }
}
