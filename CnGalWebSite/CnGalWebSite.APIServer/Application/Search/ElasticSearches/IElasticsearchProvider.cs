using Nest;

namespace CnGalWebSite.APIServer.Application.Search.ElasticSearches
{
    public interface IElasticsearchProvider
    {
        IElasticClient GetClient();
    }
}
