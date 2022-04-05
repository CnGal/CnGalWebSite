using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.WeiXin
{
    public interface IWeiXinService
    {

        void CreateMenu();

        Task<string> GetNewestPublishGames();

        Task<string> GetNewestUnPublishGames();

        Task<string> GetNewestNews();

        Task<string> GetNewestEditGames();

        Task<string> GetEntryInfor(int id);

        Task<string> GetRandom();

        Task<string> GetSearchResults(string text);

        string GetAboutUsage();
    }
}
