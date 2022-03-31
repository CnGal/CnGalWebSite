using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Search.ElasticSearches
{
    public class ElasticsearchBaseService<TEntity> : IElasticsearchBaseService<TEntity> where TEntity : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _index = typeof(TEntity).ToString().Split('.').Last().ToLower();

        public ElasticsearchBaseService(IElasticsearchProvider esClientProvider)
        {
            _elasticClient = esClientProvider.GetClient();
        }

        public string GetIndex()
        {
            return _index;
        }

        public async Task<bool> IndexExistsAsync()
        {
            return (await _elasticClient.Indices.ExistsAsync(_index)).Exists;
        }


        public async Task InsertAsync(TEntity entity)
        {
            //这里可判断是否存在

            var response = await _elasticClient.IndexAsync(entity,
                s => s.Index(_index));

            if (!response.IsValid)
            {
                throw new Exception("新增数据失败:" + response.OriginalException.Message);
            }
        }

        public async Task InsertRangeAsync(IEnumerable<TEntity> entity)
        {
            var bulkRequest = new BulkRequest(_index.ToString())
            {
                Operations = new List<IBulkOperation>()
            };
            var operations = entity.Select(o => new BulkIndexOperation<TEntity>(o)).Cast<IBulkOperation>().ToList();
            bulkRequest.Operations = operations;
            var response = await _elasticClient.BulkAsync(bulkRequest);

            if (!response.IsValid)
            {
                throw new Exception("批量新增数据失败:" + response.OriginalException.Message);
            }
        }

        public async Task UpdateAsync(TEntity entity, string Id)
        {
            var response = await _elasticClient.UpdateAsync<TEntity>(Id, x => x.Index(_index).Doc(entity));
            if (!response.IsValid)
            {
                throw new Exception("更新失败:" + response.OriginalException.Message);
            }
        }

        public async Task DeleteAsync(string Id)
        {
            await _elasticClient.DeleteAsync<TEntity>(Id, x => x.Index(_index));
        }

        public async Task RemoveIndex()
        {
            var exists = await IndexExistsAsync();
            if (!exists)
            {
                return;
            }

            var response = await _elasticClient.Indices.DeleteAsync(_index);

            if (!response.IsValid)
            {
                throw new Exception("删除index失败:" + response.OriginalException.Message);
            }
        }

        public async Task<Tuple<int, IList<TEntity>>> QueryAsync(int page, int limit, string Text, string sort, SortOrder sortOrder, Func<BoolQueryDescriptor<TEntity>, IBoolQuery> field, QueryType type)
        {
            var from = type == QueryType.Index ? page : ((page - 1) * limit);
            var sortDescriptor = new SortDescriptor<dynamic>();
            if (string.IsNullOrEmpty(sort) == false)
            {
                sortDescriptor.Field(sort, sortOrder);
            }

            if (field == null)
            {
                var query = string.IsNullOrWhiteSpace(Text) ?
             await _elasticClient.SearchAsync<TEntity>(x => x.Index(_index)
                                  .From(from)
                                  .Size(limit)
                                 .Sort(s => sortDescriptor))
             : await _elasticClient.SearchAsync<TEntity>(x => x.Index(_index)
                                 .From(from)
                                 .Size(limit)
                                 .Sort(s => sortDescriptor)
                                 .Query(q => q.QueryString(qs => qs
                                        .Query(Text).DefaultOperator(Operator.And))));
                return new Tuple<int, IList<TEntity>>(Convert.ToInt32(query.Total), query.Documents.ToList());
            }
            else
            {
                var query = string.IsNullOrWhiteSpace(Text) ?
             await _elasticClient.SearchAsync<TEntity>(x => x.Index(_index)
                                  .From(from)
                                  .Size(limit)
                                  .Sort(s => sortDescriptor)
                                  .Query(q =>
                                     q.Bool(field)))
             : await _elasticClient.SearchAsync<TEntity>(x => x.Index(_index)
                                 .From(from)
                                 .Size(limit)
                                 .Sort(s => sortDescriptor)
                                 .Query(q =>
                                     q.Bool(field)
                                     && q.QueryString(qs => qs
                                       .Query(Text).DefaultOperator(Operator.And))));
                return new Tuple<int, IList<TEntity>>(Convert.ToInt32(query.Total), query.Documents.ToList());
            }


        }
    }
}
