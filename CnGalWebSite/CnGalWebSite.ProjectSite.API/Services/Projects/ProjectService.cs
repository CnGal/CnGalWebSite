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
                BudgetMax = model.Positions.Sum(x => x.BudgetMax),
                BudgetMin = model.Positions.Sum(s => s.BudgetMin),
                Percentage = model.Positions.Average(s => s.Percentage),
                Description = model.Description,
                EndTime = model.EndTime,
                Id = model.Id,
                Name = model.Name,
            };
        }
    }
}
