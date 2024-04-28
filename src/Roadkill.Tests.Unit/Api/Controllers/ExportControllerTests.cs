using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Roadkill.Api.Controllers;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public sealed class ExportControllerTests
	{
		private IPageRepository _pageRepositoryMock;
		private readonly ExportController _exportController;
		private Fixture _fixture;

		public ExportControllerTests()
		{
			_fixture = new Fixture();

			_pageRepositoryMock = Substitute.For<IPageRepository>();
			_exportController = new ExportController(_pageRepositoryMock);
		}

		[Fact]
		public async Task ExportToXml()
		{
			// given
			var actualPages = _fixture.CreateMany<Page>().ToList();

			_pageRepositoryMock.AllPagesAsync()
				.Returns(actualPages);

			var serializer = new XmlSerializer(typeof(List<Page>));

			// when
			ActionResult<string> actionResult = await _exportController.ExportPagesToXml();

			// then
			actionResult.ShouldBeOkObjectResult();

			string actualXml = actionResult.GetOkObjectResultValue();
			var deserializedPages = serializer.Deserialize(new StringReader(actualXml)) as List<Page>;
			deserializedPages.Count.ShouldBe(actualPages.Count);
		}
	}
}
