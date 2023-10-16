using CnGalWebSite.Components.Models;
using CnGalWebSite.Extensions;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Extensions
{
    public static class ProjectExtensions
    {
        public static string GetPositionName(this ProjectPositionViewModel model)
        {
            return (model.PositionType == ProjectPositionType.Other ? model.PositionTypeName : model.PositionType.GetDisplayName()) + " #" + model.Id;
        }

        public static string GetPositionIcon(this ProjectPositionViewModel model)
        {
            return model.PositionType switch
            {
                ProjectPositionType.Music => IconType.Music.ToIconString(),
                ProjectPositionType.Painter => IconType.Style.ToIconString(),
                ProjectPositionType.Programmer => IconType.Programmer.ToIconString(),
                ProjectPositionType.Writer => IconType.Writer.ToIconString(),
                ProjectPositionType.CV => IconType.Dub.ToIconString(),
                ProjectPositionType.Other => IconType.Shape.ToIconString(),
                _ => IconType.Shape.ToIconString()
            };
        }
    }
}
