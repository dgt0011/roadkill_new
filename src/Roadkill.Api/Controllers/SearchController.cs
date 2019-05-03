﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Common.Models;
using Roadkill.Core.Search.Adapters;

namespace Roadkill.Api.Controllers
{
	[Authorize]
	[Route("[controller]")]
	public class SearchController : ControllerBase
	{
		private readonly ISearchAdapter _searchAdapter;

		public SearchController(ISearchAdapter searchAdapter)
		{
			_searchAdapter = searchAdapter;
		}

		[HttpGet]
		[Route(nameof(Search))]
		public Task<IEnumerable<SearchResponseModel>> Search(string searchText)
		{
			throw new NotImplementedException();
		}

		[HttpPost]
		[Route(nameof(Add))]
		public Task<string> Add(SearchRequestModel searchRequest)
		{
			throw new NotImplementedException();
		}

		[HttpDelete]
		[Route(nameof(Delete))]
		public int Delete(int id)
		{
			throw new NotImplementedException();
		}

		[HttpPut]
		[Route(nameof(Update))]
		public void Update(SearchRequestModel searchRequest)
		{
			throw new NotImplementedException();
		}
	}
}
