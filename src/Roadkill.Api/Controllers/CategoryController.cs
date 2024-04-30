using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Response;
using Roadkill.Api.ObjectConverters;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Api.Authorization.Policies;
using Roadkill.Api.Common.Request;

namespace Roadkill.Api.Controllers
{
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryRepository _categoryRepository;
		private readonly ICategoryObjectsConverter _categoryObjectsConverter;

		public CategoryController(
			ICategoryRepository categoryRepository,
			)
		{
			_categoryRepository = categoryRepository;
		}

		/// <summary>
		/// Gets a single category by its ID.
		/// </summary>
		/// <param name="id">The unique ID of the category to retrieve.</param>
		/// <returns>The category information, or a 404 not found if the category cannot be found.
		/// </returns>
		[HttpGet]
		[AllowAnonymous]
		[Route("{id}")]
		public async Task<ActionResult<CategoryResponse>> Get(int id)
		{
			Category category = await _categoryRepository.GetCategoryByIdAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			return _categoryObjectsConverter.ConvertToCategoryResponse(category);
		}

		/// <summary>
		/// Retrieves all categories in the Roadkill database.
		/// </summary>
		/// <returns>All the categories in the database.
		/// </returns>
		[HttpGet]
		[Route(nameof(AllCategories))]
		[AllowAnonymous]
		public async Task<ActionResult<IEnumerable<CategoryResponse>>> AllCategories()
		{
			IEnumerable<Category> allCategories = await _categoryRepository.AllCategoriesAsync();
			return Ok(allCategories.Select(_categoryObjectsConverter.ConvertToCategoryResponse));
		}

		/// <summary>
		/// Add a category to the database using the provided information. 
		/// </summary>
		/// <param name="categoryRequest">The category information to add.</param>
		/// <returns>A 202 HTTP status with the newly created category, with its generated ID populated.</returns>
		[HttpPost]
		[Authorize(Policy = PolicyNames.AddCategory)]
		public async Task<ActionResult<PageResponse>> Add([FromBody] CategoryRequest categoryRequest)
		{
			Category category = _categoryObjectsConverter.ConvertToCategory(categoryRequest);
			if (category == null)
			{
				//TODO: Not Found is a really shit return status if the convert has failed
				return NotFound();
			}

			Category newCategory = await _categoryRepository.AddNewCategoryAsync(category);
			CategoryResponse response = _categoryObjectsConverter.ConvertToCategoryResponse(newCategory);

			return CreatedAtAction(nameof(Add), nameof(CategoryController), response);
		}

		/// <summary>
		/// Updates an existing category in the database.
		/// </summary>
		/// <param name="categoryRequest">The category details to update, which should include the category id.</param>
		/// <returns>The updated category details, or a 404 not found if the existing category cannot be found</returns>
		[HttpPut]
		[Authorize(Policy = PolicyNames.UpdateCategory)]
		public async Task<ActionResult<CategoryResponse>> Update(CategoryRequest categoryRequest)
		{
			Category category = _categoryObjectsConverter.ConvertToCategory(categoryRequest);
			if (category == null)
			{
				// again, a kinda shit response
				return NotFound();
			}

			await _categoryRepository.UpdateExistingAsync(category);
			return NoContent();
		}

		/// <summary>
		/// Deletes an existing category from the database. This is an administrator-only action.
		/// </summary>
		/// <param name="categoryId">The id of the category to remove.</param>
		/// <returns>A 204 if the category successfully deleted.</returns>
		[HttpDelete]
		[Authorize(Policy = PolicyNames.DeleteCategory)]
		public async Task<ActionResult<string>> Delete(int categoryId)
		{
			await _categoryRepository.DeleteCategoryAsync(categoryId);
			return NoContent();
		}

	}
}
