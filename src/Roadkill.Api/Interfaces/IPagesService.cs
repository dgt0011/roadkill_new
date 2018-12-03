﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Roadkill.Api.Models;

namespace Roadkill.Api.Interfaces
{
	public interface IPagesService
	{
		Task<PageModel> Add(PageModel model);

		Task<PageModel> Update(PageModel model);

		Task Delete(int pageId);

		Task<PageModel> GetById(int id);

		Task<IEnumerable<PageModel>> AllPages();

		Task<IEnumerable<PageModel>> AllPagesCreatedBy(string username);

		Task<PageModel> FindHomePage();

		Task<PageModel> FindByTitle(string title);
	}
}