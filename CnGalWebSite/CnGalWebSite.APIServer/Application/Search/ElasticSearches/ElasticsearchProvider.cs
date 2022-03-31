using Microsoft.Extensions.Configuration;
using Nest;
using System;

namespace CnGalWebSite.APIServer.Application.Search.ElasticSearches
{
    public class ElasticsearchProvider : IElasticsearchProvider
    {
        private readonly IConfiguration _configuration;

        public ElasticsearchProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IElasticClient GetClient()
        {
            var url = _configuration["ElasticSearchContextUrl"];
            //如果有多个节点以|分开
            //var urls = url.Split('|').Select(x => new Uri(x)).ToList();

            //单个节点
            var connectionSettings = new ConnectionSettings(new Uri(url));
            //多个节点
            //var connectionPool = new SniffingConnectionPool(urls);
            //var connectionSetting = new ConnectionSettings(connectionPool).DefaultIndex("");
            //如果有账号密码
            //connectionSettings.BasicAuthentication(UserName, Password);

            return new ElasticClient(connectionSettings);
        }
    }
}
