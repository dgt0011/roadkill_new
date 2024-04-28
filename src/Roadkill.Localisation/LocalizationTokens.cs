﻿// ReSharper disable CA1802

using System.Diagnostics.CodeAnalysis;

namespace Roadkill.Localisation
{
	/// <summary>
	///     Replaces tokens such as {bold} in the localization text (which breaks the XML resx format) with HTML.
	/// </summary>
	[SuppressMessage("Codecop", "CA1802", Justification = "Localization")]
	[SuppressMessage("Codecop", "CA1052", Justification = "Localization")]
	[SuppressMessage("Codecop", "SA1310", Justification = "Localization")]
	public static class LocalizationTokens
	{
		private static readonly string ADEXPLORERSTART = "{BEGIN_ADEXPLORER_LINK}";
		private static readonly string ADEXPLORER_END = "{END_ADEXPLORER_LINK}";
		private static readonly string RECAPTCHA_START = "{BEGIN_RECAPTCHA_LINK}";
		private static readonly string RECAPTCHA_END = "{END_RECAPTCHA_LINK}";
		private static readonly string MENU_TOKENS = "{MENUTOKENS}";

		private static readonly string ADEXPLORER_REPLACEMENT =
			@"<a href=""http://technet.microsoft.com/en-us/sysinternals/bb963907"" target=""_blank"">";

		private static readonly string RECAPTCHA_REPLACEMENT =
			@"<a href=""https://www.google.com/recaptcha/admin/create"" target=""_blank"">";

		private static readonly string MENU_TOKENS_REPLACEMENT =
			"<br/>%mainpage%<br/>%categories%<br/>%allpages%<br/>%newpage%<br/>%managefiles%<br/>%sitesettings%.";

		private static readonly string END_ANCHOR = "</a>";
		private static readonly string BOLD_START = "{bold}";
		private static readonly string BOLD_END = "{/bold}";
		private static readonly string BR = "{br}";
		private static readonly string HEAD = "{head}";

		public static string ReplaceAdExplorer(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			text = text.Replace(ADEXPLORERSTART, ADEXPLORER_REPLACEMENT);
			text = text.Replace(ADEXPLORER_END, END_ANCHOR);
			return text;
		}

		public static string ReplaceRecaptcha(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			text = text.Replace(RECAPTCHA_START, RECAPTCHA_REPLACEMENT);
			text = text.Replace(RECAPTCHA_END, END_ANCHOR);

			return text;
		}

		public static string ReplaceMenuTokens(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			text = text.Replace(MENU_TOKENS, MENU_TOKENS_REPLACEMENT);
			return text;
		}

		public static string ReplaceHtmlTokens(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			text = text.Replace(BOLD_START, "<b>");
			text = text.Replace(BOLD_END, "</b>");
			text = text.Replace(BR, "<br/>");
			return text;
		}

		public static string ReplaceHeadToken(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;

			text = text.Replace(HEAD, "&lt;head&gt;");
			return text;
		}
	}
}
