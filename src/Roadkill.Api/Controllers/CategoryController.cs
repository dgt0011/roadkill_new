using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

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
	}
}
