using Ganss.Xss;
using Roadkill.Text.Models;
using Roadkill.Text.Sanitizer;

namespace Roadkill.Text.TextMiddleware
{
	public class HarmfulTagMiddleware : Middleware
	{
		private readonly IHtmlSanitizer _sanitizer;

		public HarmfulTagMiddleware(IHtmlSanitizerFactory htmlSanitizerFactory)
		{
			_sanitizer = htmlSanitizerFactory?.CreateHtmlSanitizer();
		}

		public override PageHtml Invoke(PageHtml pageHtml)
		{
			if (_sanitizer != null && pageHtml != null)
			{
				pageHtml.Html = _sanitizer.Sanitize(pageHtml.Html);
			}

			return pageHtml;
		}
	}
}
