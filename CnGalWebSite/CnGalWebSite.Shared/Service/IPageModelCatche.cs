using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IPageModelCatche<TModel> where TModel : class
    {
        void Init(string baseUrl);

        Task<TModel> GetCatche(string apiUrl, bool noRefresh = false);

        void Clean(string apiUrl);
    }
}
