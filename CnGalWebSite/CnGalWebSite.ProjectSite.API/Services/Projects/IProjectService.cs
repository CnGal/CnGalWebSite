using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;

namespace CnGalWebSite.ProjectSite.API.Services.Projects
{
    public interface IProjectService
    {
        ProjectPositionInfoViewModel GetProjectPositionInfoViewModel(ProjectPosition model);

        ProjectInfoViewModel GetProjectInfoViewModel(Project model);
    }
}
