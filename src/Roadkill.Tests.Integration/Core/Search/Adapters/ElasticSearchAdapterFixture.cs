using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Elastic.Clients.Elasticsearch;

using Roadkill.Core.Entities;
using Roadkill.Core.Search.Adapters;

namespace Roadkill.Tests.Integration.Core.Search.Adapters
{
	public class ElasticSearchAdapterFixture : IDisposable
	{
		private readonly ElasticsearchClientSettings _settings;

		public ElasticSearchAdapterFixture()
		{
			var uri = new Uri("http://localhost:9200");

			//var connectionPool = new SingleNodeConnectionPool(uri);
			//var connectionSettings = new ConnectionSettings(connectionPool);
			//ElasticClient = new ElasticsearchClient(connectionSettings);

			_settings = new ElasticsearchClientSettings(uri);
			ElasticClient = new ElasticsearchClient(_settings);

			SearchAdapter = new ElasticSearchAdapter(ElasticClient);

			AddDummyData();
		}

		public List<SearchablePage> TestPages { get; private set; }

		public ElasticsearchClient ElasticClient { get; set; }

		public ElasticSearchAdapter SearchAdapter { get; set; }

		public void Dispose()
		{
			//ElasticClient.LowLevel.Indices.Delete<SearchablePageResponse>(ElasticSearchAdapter.PagesIndexName);
			ElasticClient.Indices.DeleteAsync<SearchablePage>(ElasticSearchAdapter.PagesIndexName);
			//_settings.Dispose();
		}

		private void AddDummyData()
		{
			var fixture = new Fixture();

			int id = (int)DateTime.Now.Ticks;
			TestPages = fixture.CreateMany<SearchablePage>(10).ToList();
			foreach (SearchablePage page in TestPages)
			{
				page.PageId = ++id;
				SearchAdapter.Add(page).ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}
	}
}
