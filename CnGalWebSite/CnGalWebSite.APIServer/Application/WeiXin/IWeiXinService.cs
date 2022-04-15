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

        Task<string> GetEntryInfor(int id, bool plainText = false, bool showLink = false);

        Task<string> GetRandom(bool plainText = false);

        Task<string> GetSearchResults(string text);

        Task<string> GetArticleInfor(int id, bool plainText = false, bool showLink = false);

        string GetAboutUsage();
    }
}
