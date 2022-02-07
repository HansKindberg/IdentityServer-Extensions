using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace HansKindberg.IdentityServer.Extensions
{
	public static class StringExtension
	{
		#region Fields

		private const string _colon = ":";

		[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
		private static readonly string _urlEncodedColon = HttpUtility.UrlEncode(_colon).ToLowerInvariant();

		#endregion

		#region Methods

		public static bool TryGetAsAbsoluteUrl(this string value, out Uri url)
		{
			// We need to use UriKind.RelativeOrAbsolute here and not UriKind.Absolute || UriKind.Relative.
			// If we are on Linux, a returnUrl of "/Account", would give an absolute file-path of "file:///Account" with UriKind.Absolute.
			// https://github.com/dotnet/runtime/issues/22718
			// ReSharper disable InvertIf
			if(!string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out url))
			{
				if(url.IsAbsoluteUri || Uri.TryCreate("https://localhost" + value, UriKind.Absolute, out url))
					return true;
			}
			// ReSharper restore InvertIf

			url = null;

			return false;
		}

		[SuppressMessage("Design", "CA1055:URI-like return values should not be strings")]
		public static string UrlDecodeColon(this string value)
		{
			const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

			if(value != null && value.Contains(_urlEncodedColon, comparison))
				value = value.Replace(_urlEncodedColon, _colon, comparison);

			return value;
		}

		[SuppressMessage("Design", "CA1055:URI-like return values should not be strings")]
		public static string UrlEncodeColon(this string value)
		{
			const StringComparison comparison = StringComparison.Ordinal;

			if(value != null && value.Contains(_colon, comparison))
				value = value.Replace(_colon, _urlEncodedColon, comparison);

			return value;
		}

		#endregion
	}
}