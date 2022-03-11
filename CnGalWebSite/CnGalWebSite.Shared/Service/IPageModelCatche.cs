using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IPageModelCatche<TModel> where TModel : class
    {
        void Init(string baseUrl, bool useNewtonsoft = false);

        Task<TModel> GetCatche(string apiUrl, bool noRefresh = true);

        void Clean(string apiUrl);

        void Clean();
    }
}
