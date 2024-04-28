using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Search.Adapters
{
	public class ElasticSearchAdapter : ISearchAdapter
	{
		public const string PagesIndexName = "pages";
		private readonly ElasticsearchClient _elasticClient;

		public ElasticSearchAdapter(ElasticsearchClient elasticClient)
		{
			_elasticClient = elasticClient;
		}

		public async Task<bool> Add(SearchablePage page)
		{
			var response = await _elasticClient.IndexAsync(page, idx => idx.Index(PagesIndexName));

			return response.Result == Result.Created;
		}

		public async Task RecreateIndex()
		{
			_ = _elasticClient.Indices.DeleteAsync<SearchablePage>(PagesIndexName);
			_ = await _elasticClient.ReindexAsync();
		}

		public async Task<bool> Update(SearchablePage page)
		{
			var response = await _elasticClient.IndexAsync(page, idx => idx.Index(PagesIndexName));
			return response.Result == Result.Updated;
		}

		public async Task<IEnumerable<SearchablePage>> Find(string query)
		{
			var searchDescriptor = CreateSearchDescriptor(query);
			var response = await _elasticClient.SearchAsync(searchDescriptor);

			return response.Documents.AsEnumerable();
		}
		
		private static SearchRequestDescriptor<SearchablePage> CreateSearchDescriptor(string query)
		{
			return new SearchRequestDescriptor<SearchablePage>()
				.From(0)
				.Size(20)
				.Index(PagesIndexName)
				.Query(q => q.QueryString(qs => qs.Query(query)));
		}
	}
}
