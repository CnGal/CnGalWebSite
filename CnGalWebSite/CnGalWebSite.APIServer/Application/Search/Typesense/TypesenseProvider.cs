using CnGalWebSite.APIServer.Application.Search.Typesense;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Typesense;
using Typesense.Setup;

namespace CnGalWebSite.APIServer.Application.Typesense
{
    public class TypesenseProvider : ITypesenseProvider
    {
        private readonly IConfiguration _configuration;

        public TypesenseProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ITypesenseClient GetClient()
        {
            var provider = new ServiceCollection()
            .AddTypesenseClient(config =>
            {
                config.ApiKey = _configuration["TypesenseAPIKey"];
                config.Nodes = new List<Node> { new Node(_configuration["TypesenseHost"], _configuration["TypesensePort"]) };
            }).BuildServiceProvider();

            var typesenseClient = provider.GetService<ITypesenseClient>();
            return typesenseClient;
        }
    }
}
