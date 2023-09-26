using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Core
{
    public interface ILive2DService
    {
        Task InitAsync();

        Task SetClothesAsync(int index);

        Task SetExpression(int index);

        Task CleanExpression();

        Task SetMotion(string group, int index);

        Task CleanMotion();
    }
}
