using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Api.Authorization;
using Roadkill.Api.Authorization.Roles;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;

namespace Roadkill.Api.Controllers
{
	[ApiController]
	[ApiVersion("3")]
	[Route("v{version:apiVersion}/[controller]")]
	[AuthorizeWithBearer]
	public class ExportController : ControllerBase
	{
		private readonly IPageRepository _pageRepository;

		public ExportController(IPageRepository pageRepository)
		{
			_pageRepository = pageRepository;
		}

		[HttpPost]
		[Route(nameof(ExportPagesToXml))]
		public async Task<ActionResult<string>> ExportPagesToXml()
		{
			IEnumerable<Page> allPages = await _pageRepository.AllPagesAsync();
			XmlSerializer serializer = new XmlSerializer(typeof(List<Page>));

			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				serializer.Serialize(writer, allPages);
				return Ok(builder.ToString());
			}
		}

		[HttpPost]
		[Route(nameof(ExportPagesVersionsToXml))]
		public Task<ActionResult<string>> ExportPagesVersionsToXml()
		{
			ActionResult<string> result = Ok("todo");

			return Task.FromResult(result);
		}

		[HttpPost]
		[Route(nameof(ExportAsSql))]
		public Task<ActionResult<string>> ExportAsSql()
		{
			ActionResult<string> result = Ok("todo");

			return Task.FromResult(result);
		}

		[HttpPost]
		[Route(nameof(ExportAttachments))]
		public Task<ActionResult<string>> ExportAttachments()
		{
			ActionResult<string> result = Ok("todo");

			return Task.FromResult(result);
		}
	}
}
