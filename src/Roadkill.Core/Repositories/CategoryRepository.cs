using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marten;
using Roadkill.Core.Entities;

namespace Roadkill.Core.Repositories
{
	public interface ICategoryRepository
	{
		Task<Category> AddNewCategoryAsync(Category category);

		Task<Category> UpdateExistingAsync(Category category);

		Task<IEnumerable<Category>> AllCategoriesAsync();

		Task<Category> GetCategoryByIdAsync(int id);

		Task DeleteCategoryAsync(int categoryId);
	}

	public class CategoryRepository : ICategoryRepository
	{
		private readonly IDocumentStore _store;

		public CategoryRepository(IDocumentStore store)
		{
			_store = store ?? throw new ArgumentNullException(nameof(store));
		}

		public async Task<Category> AddNewCategoryAsync(Category category)
		{
			if (category is null)
			{
				throw new ArgumentNullException(nameof(category));
			}

			category.Id = 0; //reset so its autoincremented

			await using var session = _store.LightweightSession();
			session.Store(category);

			await session.SaveChangesAsync();
			return category;
		}

		public async Task<IEnumerable<Category>> AllCategoriesAsync()
		{
			await using var session = _store.QuerySession();
			return await session
				.Query<Category>()
				.ToListAsync();
		}

		public async Task<Category> GetCategoryByIdAsync(int id)
		{
			await using var session = _store.QuerySession();
			return await session
				.Query<Category>()
				.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task<Category> UpdateExistingAsync(Category category)
		{
			await using var session = _store.LightweightSession();
			session.Update(category);

			await session.SaveChangesAsync();
			return category;
		}

		public async Task DeleteCategoryAsync(int categoryId)
		{
			await using var session = _store.LightweightSession();
			session.Delete<Category>(categoryId);

			//TODO: FInd any Page with the Category and reset to null
			//		Alternatively - should that be done at a 'higher' level?

			await session.SaveChangesAsync();
		}

		public async Task DeleteAllCategoriesAsync()
		{
			await using var session = _store.LightweightSession();
			session.DeleteWhere<Category>(x => true);

			//TODO: Update the Pages to remove the Categories

			await session.SaveChangesAsync();
		}

		public void Wipe()
		{
			try
			{
				_store.Advanced.Clean.DeleteDocumentsByTypeAsync(typeof(Category));
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
