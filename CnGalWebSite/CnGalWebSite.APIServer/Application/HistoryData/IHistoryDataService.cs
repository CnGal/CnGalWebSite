using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.HistoryData
{
    public interface IHistoryDataService
    {
        Task GenerateZhiHuArticleImportJson();
        Task ImportBgmLink();
    }
}
