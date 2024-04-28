using System.Collections.Generic;
using Ganss.Xss;
using NSubstitute;
using Roadkill.Text;
using Roadkill.Text.Sanitizer;
using Shouldly;
using Xunit;

namespace Roadkill.Tests.Unit.Text.Sanitizer
{
	public class HtmlSanitizerFactoryTests
	{
		[Fact]
		public void should_return_null_when_not_enabled()
		{
			// given
			var textSettings = new TextSettings() { UseHtmlWhiteList = false };
			HtmlSanitizerFactory factory = CreateFactory(textSettings);

			// when
			IHtmlSanitizer sanitizer = factory.CreateHtmlSanitizer();

			// then
			sanitizer.ShouldBeNull();
		}

		[Fact]
		public void should_configure_whitelist_for_sanitizer()
		{
			// given
			var whiteListSettings = new HtmlWhiteListSettings()
			{
				AllowedElements = new List<string> { "StarWarsMarquee" },
				AllowedAttributes = new List<string> { "cheesecake" }
			};

			var whiteListProviderMock = Substitute.For<IHtmlWhiteListProvider>();
			whiteListProviderMock
				.Deserialize()
				.Returns(whiteListSettings);

			HtmlSanitizerFactory factory = CreateFactory(null, whiteListProviderMock);

			// when
			IHtmlSanitizer sanitizer = factory.CreateHtmlSanitizer();

			// then
			sanitizer.ShouldNotBeNull();

			sanitizer.AllowedSchemes.ShouldContain("http");
			sanitizer.AllowedSchemes.ShouldContain("https");
			sanitizer.AllowedSchemes.ShouldContain("mailto");

			sanitizer.AllowedTags.ShouldContain("StarWarsMarquee");
			sanitizer.AllowedAttributes.ShouldContain("cheesecake");
		}

		[Fact]
		public void should_configure_removing_attribute_event_to_ignore_special_tag()
		{
			// given
			HtmlSanitizerFactory factory = CreateFactory();
			const string expectedHtml = "<a href=\"Special:redpage\"></a>";

			// when
			IHtmlSanitizer sanitizer = factory.CreateHtmlSanitizer();

			// then
			string actualHtml = sanitizer.Sanitize(expectedHtml);

			expectedHtml.ShouldBe(actualHtml);
		}

		private static HtmlSanitizerFactory CreateFactory(TextSettings textSettings = null, IHtmlWhiteListProvider whiteListProviderMock = null)
		{
			if (textSettings == null)
			{
				textSettings = new TextSettings() { UseHtmlWhiteList = true };
			}

			if (whiteListProviderMock == null)
			{
				whiteListProviderMock = Substitute.For<IHtmlWhiteListProvider>();
				whiteListProviderMock
					.Deserialize()
					.Returns(new HtmlWhiteListSettings()
					{
						AllowedElements = new List<string>(),
						AllowedAttributes = new List<string>()
					});
			}

			return new HtmlSanitizerFactory(textSettings, whiteListProviderMock);
		}
	}
}
