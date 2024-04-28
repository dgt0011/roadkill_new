using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Repositories
{
	public interface IPageRepository
	{
		Task<Page> AddNewPageAsync(Page page);

		Task<IEnumerable<Page>> AllPagesAsync();

		Task<IEnumerable<string>> AllTagsAsync();

		Task DeletePageAsync(int id);

		Task DeleteAllPagesAsync();

		Task<IEnumerable<Page>> FindPagesCreatedByAsync(string username);

		Task<IEnumerable<Page>> FindPagesLastModifiedByAsync(string username);

		Task<IEnumerable<Page>> FindPagesContainingTagAsync(string tag);

		Task<Page> GetPageByIdAsync(int id);

		// Case insensitive search by page title
		Task<Page> GetPageByTitleAsync(string title);

		Task<Page> UpdateExistingAsync(Page page);
	}

	public class PageRepository : IPageRepository
	{
		private readonly IDocumentStore _store;

		public PageRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task<Page> AddNewPageAsync(Page page)
		{
			page.Id = 0; // reset so it's autoincremented

			using (var session = _store.LightweightSession())
			{
				session.Store(page);

				await session.SaveChangesAsync();
				return page;
			}
		}

		public async Task<IEnumerable<Page>> AllPagesAsync()
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<string>> AllTagsAsync()
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Select(x => x.Tags)
					.ToListAsync();
			}
		}

		public async Task DeletePageAsync(int pageId)
		{
			using (var session = _store.LightweightSession())
			{
				session.Delete<Page>(pageId);
				await session.SaveChangesAsync();
			}
		}

		public async Task DeleteAllPagesAsync()
		{
			using (var session = _store.LightweightSession())
			{
				session.DeleteWhere<Page>(x => true);
				session.DeleteWhere<PageVersion>(x => true);

				await session.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<Page>> FindPagesCreatedByAsync(string username)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Where(x => x.CreatedBy.Equals(username, StringComparison.CurrentCultureIgnoreCase))
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<Page>> FindPagesLastModifiedByAsync(string username)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Where(x => x.LastModifiedBy.Equals(username, StringComparison.CurrentCultureIgnoreCase))
					.ToListAsync();
			}
		}

		public async Task<IEnumerable<Page>> FindPagesContainingTagAsync(string tag)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.Where(x => x.Tags.Contains(tag))
					.ToListAsync();
			}
		}

		public async Task<Page> GetPageByIdAsync(int id)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.FirstOrDefaultAsync(x => x.Id == id);
			}
		}

		public async Task<Page> GetPageByTitleAsync(string title)
		{
			using (var session = _store.QuerySession())
			{
				return await session
					.Query<Page>()
					.FirstOrDefaultAsync(x => x.Title == title);
			}
		}

		public async Task<Page> UpdateExistingAsync(Page page)
		{
			using (var session = _store.LightweightSession())
			{
				session.Update(page);

				await session.SaveChangesAsync();
				return page;
			}
		}

		public void Wipe()
		{
			try
			{
				_store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(Page));
				_store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(PageVersion));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
