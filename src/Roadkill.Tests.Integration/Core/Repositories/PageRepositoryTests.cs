using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Marten;
using Npgsql;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable PossibleMultipleEnumeration
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Roadkill.Tests.Integration.Core.Repositories
{
	public class PageRepositoryTests
	{
		private readonly ITestOutputHelper _outputHelper;
		private readonly Fixture _fixture;

		public PageRepositoryTests(ITestOutputHelper outputHelperHelper)
		{
			_outputHelper = outputHelperHelper;
			_fixture = new Fixture();
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(PageRepositoryTests), outputHelperHelper);

			try
			{
				new PageRepository(documentStore).Wipe();
			}
			catch (Exception e)
			{
				outputHelperHelper.WriteLine(GetType().Name + " caught: " + e.ToString());
			}
		}

		public PageRepository CreateRepository()
		{
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(PageRepositoryTests), _outputHelper);

			return new PageRepository(documentStore);
		}

		[Fact]
		public async Task AddNewPage_should_add_page_and_increment_id()
		{
			// given
			string createdBy = "lyon";
			DateTime createdOn = DateTime.Today;

			PageRepository repository = CreateRepository();

			Page page = _fixture.Create<Page>();
			page.Id = -1; // should be reset
			page.CreatedBy = createdBy;
			page.CreatedOn = createdOn;
			page.LastModifiedBy = createdBy;
			page.LastModifiedOn = createdOn;

			// when
			await repository.AddNewPageAsync(page);
			Page actualPage = await repository.AddNewPageAsync(page);

			// then
			actualPage.ShouldNotBeNull();
			actualPage.CreatedOn.ShouldBe(createdOn);
			actualPage.CreatedBy.ShouldBe(createdBy);

			Page savedVersion = await repository.GetPageByIdAsync(actualPage.Id);
			savedVersion.ShouldNotBeNull();
			savedVersion.Id.ShouldBeGreaterThanOrEqualTo(1);
		}

		[Fact]
		public async Task AllPages()
		{
			// given
			PageRepository repository = CreateRepository();
			List<Page> pages = CreateTenPages(repository);

			// when
			IEnumerable<Page> actualPages = await repository.AllPagesAsync();

			// then
			actualPages.Count().ShouldBe(pages.Count);
		}

		[Fact]
		public async Task AllTags_should_return_raw_tags_for_all_pages()
		{
			// given
			PageRepository repository = CreateRepository();
			var pages = _fixture.CreateMany<Page>(10).ToList();
			pages.ForEach(p => p.Tags = "tag1, tag2, tag3");
			CreateTenPages(repository, pages);

			// when
			IEnumerable<string> actualTags = await repository.AllTagsAsync();

			// then
			actualTags.Count().ShouldBe(pages.Count);
			actualTags.First().ShouldBe("tag1, tag2, tag3");
		}

		[Fact]
		public async Task DeletePage_should_delete_specific_page()
		{
			// given
			PageRepository repository = CreateRepository();
			CreateTenPages(repository);

			var pageToDelete = _fixture.Create<Page>();
			await repository.AddNewPageAsync(pageToDelete);

			// when
			await repository.DeletePageAsync(pageToDelete.Id);

			// then
			var deletedPage = await repository.GetPageByIdAsync(pageToDelete.Id);
			deletedPage.ShouldBeNull();
		}

		[Fact]
		public async Task DeleteAllPages_should_clear_pages()
		{
			// given
			PageRepository repository = CreateRepository();
			CreateTenPages(repository);

			// when
			await repository.DeleteAllPagesAsync();

			// then
			IEnumerable<Page> allPages = await repository.AllPagesAsync();
			allPages.ShouldBeEmpty();
		}

		[Fact]
		public async Task FindPagesCreatedBy_should_find_pages_created_by_with_case_insensitive_search()
		{
			// given
			PageRepository repository = CreateRepository();
			CreateTenPages(repository); // add random data

			var page1 = _fixture.Create<Page>();
			var page2 = _fixture.Create<Page>();
			page1.CreatedBy = "myself";
			page2.CreatedBy = "MYSELf";

			await repository.AddNewPageAsync(page1);
			await repository.AddNewPageAsync(page2);

			// when
			IEnumerable<Page> actualPages = await repository.FindPagesCreatedByAsync("myself");

			// then
			actualPages.Count().ShouldBe(2);
			actualPages.First(x => x.Id == page1.Id).ShouldNotBeNull();
			actualPages.First(x => x.Id == page2.Id).ShouldNotBeNull();
		}

		[Fact]
		public async Task FindPagesLastModifiedBy_should_find_pages_with_case_insensitive_search()
		{
			// given
			PageRepository repository = CreateRepository();
			CreateTenPages(repository); // add random pages

			var page1 = _fixture.Create<Page>();
			var page2 = _fixture.Create<Page>();
			page1.LastModifiedBy = "THAT guy";
			page2.LastModifiedBy = "That Guy";

			await repository.AddNewPageAsync(page1);
			await repository.AddNewPageAsync(page2);

			// when
			IEnumerable<Page> actualPages = await repository.FindPagesLastModifiedByAsync("that guy");

			// then
			actualPages.Count().ShouldBe(2);
			actualPages.First(x => x.Id == page1.Id).ShouldNotBeNull();
			actualPages.First(x => x.Id == page2.Id).ShouldNotBeNull();
		}

		[Fact]
		public async Task FindPagesContainingTag_should_find_tags_using_case_insensitive_search()
		{
			// given
			PageRepository repository = CreateRepository();
			CreateTenPages(repository);

			var pages = _fixture.CreateMany<Page>(3).ToList();
			pages.ForEach(p => p.Tags = _fixture.Create<string>() + ", facebook-data-leak");
			await repository.AddNewPageAsync(pages[0]);
			await repository.AddNewPageAsync(pages[1]);
			await repository.AddNewPageAsync(pages[2]);

			// when
			var actualPages = await repository.FindPagesContainingTagAsync("facebook-data-leak");

			// then
			actualPages.Count().ShouldBe(3);
			actualPages.First(x => x.Id == pages[0].Id).ShouldNotBeNull();
			actualPages.First(x => x.Id == pages[1].Id).ShouldNotBeNull();
			actualPages.First(x => x.Id == pages[2].Id).ShouldNotBeNull();
		}

		[Fact]
		public async Task GetPageById_should_find_by_id()
		{
			// given
			PageRepository repository = CreateRepository();
			List<Page> pages = CreateTenPages(repository);

			Page expectedPage = pages[0];

			// when
			Page actualPage = await repository.GetPageByIdAsync(expectedPage.Id);

			// then
			actualPage.ShouldNotBeNull();
			actualPage.ShouldBeEquivalent(expectedPage);
		}

		[Fact]
		public async Task GetPageByTitle_should_match_by_exact_title()
		{
			// given
			PageRepository repository = CreateRepository();
			List<Page> pages = CreateTenPages(repository);

			Page expectedPage = pages[0];

			// when
			Page actualPage = await repository.GetPageByTitleAsync(expectedPage.Title);

			// then
			actualPage.ShouldNotBeNull();
			actualPage.ShouldBeEquivalent(expectedPage);
		}

		[Fact]
		public async Task UpdateExisting_should_save_changes()
		{
			// given
			PageRepository repository = CreateRepository();
			List<Page> pages = CreateTenPages(repository);

			Page expectedPage = pages[0];
			expectedPage.Tags = "new-tags";
			expectedPage.Title = "new title";

			// when
			await repository.UpdateExistingAsync(expectedPage);

			// then
			Page actualPage = await repository.GetPageByIdAsync(expectedPage.Id);
			actualPage.ShouldBeEquivalent(expectedPage);
		}

		private List<Page> CreateTenPages(PageRepository repository, List<Page> pages = null)
		{
			if (pages == null)
			{
				pages = _fixture.CreateMany<Page>(10).ToList();
			}

			var newPages = new List<Page>();
			foreach (Page page in pages)
			{
				Page newPage = repository.AddNewPageAsync(page).GetAwaiter().GetResult();
				newPages.Add(newPage);
			}

			return newPages;
		}

		private void PrintPages()
		{
			using var connection = new NpgsqlConnection(DocumentStoreManager.ConnectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = "delete from public.mt_doc_page";
			command.CommandText = "delete from public.mt_doc_pagecontent";

			command.CommandText = "select count(*) from public.mt_doc_page";
			long result = (long)command.ExecuteScalar();
			_outputHelper.WriteLine("Pages: {0}", result);

			command.CommandText = "select count(*) from public.mt_doc_pagecontent";
			result = (long)command.ExecuteScalar();
			_outputHelper.WriteLine("PageContents: {0}", result);
		}
	}
}
