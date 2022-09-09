using System;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IPageModelCatche<TModel> where TModel : class
    {
        void Init(string name, string baseUrl, bool useNewtonsoft = false);

        Task<TModel> GetCache(string apiUrl);

        void Clean(string apiUrl);

        void Clean();

        bool Check(string apiUrl);

    }
}
