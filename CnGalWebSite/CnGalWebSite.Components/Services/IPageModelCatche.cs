namespace CnGalWebSite.Components.Service
{
    public interface IPageModelCatche<TModel> where TModel : class
    {
        void Init(string name, string baseUrl);

        Task<TModel> GetCache(string apiUrl);

        void Clean(string apiUrl);

        void Clean();

        bool Check(string apiUrl);

    }
}
