﻿using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Roadkill.Api.Controllers;
using Roadkill.Text;
using Roadkill.Text.Models;
using Roadkill.Text.TextMiddleware;
using Shouldly;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration

namespace Roadkill.Tests.Unit.Api.Controllers
{
	public class MarkdownControllerTests
	{
		private readonly Mock<ITextMiddlewareBuilder> _textMiddlewareMock;
		private readonly MarkdownController _markdownController;
		private Fixture _fixture;

		public MarkdownControllerTests()
		{
			_fixture = new Fixture();

			_textMiddlewareMock = new Mock<ITextMiddlewareBuilder>();
			_markdownController = new MarkdownController(_textMiddlewareMock.Object);
		}

		[Fact]
		public async Task ConvertToHtml()
		{
			// given
			string expectedHtml = "<html>";
			string markdown = "a bit of markdown";

			_textMiddlewareMock.Setup(x => x.Execute(markdown))
				.Returns(new PageHtml(expectedHtml));

			// when
			string actualHtml = await _markdownController.ConvertToHtml(markdown);

			// then
			actualHtml.ShouldBe(expectedHtml);
			_textMiddlewareMock.Verify(x => x.Execute(markdown), Times.Once);
		}
	}
}