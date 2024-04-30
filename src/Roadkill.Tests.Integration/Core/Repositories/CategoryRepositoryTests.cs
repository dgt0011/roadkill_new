using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Marten;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;


namespace Roadkill.Tests.Integration.Core.Repositories
{
	public class CategoryRepositoryTests
	{
		private readonly ITestOutputHelper _outputHelper;
		private readonly Fixture _fixture;

		public CategoryRepositoryTests(ITestOutputHelper outputHelperHelper)
		{
			_outputHelper = outputHelperHelper;
			_fixture = new Fixture();
			IDocumentStore documentStore =
				DocumentStoreManager.GetMartenDocumentStore(typeof(CategoryRepositoryTests), outputHelperHelper);

			try
			{
				new PageRepository(documentStore).Wipe();
			}
			catch (Exception e)
			{
				_outputHelper.WriteLine(GetType().Name + " caught: " + e.ToString());
			}
		}

		public CategoryRepository CreateRepository()
		{
			IDocumentStore documentStore =
				DocumentStoreManager.GetMartenDocumentStore(typeof(CategoryRepositoryTests), _outputHelper);

			return new CategoryRepository(documentStore);
		}



		[Fact]
		public async Task AddNewCategory_should_add_category_and_increment_id()
		{
			// given a new Category to add to the repository
			string title = "Testing";
			string description = "A TEsting Category";

			CategoryRepository repository = CreateRepository();

			Category category = _fixture.Create<Category>();
			category.Title = title;
			category.Description = description;

			// when the Category is saved to the repository
			var returnedCategory = await repository.AddNewCategoryAsync(category);

			// then the returned category Id has been set to a non zero value
			returnedCategory.Id.ShouldBeGreaterThan(0);

			// and the category has been saved to the repository
			var storedCategory = await repository.GetCategoryByIdAsync(returnedCategory.Id);

			storedCategory.ShouldNotBeNull();
		}

		[Fact]
		public async Task DeleteCategory_should_delete_specific_page()
		{
			// given a Category is known to exist
			CategoryRepository repository = CreateRepository();

			var categoryToDelete = _fixture.Create<Category>();
			await repository.AddNewCategoryAsync(categoryToDelete);

			// when the known Category is deleted
			await repository.DeleteCategoryAsync(categoryToDelete.Id);

			// then the Category has been deleted
			var deletedCategory = await repository.GetCategoryByIdAsync(categoryToDelete.Id);
			deletedCategory.ShouldBeNull();
		}

		[Fact]
		public async Task DeleteAlCategories_should_clear_categories()
		{
			// given a set of known Categories exist in the data store
			CategoryRepository repository = CreateRepository();
			CreateTenCategories(repository);

			// when all categories are deleted
			await repository.DeleteAllCategoriesAsync();

			// then
			IEnumerable<Category> allCategories = await repository.AllCategoriesAsync();
			allCategories.ShouldBeEmpty();
		}

		[Fact]
		public async Task UpdateExisting_should_save_changes()
		{
			// given
			CategoryRepository repository = CreateRepository();
			List<Category> categories = CreateTenCategories(repository);

			Category expectedCategory = categories[0];
			expectedCategory.Description = "Updated Description for testing";

			// when
			await repository.UpdateExistingAsync(expectedCategory);

			// then
			Category actualCategory = await repository.GetCategoryByIdAsync(expectedCategory.Id);
			actualCategory.ShouldBeEquivalent(expectedCategory);
		}

		private List<Category> CreateTenCategories(CategoryRepository repository, List<Category> categories = null)
		{
			categories ??= _fixture.CreateMany<Category>(10).ToList();

			var newCategories = new List<Category>();

			foreach (Category category in categories)
			{
				Category newCategory = repository.AddNewCategoryAsync(category).GetAwaiter().GetResult();
				newCategories.Add(newCategory);
			}

			return newCategories;
		}
	}
}
