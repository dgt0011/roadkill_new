﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Models;

namespace Roadkill.Api.Interfaces
{
	public interface ITagsService
	{
		/// <summary>
		/// Renames a tag by changing all pages that reference the tag to use the new tag name.
		/// </summary>
		Task Rename(string oldTagName, string newTagName);

		/// <summary>
		/// Retrieves a list of all tags in the system.
		/// </summary>
		/// <returns>A <see cref="IEnumerable{TagViewModel}"/> for the tags.</returns>
		Task<IEnumerable<TagModel>> AllTags();

		/// <summary>
		/// Finds all pages with the given tag.
		/// </summary>
		/// <param name="tag">The tag to search for.</param>
		/// <returns>A <see cref="IEnumerable{PageViewModel}"/> of pages tagged with the provided tag.</returns>
		Task<IEnumerable<PageModel>> FindPageWithTag(string tag);
	}
}