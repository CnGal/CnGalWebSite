using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Core
{
    public interface ILive2DService
    {
        event Action Live2DInitialized;

        Task InitAsync();

        Task SetModelAsync(int index);

        Task SetExpression(string name);

        Task CleanExpression();

        Task SetClothes(string name);

        Task CleanClothes();

        Task SetStockings(string name);

        Task CleanStockings();

        Task SetShoes(string name);

        Task CleanShoes();

        Task SetMotion(string group, int index);

        Task CleanMotion();

        void ReleaseLive2D();
    }
}
