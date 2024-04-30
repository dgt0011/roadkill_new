using System;
using AutoMapper;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Entities;

namespace Roadkill.Api.ObjectConverters
{
	public interface ICategoryObjectsConverter
	{
		CategoryResponse ConvertToCategoryResponse(Category category);

		Category ConvertToToCategory(CategoryRequest request);
	}

	public class CategoryObjectsConverter : ICategoryObjectsConverter
	{
		private readonly IMapper _mapper;

		public CategoryObjectsConverter(IMapper mapper)
		{
			_mapper = mapper;
		}

		public CategoryResponse ConvertToCategoryResponse(Category category)
		{
			if (category is null)
			{
				throw new ArgumentNullException(nameof(category));
			}

			var categoryResponse = _mapper.Map<CategoryResponse>(category);
			return categoryResponse;
		}

		public Category ConvertToToCategory(CategoryRequest request)
		{
			return _mapper.Map<Category>(request);
		}
	}
}
