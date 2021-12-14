using Nest;

namespace CnGalWebSite.APIServer.Application.ElasticSearches
{
    public interface IElasticsearchProvider
    {
        IElasticClient GetClient();
    }
}
