using CnGalWebSite.DataModel.ViewModel.Others;
using System.Threading.Tasks;
using System;

namespace CnGalWebSite.APIServer.Application.Charts
{
    public interface IChartService
    {
        Task<LineChartModel> GetLineChartAsync(LineChartType type, DateTime afterTime, DateTime beforeTime);
    }
}
