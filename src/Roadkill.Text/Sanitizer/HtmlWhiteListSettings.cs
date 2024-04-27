using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Roadkill.Text.Sanitizer
{
	public class HtmlWhiteListSettings
	{
		public List<string> AllowedElements { get; set; }

		public List<string> AllowedAttributes { get; set; }
	}
}
