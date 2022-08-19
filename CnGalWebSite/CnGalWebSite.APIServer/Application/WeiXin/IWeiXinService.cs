using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.WeiXin
{
    public interface IWeiXinService
    {

        void CreateMenu();

        Task<string> GetNewestPublishGames(bool plainText = false);

        Task<string> GetNewestUnPublishGames(bool plainText = false);

        Task<string> GetNewestNews(bool plainText = false);

        Task<string> GetNewestEditGames(bool plainText = false);

        Task<string> GetEntryInfor(int id, bool plainText = false, bool showLink = false, bool showOutlink = true);

        Task<string> GetRandom(bool plainText = false, bool showLink = false);

        Task<string> GetSearchResults(string text);

        Task<string> GetArticleInfor(int id, bool plainText = false, bool showLink = false);

        string GetAboutUsage();
    }
}
