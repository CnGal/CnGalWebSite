using Typesense;

namespace CnGalWebSite.APIServer.Application.Search.Typesense
{
    public interface ITypesenseProvider
    {
        ITypesenseClient GetClient();
    }
}
