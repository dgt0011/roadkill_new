using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoMapper;
using Roadkill.Api.Common.Request;
using Roadkill.Api.Common.Response;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.ObjectConverters
{
	public interface IPageObjectsConverter
	{
		PageResponse ConvertToPageResponse(Page page);

		Page ConvertToPage(PageRequest pageRequest);
	}

	public class PageObjectsConverter : IPageObjectsConverter
	{
		private readonly IMapper _mapper;
		private readonly ICategoryRepository _categoryRepository;

		public PageObjectsConverter(IMapper mapper, ICategoryRepository categoryRepository)
		{
			_mapper = mapper;
			_categoryRepository = categoryRepository;
		}

		public PageResponse ConvertToPageResponse(Page page)
		{
			if (page is null)
			{
				throw new ArgumentNullException(nameof(page));
			}

			var pageResponse = _mapper.Map<PageResponse>(page);
			pageResponse.SeoFriendlyTitle = CreateSeoFriendlyPageTitle(page.Title);
			pageResponse.TagList = TagsToList(page.Tags);
			pageResponse.Category = CategoryIdToCategory(page.CategoryId);

			return pageResponse;
		}

		public Page ConvertToPage(PageRequest pageRequest)
		{
			return _mapper.Map<Page>(pageRequest);
		}

		private static IEnumerable<string> TagsToList(string csvTags)
		{
			var tagList = new List<string>();
			char delimiter = ',';

			if (!string.IsNullOrEmpty(csvTags))
			{
				// For the legacy tag separator format
				if (csvTags.IndexOf(";", StringComparison.Ordinal) != -1)
				{
					delimiter = ';';
				}

				if (csvTags.IndexOf(delimiter, StringComparison.Ordinal) != -1)
				{
					string[] parts = csvTags.Split(delimiter);
					foreach (string item in parts)
					{
						if (item != "," && !string.IsNullOrWhiteSpace(item))
						{
							tagList.Add(item.Trim());
						}
					}
				}
				else
				{
					tagList.Add(csvTags.TrimEnd());
				}
			}

			return tagList;
		}

		private static string CreateSeoFriendlyPageTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				return title;
			}

			// Search engine friendly slug routine with help from http://www.intrepidstudios.com/blog/2009/2/10/function-to-generate-a-url-friendly-string.aspx

			// remove invalid characters
			title = Regex.Replace(title, @"[^\w\d\s-]", "");  // this is unicode safe, but may need to revert back to 'a-zA-Z0-9', need to check spec

			// convert multiple spaces/hyphens into one space
			title = Regex.Replace(title, @"[\s-]+", " ").Trim();

			// If it's over 30 chars, take the first 30.
			title = title[..(title.Length <= 75 ? title.Length : 75)].Trim();

			// hyphenate spaces
			title = Regex.Replace(title, @"\s", "-");

			return title;
		}

		private Category CategoryIdToCategory(int? categoryId)
		{
			return categoryId == null ? null : _categoryRepository.GetCategoryByIdAsync(categoryId.Value).Result;
		}
	}
}
